using PhantomBrigade;
using PhantomBrigade.Data;
using PhantomBrigade.Game;
using System;
using System.Collections.Generic;
using UnityEngine;

// 3todo.notes: DataHelperLoading.cs has LoadingStart(string key, string saveLocation); calls DataManagerSave.LoadData(saveLocation).
//   DataHelperLoading.cs: TryLoading(string key, SaveLocation saveLocation, Action callbackOnEnd = null, bool keepScreenAfterLoading = false, bool isScheduleOverworldAudio = true)
//     calls its private helper: DataHelperLoading.LoadingStart(key, saveLocation);
//        which calls (into DataManagerSave.cs): DataManagerSave.SetSaveName(key); DataManagerSave.LoadData(saveLocation);
//        and calls Co.Delay(CIViewBackgroundLoading.ins.hideableMainHolder.duration, new Action(DataHelperLoading.LoadingOverworld));
//           which invokes (causes to be invoked) new Action(DataHelperLoading.LoadingOverworld2)
//              which calls DataManagerSave.TryLoadingOverworldEntities()
//                 which is about 1400 lines and does a bunch of stuff, including a call at the end to DataHelperLoading.ContinueLoadingWorld(flag || flag5, flag2);
//                    which either calls DataHelperLoading.LoadingEnd() if it was the final stage, or continues on with DataHelperLoading.ContinueLoadingBase(finalStageIsBase);
//                       which either calls DataHelperLoading.LoadingEnd() if it was the final stage, or continues on with DataHelperLoading.ContinueLoadingCombat();
//                          which does its thing then queues up Co.DelayFrames(10, new Action(DataHelperLoading.LoadingEnd));

namespace LiveryGUIMod {

    public static class LiverySetsDB {
        // sentinel value meaning "transparent / do not override the mech's current livery for this part"
        public const string PILOT_TRANSPARENT = "__PILOT_TRANSPARENT__";

        // Pilot unique id (string) -> assignment set
        static readonly Dictionary<string, LiveryAssignmentSet> _pilotLiveries = new Dictionary<string, LiveryAssignmentSet>(StringComparer.Ordinal);

        // Mech instance id (int) -> assignment set (used to store/restore original mech liveries while a pilot is re/assigned)
        static readonly Dictionary<int, LiveryAssignmentSet> _mechOriginalLiveries = new Dictionary<int, LiveryAssignmentSet>();
        static readonly Dictionary<string, LiveryAssignmentSet> _pendingMechOriginalLiveriesByName = new Dictionary<string, LiveryAssignmentSet>(StringComparer.Ordinal);

        static readonly HashSet<EquipmentEntity> emptyEquipmentList = new HashSet<EquipmentEntity>();

        // Builds the part-key for use in LiverySetsDB's Dictionary objects.
        public static string PartKey(string socket, string hardpoint) {
            // NOTE: The top-level mech livery will not have a socket/hardpoint, so will use that <nosocket>:<nohardpoint> as its key.
            // And the mid-level part like whole-left-arm will look like optional_left:<nohardpoint>.
            // Versus sub-part like optional_left:external_arm_lower.
            return (socket ?? "<nosocket>") +":" + (hardpoint ?? "<nohardpoint>");
        }

        // Apply the desired livery-set to the mech in the game world. If includePilotOverlay is true,
        // try to overlay any pilot-specific assignments on top of the mech-original set.
        public static void ApplyLiverySetToMech(int mechInstanceId, bool includeMechLiverySet, bool includePilotOverlay, string pilotIdOverride = null, bool updateLoadoutPreview = true) {
            if (mechInstanceId < 0) return;

            PersistentEntity unit = IDUtility.GetPersistentEntity(mechInstanceId);
            if (unit == null) return;
            bool shouldUpdateLoadoutPreview = updateLoadoutPreview && CIViewBaseCustomizationRoot.ins != null && CIViewBaseLoadout.selectedUnitID == mechInstanceId;

            // ensure we have a base mech-original set; capture if missing
            // 3todo.later consider if this might be dead code
            if (!TryGetMechOriginalSet(mechInstanceId, out var baseSet)) {
                CaptureEntireMechLiverySet(mechInstanceId);
                baseSet = GetOrCreateMechOriginalSet(mechInstanceId);
            }

            // start with a copy of base
            var finalSet = new LiveryAssignmentSet();
            if (includeMechLiverySet) {
                MergeInto(finalSet, baseSet);
            }
            else {
                AddTransparentAssignmentsForCurrentMechParts(finalSet, unit);
            }

            if (includePilotOverlay) {
                string pilotId = string.IsNullOrEmpty(pilotIdOverride) ? ResolvePilotIdForMech(mechInstanceId) : pilotIdOverride;
                if (!string.IsNullOrEmpty(pilotId) && TryGetPilotSet(pilotId, out var pilotSet)) {
                    // overlay: only apply pilot entries that are not the transparent sentinel
                    PruneBaseDescendantsHiddenByPilotAssignments(finalSet, pilotSet);
                    foreach (var kv in pilotSet.Assignments) {
                        if (kv.Value == null) continue;
                        if (string.Equals(kv.Value, PILOT_TRANSPARENT, StringComparison.Ordinal)) continue;
                        finalSet.SetForPart(kv.Key, kv.Value);
                    }
                }
            }

            // Apply finalSet to existing entities on the mech. Only touch parts/subsystems that are present.
            // top-level
            if (finalSet.Assignments.TryGetValue(PartKey(null, null), out var topLivery)) {
                if (string.IsNullOrEmpty(topLivery) || string.Equals(topLivery, PILOT_TRANSPARENT, StringComparison.Ordinal)) {
                    if (unit.hasDataKeyEquipmentLivery) unit.RemoveDataKeyEquipmentLivery();
                } 
                else
                    unit.ReplaceDataKeyEquipmentLivery(topLivery);
                if (shouldUpdateLoadoutPreview)
                    CIViewBaseCustomizationRoot.ins.UpdateUnitLivery(string.IsNullOrEmpty(topLivery) || string.Equals(topLivery, PILOT_TRANSPARENT, StringComparison.Ordinal) ? null : topLivery);
            }

            var parts = EquipmentUtility.GetPartsInUnit(unit) ?? emptyEquipmentList;
            foreach (var part in parts) {
                if (part == null) continue;
                string socket = part.partParentUnit.socket;
                string partKey = PartKey(socket, null);
                if (finalSet.Assignments.TryGetValue(partKey, out var pLivery)) {
                    if (string.IsNullOrEmpty(pLivery) || string.Equals(pLivery, PILOT_TRANSPARENT, StringComparison.Ordinal)) {
                        if (part.hasDataKeyEquipmentLivery) part.RemoveDataKeyEquipmentLivery();
                    }
                    else
                        part.ReplaceDataKeyEquipmentLivery(pLivery);
                    if (shouldUpdateLoadoutPreview)
                        CIViewBaseCustomizationRoot.ins.UpdatePartLivery(socket, string.IsNullOrEmpty(pLivery) || string.Equals(pLivery, PILOT_TRANSPARENT, StringComparison.Ordinal) ? null : pLivery);
                }

                var subs = EquipmentUtility.GetSubsystemsInPart(part) ?? emptyEquipmentList;
                foreach (var sub in subs) {
                    if (sub == null) continue;
                    string hardpoint = sub.hasSubsystemParentPart ? sub.subsystemParentPart.hardpoint : null;
                    string subKey = PartKey(socket, hardpoint);
                    if (finalSet.Assignments.TryGetValue(subKey, out var sLivery)) {
                        if (string.IsNullOrEmpty(sLivery) || string.Equals(sLivery, PILOT_TRANSPARENT, StringComparison.Ordinal)) {
                            if (sub.hasDataKeyEquipmentLivery) sub.RemoveDataKeyEquipmentLivery();
                        }
                        else
                            sub.ReplaceDataKeyEquipmentLivery(sLivery);
                        if (shouldUpdateLoadoutPreview)
                            CIViewBaseCustomizationRoot.ins.UpdateSubsystemLivery(socket, hardpoint, string.IsNullOrEmpty(sLivery) || string.Equals(sLivery, PILOT_TRANSPARENT, StringComparison.Ordinal) ? null : sLivery);
                    }
                }
            }
        }

        static void PruneBaseDescendantsHiddenByPilotAssignments(LiveryAssignmentSet finalSet, LiveryAssignmentSet pilotSet) {
            if (finalSet == null || pilotSet == null)
                return;

            foreach (var kv in pilotSet.Assignments) {
                if (!IsConcreteLiveryKey(kv.Value))
                    continue;

                ClearNonTransparentDescendantAssignments(finalSet, kv.Key);
            }
        }

        static void ClearNonTransparentDescendantAssignments(LiveryAssignmentSet set, string ancestorPartKey) {
            if (set == null || string.IsNullOrEmpty(ancestorPartKey))
                return;

            var keysToClear = new List<string>();
            foreach (var kv in set.Assignments) {
                if (IsConcreteLiveryKey(kv.Value) && IsDescendantPartKey(ancestorPartKey, kv.Key))
                    keysToClear.Add(kv.Key);
            }

            foreach (string key in keysToClear)
                set.SetForPart(key, PILOT_TRANSPARENT);
        }

        static bool IsDescendantPartKey(string ancestorPartKey, string candidatePartKey) {
            if (string.IsNullOrEmpty(candidatePartKey) || string.Equals(ancestorPartKey, candidatePartKey, StringComparison.Ordinal))
                return false;

            if (!TrySplitPartKey(ancestorPartKey, out string ancestorSocket, out string ancestorHardpoint))
                return false;

            if (!TrySplitPartKey(candidatePartKey, out string candidateSocket, out string candidateHardpoint))
                return false;

            bool ancestorIsUnit = ancestorSocket == "<nosocket>" && ancestorHardpoint == "<nohardpoint>";
            if (ancestorIsUnit)
                return true;

            bool ancestorIsSocket = ancestorHardpoint == "<nohardpoint>";
            if (ancestorIsSocket)
                return ancestorSocket == candidateSocket && candidateHardpoint != "<nohardpoint>";

            return false;
        }

        static bool TrySplitPartKey(string partKey, out string socket, out string hardpoint) {
            socket = null;
            hardpoint = null;

            int separatorIndex = string.IsNullOrEmpty(partKey) ? -1 : partKey.IndexOf(':');
            if (separatorIndex < 0)
                return false;

            socket = partKey.Substring(0, separatorIndex);
            hardpoint = partKey.Substring(separatorIndex + 1);
            return true;
        }

        static bool IsConcreteLiveryKey(string liveryKey) {
            return !string.IsNullOrEmpty(liveryKey) && !string.Equals(liveryKey, PILOT_TRANSPARENT, StringComparison.Ordinal);
        }

        static void AddTransparentAssignmentsForCurrentMechParts(LiveryAssignmentSet set, PersistentEntity unit) {
            if (set == null || unit == null)
                return;

            set.SetForPart(PartKey(null, null), PILOT_TRANSPARENT);

            var parts = EquipmentUtility.GetPartsInUnit(unit) ?? emptyEquipmentList;
            foreach (EquipmentEntity part in parts) {
                if (part == null)
                    continue;

                string socket = part.partParentUnit.socket;
                set.SetForPart(PartKey(socket, null), PILOT_TRANSPARENT);

                var subsystems = EquipmentUtility.GetSubsystemsInPart(part) ?? emptyEquipmentList;
                foreach (EquipmentEntity subsystem in subsystems) {
                    if (subsystem == null)
                        continue;

                    string hardpoint = subsystem.hasSubsystemParentPart ? subsystem.subsystemParentPart.hardpoint : null;
                    set.SetForPart(PartKey(socket, hardpoint), PILOT_TRANSPARENT);
                }
            }
        }

        // Capture the entire current livery assignment map for a mech and store as the mech-original set.
        static void CaptureEntireMechLiverySet(int mechId) {
            try {
                PersistentEntity unit = IDUtility.GetPersistentEntity(mechId);
                if (unit == null) return;

                var set = GetOrCreateMechOriginalSet(mechId);

                // Top-level unit livery
                string topLivery = unit.hasDataKeyEquipmentLivery ? unit.dataKeyEquipmentLivery.s : null;
                set.SetForPart(PartKey(null, null), string.IsNullOrEmpty(topLivery) ? PILOT_TRANSPARENT : topLivery);

                // Per-part (sockets)
                var parts = EquipmentUtility.GetPartsInUnit(unit) ?? emptyEquipmentList;
                foreach (var part in parts) {
                    if (part == null) continue;
                    string socket = part.partParentUnit.socket;
                    string partLivery = part.hasDataKeyEquipmentLivery ? part.dataKeyEquipmentLivery.s : null;
                    set.SetForPart(PartKey(socket, null), string.IsNullOrEmpty(partLivery) ? PILOT_TRANSPARENT : partLivery);

                    // Subsystems (hardpoints)
                    var subsystems = EquipmentUtility.GetSubsystemsInPart(part) ?? emptyEquipmentList;
                    foreach (var sub in subsystems) {
                        if (sub == null) continue;
                        string hardpoint = sub.hasSubsystemParentPart ? sub.subsystemParentPart.hardpoint : null;
                        string subLivery = sub.hasDataKeyEquipmentLivery ? sub.dataKeyEquipmentLivery.s : null;
                        set.SetForPart(PartKey(socket, hardpoint), string.IsNullOrEmpty(subLivery) ? PILOT_TRANSPARENT : subLivery);
                    }
                }

                Debug.Log($"[LiveryGUI] Dev-Spam: Captured full mech livery set for mech={mechId}, entries={set.Assignments.Count}");
                DebugLogLiverySetDictionaries();
            }
            catch (Exception ex) {
                Debug.LogError($"[LiveryGUI] CaptureEntireMechLiverySet failed: {ex}");
            }
        }

        // Retrieve existing set for pilot or create a new empty one.
        public static LiveryAssignmentSet GetOrCreatePilotSet(string pilotId) {
            if (string.IsNullOrEmpty(pilotId)) {
                Debug.LogWarning("[PilotLiveryStore] GetOrCreatePilotSet called with null/empty pilotId");
                return null;
            }

            if (!_pilotLiveries.TryGetValue(pilotId, out var set)) {
                set = new LiveryAssignmentSet();
                _pilotLiveries[pilotId] = set;
            }
            return set;
        }

        // Try get, don't create
        public static bool TryGetPilotSet(string pilotId, out LiveryAssignmentSet set) {
            set = null;
            if (string.IsNullOrEmpty(pilotId))
                return false;
            return _pilotLiveries.TryGetValue(pilotId, out set);
        }

        // Set a single part assignment for a pilot. Creates the set if necessary.
        public static void SetPilotLivery(string pilotId, string partKey, string liveryKey) {
            if (string.IsNullOrEmpty(pilotId)) {
                Debug.LogWarning("[PilotLiveryStore] SetPilotLivery called with null/empty pilotId");
                return;
            }

            var set = GetOrCreatePilotSet(pilotId);
            set.SetForPart(partKey, liveryKey);
        }

        // Remove a pilot's entire set
        public static bool RemovePilotSet(string pilotId) {
            if (string.IsNullOrEmpty(pilotId))
                return false;
            return _pilotLiveries.Remove(pilotId);
        }

        // Clear all pilot entries
        public static void ClearAllPilotSets() {
            _pilotLiveries.Clear();
        }

        // Enumerate known pilot ids
        public static IEnumerable<string> GetAllPilotIds() {
            return _pilotLiveries.Keys;
        }

        // --- mech original livery helpers ---
        public static LiveryAssignmentSet GetOrCreateMechOriginalSet(int mechInstanceId) {
            if (!_mechOriginalLiveries.TryGetValue(mechInstanceId, out var set)) {
                set = new LiveryAssignmentSet();
                _mechOriginalLiveries[mechInstanceId] = set;
            }
            return set;
        }

        public static bool TryGetMechOriginalSet(int mechInstanceId, out LiveryAssignmentSet set) {
            return _mechOriginalLiveries.TryGetValue(mechInstanceId, out set);
        }

        public static void SetMechOriginalLivery(int mechInstanceId, string partKey, string liveryKey) {
            var set = GetOrCreateMechOriginalSet(mechInstanceId);
            set.SetForPart(partKey, liveryKey);
        }

        public static bool RemoveMechOriginalSet(int mechInstanceId) {
            return _mechOriginalLiveries.Remove(mechInstanceId);
        }

        public static void ClearAllMechOriginalSets() {
            _mechOriginalLiveries.Clear();
        }

        public static IEnumerable<int> GetAllMechIds() {
            return _mechOriginalLiveries.Keys;
        }

        public static Dictionary<string, LiveryAssignmentSet> ExportMechOriginalSetsByUnitName() {
            EnsureMechOriginalSetsForCurrentUnits();

            var result = new Dictionary<string, LiveryAssignmentSet>(StringComparer.Ordinal);
            foreach (var kv in _mechOriginalLiveries) {
                PersistentEntity unit = IDUtility.GetPersistentEntity(kv.Key);
                if (unit == null || !unit.hasNameInternal || string.IsNullOrEmpty(unit.nameInternal.s) || kv.Value == null)
                    continue;

                result[unit.nameInternal.s] = SanitizeForSave(kv.Value);
            }
            return result;
        }

        public static Dictionary<string, LiveryAssignmentSet> ExportPilotSetsByPilotName() {
            EnsurePilotSetsForCurrentPilots();

            var result = new Dictionary<string, LiveryAssignmentSet>(StringComparer.Ordinal);
            foreach (PersistentEntity pilot in GetCurrentPilots()) {
                string pilotName = pilot.nameInternal.s;
                if (!TryGetPilotSet(pilotName, out var set))
                    set = new LiveryAssignmentSet();

                result[pilotName] = SanitizeForSave(set);
            }
            return result;
        }

        public static void ImportSaveGameLiverySets(Dictionary<string, LiveryAssignmentSet> unitSetsByName, Dictionary<string, LiveryAssignmentSet> pilotSetsByName) {
            _mechOriginalLiveries.Clear();
            _pilotLiveries.Clear();
            _pendingMechOriginalLiveriesByName.Clear();

            if (unitSetsByName != null) {
                foreach (var kv in unitSetsByName) {
                    if (string.IsNullOrEmpty(kv.Key) || kv.Value == null)
                        continue;

                    _pendingMechOriginalLiveriesByName[kv.Key] = SanitizeLoadedSet(kv.Value);
                }
            }

            if (pilotSetsByName != null) {
                foreach (var kv in pilotSetsByName) {
                    if (string.IsNullOrEmpty(kv.Key) || kv.Value == null)
                        continue;

                    _pilotLiveries[kv.Key] = SanitizeLoadedSet(kv.Value);
                }
            }
        }

        public static void InitializeSaveGameLiverySetsAfterEntityLoad() {
            int loadedUnitCount = 0;
            foreach (var kv in _pendingMechOriginalLiveriesByName) {
                PersistentEntity unit = IDUtility.GetPersistentEntity(kv.Key);
                if (unit == null || !unit.hasId || kv.Value == null)
                    continue;

                _mechOriginalLiveries[unit.id.id] = kv.Value.Clone();
                loadedUnitCount++;
            }
            _pendingMechOriginalLiveriesByName.Clear();

            EnsureMechOriginalSetsForCurrentUnits();
            EnsurePilotSetsForCurrentPilots();
            ApplyAllCurrentUnitLiverySets();

            Debug.Log($"[LiveryGUI] Initialized save-game livery sets after entity load: loadedUnits={loadedUnitCount}, runtimeUnits={_mechOriginalLiveries.Count}, pilots={_pilotLiveries.Count}");
        }

        // Utility: merge (copy) a source set into a destination set, overwriting per-part keys.
        public static void MergeInto(LiveryAssignmentSet destination, LiveryAssignmentSet source) {
            if (destination == null || source == null) return;
            foreach (var kv in source.Assignments)
                destination.SetForPart(kv.Key, kv.Value);
        }

        public static string NormalizeLiveryKey(string liveryKey) {
            return string.IsNullOrEmpty(liveryKey) ? PILOT_TRANSPARENT : liveryKey;
        }

        public static bool IsPilotLiveryResponsibleForPart(int mechInstanceId, string partKey, string pilotIdOverride = null) {
            if (mechInstanceId < 0 || string.IsNullOrEmpty(partKey))
                return false;

            string pilotId = string.IsNullOrEmpty(pilotIdOverride) ? ResolvePilotIdForMech(mechInstanceId) : pilotIdOverride;
            if (string.IsNullOrEmpty(pilotId) || !TryGetPilotSet(pilotId, out var pilotSet))
                return false;

            foreach (var kv in pilotSet.Assignments) {
                if (!IsConcreteLiveryKey(kv.Value))
                    continue;

                if (string.Equals(kv.Key, partKey, StringComparison.Ordinal) || IsDescendantPartKey(kv.Key, partKey))
                    return true;
            }

            return false;
        }

        public static string GetCurrentLiveryKeyForSlot(int mechId, string socket, string hardpoint) {
            PersistentEntity unit = IDUtility.GetPersistentEntity(mechId);
            if (unit == null)
                return null;

            if (string.IsNullOrEmpty(socket))
                return unit.hasDataKeyEquipmentLivery ? unit.dataKeyEquipmentLivery.s : null;

            EquipmentEntity part = EquipmentUtility.GetPartInUnit(unit, socket, false, null);
            if (part == null)
                return null;

            if (string.IsNullOrEmpty(hardpoint))
                return part.hasDataKeyEquipmentLivery ? part.dataKeyEquipmentLivery.s : null;

            EquipmentEntity subsystem = EquipmentUtility.GetSubsystemInPart(part, hardpoint, false, null);
            if (subsystem == null)
                return null;

            return subsystem.hasDataKeyEquipmentLivery ? subsystem.dataKeyEquipmentLivery.s : null;
        }

        public static void OnMaybeMechSelectionChanged() {
            // If we've never seen this mech before, capture its full current livery assignments.
            int mechId = CIViewBaseLoadout.selectedUnitID;
            if (mechId >= 0 && !LiverySetsDB.TryGetMechOriginalSet(mechId, out _))
                CaptureEntireMechLiverySet(mechId);
        }

        // Determines & reapplies the appropriate livery-set to mech based upon current modes
        public static void ReassertMechLiverySet(int mechId = -1) {
            if (mechId < 0) mechId = CIViewBaseLoadout.selectedUnitID;
            if (mechId < 0) return;

            if (!TryGetMechOriginalSet(mechId, out _)) {
                CaptureEntireMechLiverySet(mechId);
                Debug.Log($"[LiveryGUI] Captured full mech livery on ReassertMechLiverySet() for mech={mechId}");
            }

            if (!GUI.IsPBLiveryViewActive()) {
                ApplyLiverySetToMech(mechId, true, true);
            } else if (!GUI.IsModPaneActive()) {
                ApplyLiverySetToMech(mechId, true, false);
            } else if (GUI.IsPilotModeActive()) {
                ApplyLiverySetToMech(mechId, GUI.IsPilotModeMechBaseVisible(), true, GUI.GetPilotModePilotId(mechId));
            } else {
                ApplyLiverySetToMech(mechId, true, false);
            }

            //3todo.later: if initial mech livery set got captured w/o a part, consider how that part's livery-set needs to make it into the capture, and how the livery-set gets applied when that part becomes present.
            //3todo.later: (consider adding that rainbow-scroll or intensity-scroll variation to pilot-transparent livery?)
        }

        // handle a livery slot being assigned via the UI. mechId may be -1 if unknown.
        public static void OnLiverySlotAssigned(int mechId, string partKey, string liveryKey) {
            if (GUI.IsPilotModeActive()) {
                string pilotId = GUI.GetPilotModePilotId(mechId);
                if (!string.IsNullOrEmpty(pilotId)) {
                    SetPilotLivery(pilotId, partKey, liveryKey);
                    Debug.Log($"[LiveryGUI] Recorded pilot livery: pilot={pilotId}, from mech={mechId}, part={partKey}, livery={liveryKey}");
                    return;
                }
            }
            else {
                SetMechOriginalLivery(mechId, partKey, liveryKey ?? PILOT_TRANSPARENT);
                Debug.Log($"[LiveryGUI] Recorded mech livery: mech={mechId}, part={partKey}, livery={liveryKey}");
            }

            DebugLogLiverySetDictionaries();
        }

        // Find name of pilot assigned to the given mech ID using the saved squad composition.
        public static string ResolvePilotIdForMech(int mechInstanceId) {
            PersistentEntity unit = IDUtility.GetPersistentEntity(mechInstanceId);
            string unitName = unit?.nameInternal.s;

            var persistent = Contexts.sharedInstance.persistent;
            if (string.IsNullOrEmpty(unitName) || !persistent.hasSquadComposition || persistent.squadComposition?.slots == null)
                return null;

            SquadComposition squadComp = persistent.squadComposition;

            foreach (var slot in squadComp.slots)
                if (!string.IsNullOrEmpty(slot?.unitNameInternal) && slot.unitNameInternal == unitName)
                    return slot.pilotNameInternal;

            Debug.LogError($"[LiveryGUI] Failed to find pilot for mech {unitName} in the {squadComp.slots.Count} entries of squad-slots");
            return null;
        }

        public static void ApplyBriefingLiverySetToUnitInSlot(int slotIndex) {
            if (!TryGetSquadSlot(slotIndex, out var squadSlot))
                return;

            if (string.IsNullOrEmpty(squadSlot.unitNameInternal))
                return;

            PersistentEntity unit = IDUtility.GetPersistentEntity(squadSlot.unitNameInternal);
            if (unit == null || !unit.hasId)
                return;

            ApplyLiverySetToMech(unit.id.id, true, true, squadSlot.pilotNameInternal, false);
        }

        public static void ApplyBriefingLiverySetsToAllUnits() {
            var persistent = Contexts.sharedInstance.persistent;
            if (!persistent.hasSquadComposition || persistent.squadComposition?.slots == null)
                return;

            for (int i = 0; i < persistent.squadComposition.slots.Count; i++)
                ApplyBriefingLiverySetToUnitInSlot(i);
        }

        public static void RefreshBriefingUnitVisual(int slotIndex) {
            if (!TryGetSquadSlot(slotIndex, out var squadSlot))
                return;

            if (HQHelperRoot.ins == null || HQHelperRoot.ins.helperUnitVisualBriefing == null)
                return;

            PersistentEntity unit = !string.IsNullOrEmpty(squadSlot.unitNameInternal) ? IDUtility.GetPersistentEntity(squadSlot.unitNameInternal) : null;
            PersistentEntity pilot = !string.IsNullOrEmpty(squadSlot.pilotNameInternal) ? IDUtility.GetPersistentEntity(squadSlot.pilotNameInternal) : null;
            HQHelperRoot.ins.helperUnitVisualBriefing.ApplyUnitToLink(slotIndex, unit, pilot, false);
        }

        static bool TryGetSquadSlot(int slotIndex, out SquadSlot squadSlot) {
            squadSlot = null;
            var persistent = Contexts.sharedInstance.persistent;
            if (!persistent.hasSquadComposition || persistent.squadComposition?.slots == null)
                return false;

            List<SquadSlot> slots = persistent.squadComposition.slots;
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return false;

            squadSlot = slots[slotIndex];
            return squadSlot != null;
        }

        static void EnsureMechOriginalSetsForCurrentUnits() {
            foreach (PersistentEntity unit in GetCurrentUnits()) {
                if (unit.hasId && !_mechOriginalLiveries.ContainsKey(unit.id.id))
                    CaptureEntireMechLiverySet(unit.id.id);
            }
        }

        static void EnsurePilotSetsForCurrentPilots() {
            foreach (PersistentEntity pilot in GetCurrentPilots()) {
                if (pilot.hasNameInternal && !string.IsNullOrEmpty(pilot.nameInternal.s) && !_pilotLiveries.ContainsKey(pilot.nameInternal.s))
                    _pilotLiveries[pilot.nameInternal.s] = new LiveryAssignmentSet();
            }
        }

        static void ApplyAllCurrentUnitLiverySets() {
            foreach (PersistentEntity unit in GetCurrentUnits()) {
                if (unit.hasId)
                    ApplyLiverySetToMech(unit.id.id, true, true, null, false);
            }
        }

        static List<PersistentEntity> GetCurrentUnits() {
            var result = new List<PersistentEntity>();
            PersistentContext persistent = Contexts.sharedInstance.persistent;
            foreach (PersistentEntity unit in persistent.GetGroup(PersistentMatcher.UnitTag)) {
                if (unit == null || !unit.isUnitTag || unit.isDestroyed || !unit.hasId || !unit.hasNameInternal || string.IsNullOrEmpty(unit.nameInternal.s))
                    continue;

                result.Add(unit);
            }
            return result;
        }

        static List<PersistentEntity> GetCurrentPilots() {
            var result = new List<PersistentEntity>();
            PersistentContext persistent = Contexts.sharedInstance.persistent;
            foreach (PersistentEntity pilot in persistent.GetGroup(PersistentMatcher.PilotTag)) {
                if (pilot == null || !pilot.isPilotTag || !pilot.hasNameInternal || string.IsNullOrEmpty(pilot.nameInternal.s))
                    continue;

                result.Add(pilot);
            }
            return result;
        }

        static LiveryAssignmentSet SanitizeForSave(LiveryAssignmentSet set) {
            return SanitizeLoadedSet(set);
        }

        static LiveryAssignmentSet SanitizeLoadedSet(LiveryAssignmentSet set) {
            var sanitized = new LiveryAssignmentSet();
            if (set == null)
                return sanitized;

            set.NormalizeAfterDeserialization();
            foreach (var kv in set.Assignments) {
                if (string.IsNullOrEmpty(kv.Key))
                    continue;

                sanitized.SetForPart(kv.Key, IsLiveryKeyAvailable(kv.Value) ? kv.Value : PILOT_TRANSPARENT);
            }
            return sanitized;
        }

        static bool IsLiveryKeyAvailable(string liveryKey) {
            if (string.IsNullOrEmpty(liveryKey) || string.Equals(liveryKey, PILOT_TRANSPARENT, StringComparison.Ordinal))
                return true;

            return DataMultiLinkerEquipmentLivery.data != null && DataMultiLinkerEquipmentLivery.data.ContainsKey(liveryKey);
        }

        static void DebugLogLiverySetDictionaries() {
            try {
                Debug.Log($"[LiveryGUI] Dev-Spam: Pilot livery sets ({_pilotLiveries.Count}):");
                foreach (var kv in _pilotLiveries) {
                    string pilotId = kv.Key ?? "<null>";
                    var set = kv.Value;
                    if (set == null || set.Assignments == null || set.Assignments.Count == 0) {
                        Debug.Log($"[LiveryGUI]  Dev-Spam: Pilot '{pilotId}': <empty>");
                        continue;
                    }
                    foreach (var a in set.Assignments) {
                        Debug.Log($"[LiveryGUI]  Dev-Spam: Pilot '{pilotId}' -> {a.Key} = {a.Value}");
                    }
                }

                Debug.Log($"[LiveryGUI] Mech original livery sets ({_mechOriginalLiveries.Count}):");
                foreach (var kv in _mechOriginalLiveries) {
                    int mechId = kv.Key;
                    var set = kv.Value;
                    if (set == null || set.Assignments == null || set.Assignments.Count == 0) {
                        Debug.Log($"[LiveryGUI]  Dev-Spam: Mech {mechId}: <empty>");
                        continue;
                    }
                    foreach (var a in set.Assignments) {
                        Debug.Log($"[LiveryGUI]  Dev-Spam: Mech {mechId} -> {a.Key} = {a.Value}");
                    }
                }
            }
            catch (Exception ex) {
                Debug.LogError($"[LiveryGUI] DebugLogLiverySetDictionaries error: {ex}");
            }
        }
    }
}
