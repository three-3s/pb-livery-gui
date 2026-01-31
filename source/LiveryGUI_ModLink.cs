using Entitas;
using HarmonyLib;
using PhantomBrigade;
using PhantomBrigade.Data;
using PhantomBrigade.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

// INTRODUCTION / USAGE NOTES:
//  - The project's reference to 'System' must point to the Phantom Brigade one, not Microsoft.
//    It was necessary to add
//    C:\Program Files(x86)\Steam\steamapps\common\Phantom Brigade\PhantomBrigade_Data\Managed\
//    to the project's Reference Paths, which unfortunately isn't stored in the .csproj.
//     - (and add reference to UnityEngine.JSONSerializeModule to the project)
//  - Debug.Log goes to LocalLow/Brace.../.../Player.log
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

// PRIORITY ITEMS:
//  - This may or may not need some future proofing. E.g., copy the definition of the Serializable
//    Data-livery container, so we can always load the consistently-defined version, then copy
//    the values over item-by-item into an actual Livery. Or maybe it's OK to let the liveries
//    fail to load if they change the livery definition. But I'm a bit concerned they could make
//    a trivial change that would break the livery-loading. Presumably there's a safer pipeline
//    built into the game. TODO: At least test it and confirm that eg adding/removing/renaming a
//    field from the yaml does in fact prevent the livery from loading.

// BUGS:
//  - The livery-name text-box is initially "-". Doing just about anything makes it work, but
//    I haven't figured out how to get it to refresh or unstuck before whatever it is fixes it.
//    Sort of worked around by making the text field visible before 'show GUI' is clicked.

// TODO.post-release:
//  - Ergonomics: Add a button to 'favorite'. Clearer highlight of current? Jump to current?
//    Next/prev buttons? Clone-this-then-reset-orig? Could be as reset button labeled for prev.
//  - Might be nice to apply exponential scaling curve to slider values, though that might need
//    to be more or less built-in (or implemented into the slider).
//  - Consider adding tooltips to buttons.
//  - Ideally would want to be able to delete liveries.
//  - Maybe a button to open the directory containing the livery .yaml files?
//  - Maybe mark the liveries in the selection-list eg with a different color if they've been
//    created (or overwritten by) this mod? Or have unsaved changes?
//  - There might be some desire to go beyond the value-limits currently hardcoded. E.g., maybe
//    up to 3.0 instead of just 2.0. Or maybe some params are effective to much higher..?
//    Could/should at least let it support not-clamping upon load just to fit in the slider
//    limits. But maybe it would be possible to create a set of widgets where you could specify
//    min/max limits, activate picker mode, and apply those limits to any clicked-on sliders.
//  - Fix the label above the text input field from "Name" to eg "Livery Name".
//  - Remove the forced-capitalization on the text input field. (Seems to only affect display,
//    not the actual string.)
//  - Prefix-intercept and prevent CIViewBaseLoadout.UpdateCamera when the text-input field is
//    selected (like is done for headerInputUnitName.isSelected in that module).
//  - It seems like the liveryKey=null should map to "default" (?). But that's not hooked up right
//    now, and the sliders do nothing for the null livery. No reason that case couldn't be made
//    to work, though it'd possibly affect many operations needing to know that null="default"
//    mapping.
//
// ADDITIONAL POSSIBLE IMPROVEMENTS:
//  - There's a fair amount of brute-force item-by-item a.primR=b.primR,a.primG=b.primG etc.

namespace LiveryGUIMod
{
    //==================================================================================================
    // (Having a class derived from ModLink might (?) be necessary, but the overrides are probably just
    //  leftover 'hello world' stuff at this point.)
    public class ModLinkCustom : ModLink
    {
#if false
        public static ModLinkCustom ins;

        public override void OnLoadStart()
        {
            ins = this;
            //Debug.Log($"OnLoadStart");
        }

        public override void OnLoad(Harmony harmonyInstance)
        {
            base.OnLoad(harmonyInstance);
            //Debug.Log($"OnLoad | Mod: {modID} | Index: {modIndexPreload} | Path: {modPath}");
        }
#endif
    }//class


    //+================================================================================================+
    //||                  MOD'S CONTROL-FLOW ENTRY POINT (via Harmony hooks)                          ||
    //+================================================================================================+
    public class Patches
    {
        // Load all liveries the user previously saved to file.
        // Initializes the liveries-snapshots DB.
        //
        // "Dear Harmony, please call into this OnInit class whenever DataMultiLinkerEquipmentLivery.OnAfterDeserialization() runs"
        [HarmonyPatch(typeof(DataMultiLinkerEquipmentLivery), MethodType.Normal), HarmonyPatch("OnAfterDeserialization")]
        public class OnInit
        {
            static bool loadedYet = false;

            // "Dear Harmony, due to this function being named Prefix(), please call this function BEFORE that
            //  DataMultiLinkerEquipmentLivery.OnAfterDeserialization() runs."
            public static void Prefix() {
                if (loadedYet)
                    return;
                loadedYet = true;

                LiverySnapshotDB.SnapshotInitialLiveries();
                LoadAndSave.LoadUserSavedLiveries();
            }
        }

        // RedrawForLivery() gets invoked when navigating to the livery-select page, and when choosing a
        // different livery or livery-slot. We'll create (and repopulate values for) our livery sliders here.
        //
        // "Dear Harmony, please call into this RedrawLiveryGUI class whenever CIViewBaseLoadout.RedrawForLivery() runs"
        [HarmonyPatch(typeof(CIViewBaseLoadout), MethodType.Normal), HarmonyPatch("RedrawForLivery")]
        public class RedrawLiveryGUI {
            // "Dear Harmony, due to this function being named Postfix(), please call this RollForSalvage() function
            //  BEFORE that DifficultyUtility.GetFlag() runs (and depending on what I say, either call the normal
            //  GetFlag() or use the result I give instead)"
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
