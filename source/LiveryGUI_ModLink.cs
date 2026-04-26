using HarmonyLib;
using PhantomBrigade.Data;
using PhantomBrigade.Mods;
using System;
using Debug = UnityEngine.Debug;

// INTRODUCTION / USAGE NOTES:
//  - The project's reference to 'System' must point to the Phantom Brigade one, not Microsoft.
//    It was necessary to add
//    C:\Program Files(x86)\Steam\steamapps\common\Phantom Brigade\PhantomBrigade_Data\Managed\
//    to the project's Reference Paths, which unfortunately isn't stored in the .csproj.
//     - (and add reference to UnityEngine.JSONSerializeModule to the project)
//  - Debug.Log goes to %AppData%/LocalLow/Brace Yourself Games/Phantom Brigade/Player.log
//  - Harmony.Debug = true + FileLog.Log (and FlushBuffer) goes to desktop harmony.log.txt
//  - You may want to read more about (or ask a chatbot about):
//     - How to use eg dnSpy to decompile & search the Phantom Brigade C# module assemblies.
//     - General info about the 'Entitas' Entity Component System.
//     - Explain what the HarmonyPatch things are.
//  - Note that modding this game via C# has some significant overlap with some other heavily
//    modded games such as RimWorld (another Unity game (+HarmonyLib)).
//  - Other basic getting-started info:
//     - https://github.com/BraceYourselfGames/PB_ModSDK/wiki/Mod-system-overview#libraries
//     - https://wiki.braceyourselfgames.com/en/PhantomBrigade/Modding/ModSystem
//

namespace LiveryGUIMod
{
    //==================================================================================================
    // (Having a class derived from ModLink might (?) be necessary, but the overrides are probably just
    //  leftover 'hello world' stuff at this point.)
    public class ModLinkCustom : ModLink {
#if false
        public static ModLinkCustom ins;

        public override void OnLoadStart() {
            ins = this;
            //Debug.Log($"OnLoadStart");
        }

        public override void OnLoad(Harmony harmonyInstance) {
            base.OnLoad(harmonyInstance);
            //Debug.Log($"OnLoad | Mod: {modID} | Index: {modIndexPreload} | Path: {modPath}");
        }
#endif
    }//class


    //+================================================================================================+
    //||                  MOD'S CONTROL-FLOW ENTRY POINT (via Harmony hooks)                          ||
    //+================================================================================================+
    public class Patches {
        // Load all liveries the user previously saved to file.
        // Initializes the liveries-snapshots DB.
        //
        // "Dear Harmony, please call OnInit.Prefix() before whenever DataMultiLinkerEquipmentLivery.OnAfterDeserialization() runs"
        [HarmonyPatch(typeof(DataMultiLinkerEquipmentLivery), MethodType.Normal), HarmonyPatch("OnAfterDeserialization")]
        public class OnInit {
            static bool loadedYet = false;
            public static void Prefix() {
                if (loadedYet)
                    return;
                loadedYet = true;

                LiverySnapshotDB.SnapshotInitialLiveries();
                LoadAndSave.LoadUserSavedLiveries();
            }
        }

        // Catch when the player attempts to attach a livery to a slot via the base UI.
        // We'll notify our in-memory DB about the change so callers can update stored sets.
        [HarmonyPatch(typeof(CIViewBaseLoadout), MethodType.Normal), HarmonyPatch("OnLiveryAttachAttempt")]
        public class OnLiveryAttachAttemptPatch {
            public static void Postfix(CIViewBaseLoadout __instance, object liveryKeyArg)
            {
                try
                {
                    string liveryKey = liveryKeyArg as string;
                    int mechId = CIViewBaseLoadout.selectedUnitID;
                    string partKey = LiverySetsDB.PartKey(CIViewBaseLoadout.selectedUnitSocket, CIViewBaseLoadout.selectedUnitHardpoint);
                    // 3todo.later: need to test how the null-or-empty works, with respect to PB's use of some sort of 'default'/null pseudo-livery.
                    string key = string.IsNullOrEmpty(liveryKey) ? LiverySetsDB.PILOT_TRANSPARENT : liveryKey;
                    Debug.Log($"[LiveryGUI] OnLiveryAttachAttempt: mech={mechId}, part={partKey}, livery={key}");
                    LiverySetsDB.OnLiverySlotAssigned(mechId, partKey, key);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LiveryGUI] Error in OnLiveryAttachAttemptPatch: {ex}");
                }
            }
        }

        // 3todo.later: consider implication of the "liveryPreservation" option (when swapping-out / attaching new parts to unit)
        //3todo.later: test what happens when part is removed then same/different part reattached (with & without the preserve-mech-livery setting enabled?)
        // Catch when player attempts to clear a livery from a slot. This slot could be whole-unit, or whole-part, or a subsystem/hardpoint on a part.
        // We need to hook all three cases, but distinguish these from e.g. "remove the actual part", since that routes through these same functions.
        [HarmonyPatch(typeof(CIViewBaseLoadout), MethodType.Normal), HarmonyPatch("OnUnitRemoved")]
        public class OnUnitRemovedPatch {
            public static void Postfix()
            {
                try
                {
                    if (CIViewBaseLoadout.liveryMode)
                    {
                        int mechId = CIViewBaseLoadout.selectedUnitID;
                        string partKey = LiverySetsDB.PartKey(null, null);
                        string liveryKey = LiverySetsDB.PILOT_TRANSPARENT;
                        Debug.Log($"[LiveryGUI] OnUnitRemovedPatch: mech={mechId}, livery={liveryKey}");
                        LiverySetsDB.OnLiverySlotAssigned(mechId, partKey, liveryKey);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LiveryGUI] Error in OnUnitRemovedPatch: {ex}");
                }
            }
        }
        [HarmonyPatch(typeof(CIViewBaseLoadout), MethodType.Normal), HarmonyPatch("OnSocketRemoved")]
        public class OnSocketRemovedPatch {
            public static void Postfix(object socketArg)
            {
                try
                {
                    if (CIViewBaseLoadout.liveryMode)
                    {
                        string socket = socketArg as string;
                        int mechId = CIViewBaseLoadout.selectedUnitID;
                        string partKey = LiverySetsDB.PartKey(socket, null);
                        string liveryKey = LiverySetsDB.PILOT_TRANSPARENT;
                        Debug.Log($"[LiveryGUI] OnSocketRemovedPatch: mech={mechId}, part={partKey}, livery={liveryKey}");
                        LiverySetsDB.OnLiverySlotAssigned(mechId, partKey, liveryKey);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LiveryGUI] Error in OnSocketRemovedPatch: {ex}");
                }
            }
        }
        [HarmonyPatch(typeof(CIViewBaseLoadout), MethodType.Normal), HarmonyPatch("OnHardpointRemoved")]
        public class OnHardpointRemovedPatch {
            public static void Postfix(object hardpointArg)
            {
                try
                {
                    if (CIViewBaseLoadout.liveryMode)
                    {
                        string socket = CIViewBaseLoadout.selectedUnitSocket;
                        string hardpoint = hardpointArg as string;
                        int mechId = CIViewBaseLoadout.selectedUnitID;
                        string partKey = LiverySetsDB.PartKey(socket, hardpoint);
                        string liveryKey = LiverySetsDB.PILOT_TRANSPARENT;
                        Debug.Log($"[LiveryGUI] OnHardpointRemovedPatch: mech={mechId}, part={partKey}, livery={liveryKey}");
                        LiverySetsDB.OnLiverySlotAssigned(mechId, partKey, liveryKey);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LiveryGUI] Error in OnHardpointRemovedPatch: {ex}");
                }
            }
        }

        // RedrawForLivery() gets invoked when navigating to the livery-select page, and when choosing a
        // different livery or livery-slot. We'll create (and repopulate values for) our livery sliders here.
        [HarmonyPatch(typeof(CIViewBaseLoadout), MethodType.Normal), HarmonyPatch("RedrawForLivery")]
        public class RedrawLiveryGUI {
            public static void Postfix(CIViewBaseLoadout __instance, string socketTarget, string hardpointTarget, bool closeOnRepeat) {
                _ = socketTarget;
                _ = hardpointTarget;
                _ = closeOnRepeat;

                //Debug.Log($"RedrawLiveryGUI(socketTarget={socketTarget},hardpointTarget={hardpointTarget},closeOnRepeat={closeOnRepeat})");

                GUI.RedrawLiveryGUI(__instance);
            }//Postfix()
        }//class RedrawLiveryGUI
    }//class Patches
}//namespace
