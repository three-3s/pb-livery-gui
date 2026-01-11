using Entitas;
using HarmonyLib;
using PhantomBrigade;
using PhantomBrigade.Data;
using PhantomBrigade.Mods;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System;
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

namespace ModExtensions
{
    //==================================================================================================
    // (Having a class derived from ModLink might (?) be necessary, but the overrides are probably just
    //  leftover 'hello world' stuff at this point.)
    public class ModLinkCustom : ModLink
    {
        public static ModLinkCustom ins;

        public override void OnLoadStart()
        {
            ins = this;
            //Debug.Log($"OnLoadStart");
        }

        public override void OnLoad(Harmony harmonyInstance)
        {
            base.OnLoad(harmonyInstance);
            Debug.Log($"OnLoad | Mod: {modID} | Index: {modIndexPreload} | Path: {modPath}");
        }
    }//class

    // marker for whether we've initialized our LiveryGUI sliders into a given root-GUI-thingy yet
    class LiveryColorSliderMarker : MonoBehaviour {}

    //+================================================================================================+
    //||                                                                                              ||
    //+================================================================================================+
    public class Patches
    {
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
                Debug.Log($"todo RedrawLiveryGUI(socketTarget={socketTarget},hardpointTarget={hardpointTarget},closeOnRepeat={closeOnRepeat})");

                // if (!CIViewBaseLoadout.liveryMode)
                //     return;
                // if (CIViewBaseLoadout.selectedUnitID < 0)
                //     return;

                var root = __instance.liveryRootObject.transform;
                var existing = root.GetComponentInChildren<LiveryColorSliderMarker>();
                if (existing == null)
                {
                    // TEMP: create a dummy GameObject first
                    var go = new GameObject("LiveryColorSliderStub");
                    go.transform.SetParent(root, false);
                    go.AddComponent<LiveryColorSliderMarker>();

                    Debug.Log($"todo creating slider");

                    var prefabSettingsSlideBar = CIViewPauseOptions.ins.settingPrefab.sliderBar;

                    var clone = GameObject.Instantiate(prefabSettingsSlideBar.gameObject, go.transform);
                    var bar = clone.GetComponent<CIBar>();
                    bar.valueMin = 0f;
                    bar.valueLimit = 1f;
                    bar.labelFormat = "F2";
                    bar.labelSuffix = " R";

                    bar.callbackOnAdjustment = new UICallback(value =>
                    {
                        int unitID = CIViewBaseLoadout.selectedUnitID;
                        if (unitID < 0) return; //todo.revisit

                        //todo:
                        //ApplyPrimaryColorROverride(unitID, value);
                        //RefreshUnitVisuals(unitID);

                        Color current = Color.red; //todo GetEffectivePrimaryColor(unitID);
                        bar.valueRaw = 0.7f;
                    }, 0f);
                }
                //todo UpdateSliderValues(__instance);
            }
        }//class RedrawLiveryGUI
    }//class Patches
}//namespace
