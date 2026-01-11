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
    class LiveryColorSliderMarker : MonoBehaviour {} //todo.rem
    class LiveryAdvancedPaneMarker : MonoBehaviour {}

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
#if true
                    // create a new pane
                    var uiRoot = __instance.transform;

                    var paneGO = new GameObject("LiveryAdvancedPane");
                    paneGO.transform.SetParent(uiRoot, false);
                    paneGO.AddComponent<LiveryAdvancedPaneMarker>();
                    
                    #if false
                        var bgPrefab = someExistingPanelBackground; //todo?
                        var bg = GameObject.Instantiate(bgPrefab, paneGO.transform, false);
                    #else
                        var sprite = paneGO.AddComponent<UISprite>();
                        sprite.color = new Color(0f, 0f, 0f, 0.6f);
                        sprite.depth = 50;
                        sprite.width = 320;
                        sprite.height = 400;
                        sprite.pivot = UIWidget.Pivot.TopLeft;
                    #endif

                    paneGO.transform.localPosition = new Vector3(800f, 0f, 0f); //todo, do right side of screen

                    // add sliders to pane (todo: more; todo: actually put the sliders into a ScrollView.Content inside this pane)
                    var helperPrefab = CIViewPauseOptions.ins.settingPrefab;
                    var helperGO_R = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);

                    helperGO_R.name = "PrimaryColorR";
                    var helper_R = helperGO_R.GetComponent<CIHelperSetting>();

                    helper_R.toggleHolder.SetActive(false); // todo.hmm
                    helper_R.levelHolder.SetActive(false);  // todo.hmm
                    helper_R.sliderHolder.SetActive(true);  // todo.hmm

                    helper_R.sharedLabelName.text = "Primary Color (R)";
                    
                    var helperGO_G = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);

                    helperGO_G.name = "PrimaryColorG";
                    var helper_G = helperGO_G.GetComponent<CIHelperSetting>();

                    helper_G.toggleHolder.SetActive(false); // todo.hmm
                    helper_G.levelHolder.SetActive(false);  // todo.hmm
                    helper_G.sliderHolder.SetActive(true);  // todo.hmm

                    helper_G.sharedLabelName.text = "Primary Color (G)";

                    paneGO.SetActive(true); // todo: paneGO.SetActive(!paneGO.activeSelf);

                    helperGO_R.transform.localPosition = new Vector3(16f, -16f, 0f);
                    helperGO_G.transform.localPosition = new Vector3(16f, -64f, 0f);
                    
                    var helperGO_B = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);

                    helperGO_B.name = "PrimaryColorB";
                    var helper_B = helperGO_B.GetComponent<CIHelperSetting>();

                    helper_B.toggleHolder.SetActive(false); // todo.hmm
                    helper_B.levelHolder.SetActive(false);  // todo.hmm
                    helper_B.sliderHolder.SetActive(true);  // todo.hmm

                    helper_B.sharedLabelName.text = "Primary Color (B)";

                    helperGO_R.transform.localPosition = new Vector3(16f, -16f, 0f);
                    helperGO_G.transform.localPosition = new Vector3(16f, -64f, 0f);
                    helperGO_B.transform.localPosition = new Vector3(16f, -112f, 0f);

                    helper_R.sliderBar.valueMin = 0f;
                    helper_R.sliderBar.valueLimit = 1f;
                    helper_R.sliderBar.labelFormat = "F2";
                    helper_R.sliderBar.labelSuffix = " R";

                    paneGO.SetActive(true); // todo: paneGO.SetActive(!paneGO.activeSelf);
#elif false
                    var helperPrefab = CIViewPauseOptions.ins.settingPrefab;
                    var helperGO = GameObject.Instantiate(helperPrefab.gameObject, root, false);
                    helperGO.name = "LiveryColorSlider";

                    helperGO.AddComponent<LiveryColorSliderMarker>();

                    var helper = helperGO.GetComponent<CIHelperSetting>();

                    // Hide unused parts
                    helper.toggleHolder.SetActive(false); // todo.hmm
                    helper.levelHolder.SetActive(false);  // todo.hmm
                    helper.sliderHolder.SetActive(true);  // todo.hmm

                    // Configure label
                    helper.sharedLabelName.text = "Primary Color (R)";

                    // Configure slider
                    var bar = helper.sliderBar;
                    bar.valueMin = 0f;
                    bar.valueLimit = 1f;
                    bar.labelFormat = "F2";
                    bar.labelSuffix = " R";

                    helperGO.transform.SetAsFirstSibling(); //todo: there was a suggested alternative to directly set the localPosition with a +Vector3
#else
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
#endif
                }
                //todo UpdateSliderValues(__instance);
            }
        }//class RedrawLiveryGUI
    }//class Patches
}//namespace
