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
    class LiveryAdvancedPaneMarker : MonoBehaviour {}


    //+================================================================================================+
    //||                                                                                              ||
    //+================================================================================================+
    public class Patches
    {
        static GameObject paneGO = null;
        static CIHelperSetting helper_R = null;
        static CIHelperSetting helper_G = null;
        static CIHelperSetting helper_B = null;

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

                if (paneGO == null)
                {
                    // create a new pane
                    var uiRoot = __instance.transform;

                    paneGO = new GameObject("LiveryAdvancedPane");
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
                    // by cloning the 'options menu' prefab sliders
                    var helperPrefab = CIViewPauseOptions.ins.settingPrefab;

                    var helperGO_R = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);
                    helperGO_R.name = "PrimaryColorR";
                    helper_R = helperGO_R.GetComponent<CIHelperSetting>();
                    helper_R.sharedLabelName.text = "Primary Color (R)";
                    
                    var helperGO_G = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);
                    helperGO_G.name = "PrimaryColorG";
                    helper_G = helperGO_G.GetComponent<CIHelperSetting>();
                    helper_G.sharedLabelName.text = "Primary Color (G)";
                    
                    var helperGO_B = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);
                    helperGO_B.name = "PrimaryColorB";
                    helper_B = helperGO_B.GetComponent<CIHelperSetting>();
                    helper_B.sharedLabelName.text = "Primary Color (B)";

                    GameObject[] game_objects = { helperGO_R, helperGO_G, helperGO_B };
                    CIHelperSetting[] helpers = { helper_R, helper_G, helper_B };
                    float next_y = -16f;
                    foreach (var helperGO in game_objects) {
                        helperGO.transform.localPosition = new Vector3(16f, next_y, 0f);
                        next_y -= 48f;
                    }
                    foreach (var helper in helpers) {
                        helper.toggleHolder.SetActive(false); // todo.hmm
                        helper.levelHolder.SetActive(false);  // todo.hmm
                        helper.sliderHolder.SetActive(true);  // todo.hmm

                        helper.sliderBar.valueMin = 0f;
                        helper.sliderBar.valueLimit = 1f;
                        helper.sliderBar.labelFormat = "F2";
                        helper.sliderBar.labelSuffix = " R";
                        
                        helper.sliderBar.callbackOnAdjustment = new UICallback(value =>
                        {
                            var livery = GetSelectedLivery();
                            if (livery == null)
                                return;

                            Color c = livery.colorPrimary;
                            c.r = helper_R.sliderBar.valueRaw;
                            c.g = helper_G.sliderBar.valueRaw;
                            c.b = helper_B.sliderBar.valueRaw;
                            livery.colorPrimary = c;

                            RefreshSphereAndMechPreviews();
                        }, 0f);
                    }

                    paneGO.SetActive(true); // todo: paneGO.SetActive(!paneGO.activeSelf);
                }
                
                SyncSlidersFromLivery(helper_R.sliderBar, helper_G.sliderBar, helper_B.sliderBar, GetSelectedLivery());
            }//Postfix()

            private static DataContainerEquipmentLivery GetSelectedLivery()
            {
                string key = CIViewBaseLoadout.selectedUnitLivery;
                if (string.IsNullOrEmpty(key))
                    return null;

                return DataMultiLinker<DataContainerEquipmentLivery>.GetEntry(key, false);
            }

            static void SyncSlidersFromLivery(
                CIBar r, CIBar g, CIBar b,
                DataContainerEquipmentLivery livery)
            {
                if (livery == null) return;

                Color c = livery.colorPrimary;

                r.valueRaw = c.r;
                g.valueRaw = c.g;
                b.valueRaw = c.b;
            }
            
            static void RefreshSphereAndMechPreviews()
            {
                int unitID = CIViewBaseLoadout.selectedUnitID;
                if (unitID < 0)
                    return;

                PersistentEntity unit = IDUtility.GetPersistentEntity(unitID);
                if (unit == null)
                    return;

                // trigger update the sprite of the sphere showing the 3 color-sections of the livery
                CIViewBaseLoadout.ins.Redraw(null, null, false);

                // trigger update of the 3D model (stolen from OnLiveryHoverStart())
                if (string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitSocket))
                {
                    CIViewBaseCustomizationRoot.ins.UpdateUnitLivery();
                    return;
                }
                if (string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitHardpoint))
                {
                    CIViewBaseCustomizationRoot.ins.UpdatePartLivery(CIViewBaseLoadout.selectedUnitSocket);
                    return;
                }
                CIViewBaseCustomizationRoot.ins.UpdateSubsystemLivery(CIViewBaseLoadout.selectedUnitSocket, CIViewBaseLoadout.selectedUnitHardpoint);
            }
        }//class RedrawLiveryGUI
    }//class Patches
}//namespace
