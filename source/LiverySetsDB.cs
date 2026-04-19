using PhantomBrigade;
using PhantomBrigade.Game;
using System;
using System.Collections.Generic;
using UnityEngine;

// 3todo.notes: DataHelperLoading.cs has LoadingStart(string key, string saveLocation); calls DataManagerSave.LoadData(saveLocation)

namespace LiveryGUIMod
{
    public static class LiverySetsDB
    {
        // sentinel value meaning "transparent / do not override the mech's current livery for this part"
        public const string PILOT_TRANSPARENT = "__PILOT_TRANSPARENT__";

        // Pilot unique id (string) -> assignment set
        static readonly Dictionary<string, LiveryAssignmentSet> _pilotLiveries = new Dictionary<string, LiveryAssignmentSet>(StringComparer.Ordinal);

        // Mech instance id (int) -> assignment set (used to store/restore original mech liveries while a pilot is re/assigned)
        static readonly Dictionary<int, LiveryAssignmentSet> _mechOriginalLiveries = new Dictionary<int, LiveryAssignmentSet>();

        // Mode and view events
        private static bool isPilotModeActive = false;
        public static bool IsPilotModeActive { get => isPilotModeActive; private set => isPilotModeActive = value; }

        // Builds the part-key for use in LiverySetsDB's Dictionary objects.
        public static string PartKey(string socket, string hardpoint)
        {
            // NOTE: The top-level mech livery will not have a socket/hardpoint, so will use that <nosocket>:<nohardpoint> as its key.
            // And the mid-level part like whole-left-arm will look like optional_left:<nohardpoint>.
            // Versus sub-part like optional_left:external_arm_lower.
            return (socket ?? "<nosocket>") +":" + (hardpoint ?? "<nohardpoint>");
        }

        // Retrieve existing set for pilot or create a new empty one.
        public static LiveryAssignmentSet GetOrCreatePilotSet(string pilotId)
        {
            if (string.IsNullOrEmpty(pilotId))
            {
                Debug.LogWarning("[PilotLiveryStore] GetOrCreatePilotSet called with null/empty pilotId");
                return null;
            }

            if (!_pilotLiveries.TryGetValue(pilotId, out var set))
            {
                set = new LiveryAssignmentSet();
                _pilotLiveries[pilotId] = set;
            }
            return set;
        }

        // Try get, don't create
        public static bool TryGetPilotSet(string pilotId, out LiveryAssignmentSet set)
        {
            set = null;
            if (string.IsNullOrEmpty(pilotId))
                return false;
            return _pilotLiveries.TryGetValue(pilotId, out set);
        }

        // Set a single part assignment for a pilot. Creates the set if necessary.
        public static void SetPilotLivery(string pilotId, string partKey, string liveryKey)
        {
            if (string.IsNullOrEmpty(pilotId))
            {
                Debug.LogWarning("[PilotLiveryStore] SetPilotLivery called with null/empty pilotId");
                return;
            }

            var set = GetOrCreatePilotSet(pilotId);
            set.SetForPart(partKey, liveryKey);
        }

        // Remove a pilot's entire set
        public static bool RemovePilotSet(string pilotId)
        {
            if (string.IsNullOrEmpty(pilotId))
                return false;
            return _pilotLiveries.Remove(pilotId);
        }

        // Clear all pilot entries
        public static void ClearAllPilotSets()
        {
            _pilotLiveries.Clear();
        }

        // Enumerate known pilot ids
        public static IEnumerable<string> GetAllPilotIds()
        {
            return _pilotLiveries.Keys;
        }

        // --- mech original livery helpers ---
        public static LiveryAssignmentSet GetOrCreateMechOriginalSet(int mechInstanceId)
        {
            if (!_mechOriginalLiveries.TryGetValue(mechInstanceId, out var set))
            {
                set = new LiveryAssignmentSet();
                _mechOriginalLiveries[mechInstanceId] = set;
            }
            return set;
        }

        public static bool TryGetMechOriginalSet(int mechInstanceId, out LiveryAssignmentSet set)
        {
            set = null;
            return _mechOriginalLiveries.TryGetValue(mechInstanceId, out set);
        }

        public static void SetMechOriginalLivery(int mechInstanceId, string partKey, string liveryKey)
        {
            var set = GetOrCreateMechOriginalSet(mechInstanceId);
            set.SetForPart(partKey, liveryKey);
        }

        public static bool RemoveMechOriginalSet(int mechInstanceId)
        {
            return _mechOriginalLiveries.Remove(mechInstanceId);
        }

        public static void ClearAllMechOriginalSets()
        {
            _mechOriginalLiveries.Clear();
        }

        public static IEnumerable<int> GetAllMechIds()
        {
            return _mechOriginalLiveries.Keys;
        }

        // Utility: merge (copy) a source set into a destination set, overwriting per-part keys.
        public static void MergeInto(LiveryAssignmentSet destination, LiveryAssignmentSet source)
        {
            if (destination == null || source == null) return;
            foreach (var kv in source.Assignments)
                destination.SetForPart(kv.Key, kv.Value);
        }

        // Notify that the UI/view was entered or exited
        public static void OnViewEntered()
        {
            int mechId = CIViewBaseLoadout.selectedUnitID;
            if (mechId >= 0)
            {
                string partKey = PartKey(CIViewBaseLoadout.selectedUnitSocket, CIViewBaseLoadout.selectedUnitHardpoint);
                string currentLivery = CIViewBaseLoadout.selectedUnitLivery;
                string key = string.IsNullOrEmpty(currentLivery) ? LiverySetsDB.PILOT_TRANSPARENT : currentLivery;
                // only record if we don't already have any record for this mech
                if (!LiverySetsDB.TryGetMechOriginalSet(mechId, out var existing))
                {
                    LiverySetsDB.SetMechOriginalLivery(mechId, partKey, key);
                    Debug.Log($"[LiveryGUI] Captured initial mech livery for mech={mechId}, part={partKey}, livery={key}");
                }
            }
        }

        public static void OnViewExited()
        {
            //3todo.later confirm if needed or if can delete
        }

        // Switch between Pilot Mode and Mech Mode
        public static void SetPilotModeActive(bool active)
        {
            if (IsPilotModeActive == active) return;
            IsPilotModeActive = active;
            Debug.Log($"[LiveryGUI] Setting IsPilotModeActive={IsPilotModeActive}");

            // when mode changes, ensure we capture current selection into mech-original if needed
            int mechId = CIViewBaseLoadout.selectedUnitID;
            if (mechId >= 0)
            {
                string partKey = PartKey(CIViewBaseLoadout.selectedUnitSocket, CIViewBaseLoadout.selectedUnitHardpoint);
                string currentLivery = CIViewBaseLoadout.selectedUnitLivery;
                string key = string.IsNullOrEmpty(currentLivery) ? PILOT_TRANSPARENT : currentLivery;
                if (!TryGetMechOriginalSet(mechId, out _))
                {
                    SetMechOriginalLivery(mechId, partKey, key);
                    Debug.Log($"[LiveryGUI] Captured mech livery on mode change for mech={mechId}, part={partKey}, livery={key}");
                }
            }
        }

        // handle a livery slot being assigned via the UI. mechId may be -1 if unknown.
        public static void OnLiverySlotAssigned(int mechId, string partKey, string liveryKey)
        {
            // update mech-original record to reflect current base livery assignment
            if (IsPilotModeActive)
            {
                // try to resolve pilot assigned to this mech
                string pilotId = ResolvePilotIdForMech(mechId);
                if (!string.IsNullOrEmpty(pilotId))
                {
                    SetPilotLivery(pilotId, partKey, liveryKey);
                    Debug.Log($"[LiveryGUI] Recorded pilot livery: pilot={pilotId}, from mech={mechId}, part={partKey}, livery={liveryKey}");
                    return;
                }
                else
                {
                    // no pilot assigned - fall back to recording as mech-original
                    SetMechOriginalLivery(mechId, partKey, liveryKey ?? PILOT_TRANSPARENT);
                    Debug.Log($"[LiveryGUI] No pilot assigned to mech={mechId}; recorded mech livery part={partKey}, livery={liveryKey}");
                    return;
                }
            }
            else
            {
                SetMechOriginalLivery(mechId, partKey, liveryKey ?? PILOT_TRANSPARENT);
                Debug.Log($"[LiveryGUI] Recorded mech livery: mech={mechId}, part={partKey}, livery={liveryKey}");
            }

            DebugLogLiverySetDictionaries();
        }

        // Find name of pilot assigned to the given mech ID using the saved squad composition.
        static string ResolvePilotIdForMech(int mechInstanceId)
        {
            PersistentEntity unit = IDUtility.GetPersistentEntity(mechInstanceId);
            string unitName = unit?.nameInternal.s;

            var persistent = Contexts.sharedInstance.persistent;
            SquadComposition squadComp = persistent.squadCompositionEntity.squadComposition;

            foreach (var slot in squadComp.slots)
                if (!string.IsNullOrEmpty(slot?.unitNameInternal) && slot.unitNameInternal == unitName)
                    return slot.pilotNameInternal;

            Debug.LogError($"[LiveryGUI] Failed to find pilot for mech {unitName} in the {squadComp.slots.Count} entries of squad-slots");
            return null;
        }

        static void DebugLogLiverySetDictionaries()
        {
            try
            {
                Debug.Log($"[LiveryGUI] Pilot livery sets ({_pilotLiveries.Count}):");
                foreach (var kv in _pilotLiveries)
                {
                    string pilotId = kv.Key ?? "<null>";
                    var set = kv.Value;
                    if (set == null || set.Assignments == null || set.Assignments.Count == 0)
                    {
                        Debug.Log($"[LiveryGUI]  Pilot '{pilotId}': <empty>");
                        continue;
                    }
                    foreach (var a in set.Assignments)
                    {
                        Debug.Log($"[LiveryGUI]  Pilot '{pilotId}' -> {a.Key} = {a.Value}");
                    }
                }

                Debug.Log($"[LiveryGUI] Mech original livery sets ({_mechOriginalLiveries.Count}):");
                foreach (var kv in _mechOriginalLiveries)
                {
                    int mechId = kv.Key;
                    var set = kv.Value;
                    if (set == null || set.Assignments == null || set.Assignments.Count == 0)
                    {
                        Debug.Log($"[LiveryGUI]  Mech {mechId}: <empty>");
                        continue;
                    }
                    foreach (var a in set.Assignments)
                    {
                        Debug.Log($"[LiveryGUI]  Mech {mechId} -> {a.Key} = {a.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveryGUI] DebugLogLiverySetDictionaries error: {ex}");
            }
        }
    }
}
