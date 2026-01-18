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

// POSSIBLE IMPROVEMENTS:
//  - GUI layout/presentation. Anchor to screen better? Readability concerns? White-on-white is
//    hard to read. Would be nice to have some grouping for the bars. (There is spare space for
//    more gaps.) And 'alpha' is distinct from the others so could be... diff color? width?

// TODO.MVP:
//  - rework clone/save/name functionality, consulting/updating LiverySnapshotDB, and add
//    buttons to revert-livery-to-snapshot, maybe revert-livery-name-input-box, and show whether
//    the livery has been saved (maybe re-color the button, but don't inhibit it).
//  - add text-popup on saved successfully or failed-and-why eg we-don't-own-the-key.
//  - Minimum button readability: add tooltips (or convert to text-buttons). ideally, add a
//    button-icon for clone, and switch the 'save' icon.
//  - confirm i'm not introducing any new 'crashes' in the player.log (via any mods)
//  - commit to or change away from using the AppData/Local mods folder
//  - PUBLISH (github + steam workshop)

// BUGS:
//  - The livery-name text-box is initially "-". Doing just about anything makes it work, but
//    I haven't figured out how to get it to refresh or unstuck before whatever it is fixes it.

// TODO.post-release:
//  - What are string contentSource and Vector4 contentParameters? There's also string source,
//    which I'd assumed was metadata, but am less sure about now. E.g., decal-overlays stuff?
//    Or maybe there's some tagging for usability by randomly generated hostiles?
//    There's also hidden, textName, and priority (which I've ignored other than co-opting
//    textName at least for now, to tie it to file-name)
//     - Regarding contentSource, it might fail to load if it's not an expected source?
//          bool flag3 = !string.IsNullOrEmpty(dataContainerEquipmentLivery.contentSource);
//          if (!flag3 || SettingUtility.IsEntFound(dataContainerEquipmentLivery.contentSource))
//             'addThatLiveryToLiveriesList' // (snipped from RedrawLiveryOptionsFull())
//  - Decide whether overriding of existing built-in liveries should be permitted via this same
//    'save-on-request and load-on-init' mechanism.
//  - Add a text-box for showing-name-of & renaming the livery.
//  - Better button icons, probably with tooltips.
//  - Save-status messages (success & failure).
//  - Ideally would want to be able to delete liveries.
//  - Maybe a button to open the directory containing the livery .yaml files?
//  - Maybe mark the liveries in the selection-list eg with a different color if they've been
//    created (or overwritten by) this mod?
//  - There might be some desire to go beyond the value-limits currently hardcoded. E.g., maybe
//    up to 3.0 instead of just 2.0. Or maybe some params are effective to much higher..?
//  - Fix the label above the text input field from "Name" to eg "Livery Name".
//  - Remove the forced-capitalization on the text input field. (Seems to only affect display,
//    not the actual string.)
//  - Fix how 'select the already-existing key-livery when cloning but key already exists': this
//    actual toggles-off if the currently selected livery is this newKey.
//  - Prefix-intercept and prevent CIViewBaseLoadout.UpdateCamera when the text-input field is
//    selected (like is done for headerInputUnitName.isSelected in that module).

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
