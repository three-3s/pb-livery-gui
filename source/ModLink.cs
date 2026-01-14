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
using PhantomBrigade.AI.Components;

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

// TODO:
//  - Need to avoid modifying the built-in liveries. Would like a copy-to-new button. Maybe save
//    all the liveries to a master .yml file in the AppData (or a folder with multiple). And load
//    those at startup.
//  - Would like a toggle-button for visibility of all the sliders.
//  - Layout could be better (especially the gap between slider-label and the slider).
//  - Ideally the blue-fill of the sliders would partially transparent?
//  - Test other display-resolutions to make sure nothing's off-screen etc.

// BUGS:
//  - The on-adjusted-slider callback causes any selected socket/subsystem to be deselected, and
//    the mech-level livery becomes selected again.
//  - The sliders remain on-screen for the non-livery equipment editing. (But go away when leaving
//    the unit-editing tab.)
//  - Hover-text for the sliders says 'Lorem ipsum'.

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

    public class SliderConfig {
        public string name;
        public string label;
        public float x_pos;
        public float y_pos;
        public Color fill_color;
        public float min;
        public float max;
        public SliderConfig(string name_in, string label_in, float x_in, float y_in, Color fill_color_in, float min_in = 0f, float max_in = 1f) {
            name = name_in;
            label = label_in;
            x_pos = x_in;
            y_pos = y_in;
            fill_color = fill_color_in;
            min = min_in;
            max = max_in;
        }
    }


    //+================================================================================================+
    //||                                                                                              ||
    //+================================================================================================+
    public class Patches
    {
        static GameObject paneGO = null;
        static Dictionary<string, CIHelperSetting> slider_helpers = null;
        static Dictionary<string, SliderConfig> slider_configs = null;

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
                    const float vGap = 40f;
                    float[] x = {
                        0f,
                        715f,
                    };
                    float[] y = {
                        -vGap *  0.0f,
                        -vGap *  1.0f,
                        -vGap *  2.0f,
                        -vGap *  3.0f,
                        -vGap *  5.0f, // +1.0 gap
                        -vGap *  6.0f,
                        -vGap *  7.0f,
                        -vGap *  8.0f,
                        -vGap * 10.0f, // +1.0 gap
                        -vGap * 11.0f,
                        -vGap * 12.0f,
                        -vGap * 13.0f,
                        -vGap * 15.0f, // +1.0 gap
                        -vGap * 16.0f,
                        -vGap * 17.0f,
                        -vGap * 18.0f,
                    };
                    const float loC  =  0f; // min color-slider val (color-component < 0 causes the whole color to be black)
                    const float hiC  =  2f; // max color-slider val
                    const float loNC = -2f; // min non-color-slider
                    const float hiNC =  2f; // max non-color-slider
                    Color R = new Color(0.5f, 0f, 0f, 0.5f);
                    Color G = new Color(0f, 0.5f, 0f, 0.5f);
                    Color B = new Color(0f, 0f, 0.7f, 0.5f);
                    Color A = new Color(1f, 1f, 1f, 0.37f);
                    slider_helpers = new Dictionary<string, CIHelperSetting>();
                    slider_configs = new Dictionary<string, SliderConfig>() {
                        { "PrimaryR",   new SliderConfig("PrimaryR",   "Primary R",   x[0], y[0],  R, loC,  hiC) },
                        { "PrimaryG",   new SliderConfig("PrimaryG",   "Primary G",   x[0], y[1],  G, loC,  hiC) },
                        { "PrimaryB",   new SliderConfig("PrimaryB",   "Primary B",   x[0], y[2],  B, loC,  hiC) },
                        { "PrimaryA",   new SliderConfig("PrimaryA",   "Primary A",   x[0], y[3],  A, loNC, hiNC) },
                        { "SecondaryR", new SliderConfig("SecondaryR", "Secondary R", x[0], y[4],  R, loC,  hiC) },
                        { "SecondaryG", new SliderConfig("SecondaryG", "Secondary G", x[0], y[5],  G, loC,  hiC) },
                        { "SecondaryB", new SliderConfig("SecondaryB", "Secondary B", x[0], y[6],  B, loC,  hiC) },
                        { "SecondaryA", new SliderConfig("SecondaryA", "Secondary A", x[0], y[7],  A, loNC, hiNC) },
                        { "TertiaryR",  new SliderConfig("TertiaryR",  "Tertiary R",  x[0], y[8],  R, loC,  hiC) },
                        { "TertiaryG",  new SliderConfig("TertiaryG",  "Tertiary G",  x[0], y[9],  G, loC,  hiC) },
                        { "TertiaryB",  new SliderConfig("TertiaryB",  "Tertiary B",  x[0], y[10], B, loC,  hiC) },
                        { "TertiaryA",  new SliderConfig("TertiaryA",  "Tertiary A",  x[0], y[11], A, loNC, hiNC) },
                        { "PrimaryX",   new SliderConfig("PrimaryX",   "Primary X",   x[1], y[0],  A, loNC, hiNC) },
                        { "PrimaryY",   new SliderConfig("PrimaryY",   "Primary Y",   x[1], y[1],  A, loNC, hiNC) },
                        { "PrimaryZ",   new SliderConfig("PrimaryZ",   "Primary Z",   x[1], y[2],  A, loNC, hiNC) },
                        { "PrimaryW",   new SliderConfig("PrimaryW",   "Primary W",   x[1], y[3],  B, loNC, hiNC) },
                        { "SecondaryX", new SliderConfig("SecondaryX", "Secondary X", x[1], y[4],  A, loNC, hiNC) },
                        { "SecondaryY", new SliderConfig("SecondaryY", "Secondary Y", x[1], y[5],  A, loNC, hiNC) },
                        { "SecondaryZ", new SliderConfig("SecondaryZ", "Secondary Z", x[1], y[6],  A, loNC, hiNC) },
                        { "SecondaryW", new SliderConfig("SecondaryW", "Secondary W", x[1], y[7],  B, loNC, hiNC) },
                        { "TertiaryX",  new SliderConfig("TertiaryX",  "Tertiary X",  x[1], y[8],  A, loNC, hiNC) },
                        { "TertiaryY",  new SliderConfig("TertiaryY",  "Tertiary Y",  x[1], y[9],  A, loNC, hiNC) },
                        { "TertiaryZ",  new SliderConfig("TertiaryZ",  "Tertiary Z",  x[1], y[10], A, loNC, hiNC) },
                        { "TertiaryW",  new SliderConfig("TertiaryW",  "Tertiary W",  x[1], y[11], B, loNC, hiNC) },
                        { "EffectX",    new SliderConfig("EffectX",    "Effect X",    x[1], y[12], A, loNC, hiNC) },
                        { "EffectY",    new SliderConfig("EffectY",    "Effect Y",    x[1], y[13], A, loNC, hiNC) },
                        { "EffectZ",    new SliderConfig("EffectZ",    "Effect Z",    x[1], y[14], A, loNC, hiNC) },
                        { "EffectW",    new SliderConfig("EffectW",    "Effect W",    x[1], y[15], B, loNC, hiNC) },
                    };

                    // create a new pane
                    var uiRoot = __instance.liveryRootObject.transform;

                    paneGO = new GameObject("LiveryAdvancedPane");
                    paneGO.transform.SetParent(uiRoot, false);
                    paneGO.AddComponent<LiveryAdvancedPaneMarker>();
                    
                    // pane background?
                    #if false
                        #if false
                            var bgPrefab = someExistingPanelBackground; // todo? no?
                            var bg = GameObject.Instantiate(bgPrefab, paneGO.transform, false);
                        #else
                            var sprite = paneGO.AddComponent<UISprite>();
                            sprite.color = new Color(0f, 0f, 0f, 0.6f);
                            sprite.depth = 50;
                            sprite.width = 320;
                            sprite.height = 400;
                            sprite.pivot = UIWidget.Pivot.TopLeft;
                        #endif
                    #endif

                    paneGO.transform.localPosition = new Vector3(613f, -4f, 0f); // instead, anchor to edge of screen? particularly the right-side column?

                    // add sliders to pane
                    // (by cloning the 'options menu' prefab sliders)
                    var helperPrefab = CIViewPauseOptions.ins.settingPrefab;

                    foreach (var item in slider_configs) {
                        string key = item.Key;
                        SliderConfig cfg = item.Value;

                        var helperGO = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);
                        helperGO.name = cfg.name;
                        CIHelperSetting helper = helperGO.GetComponent<CIHelperSetting>();
                        helper.sharedLabelName.text = cfg.label;
                        helperGO.transform.localPosition = new Vector3(cfg.x_pos, cfg.y_pos, 0f);

                        Vector3 sliderLocalPos = helper.sliderHolder.transform.localPosition;
                        sliderLocalPos.x -= 262f;
                        helper.sliderHolder.transform.localPosition = sliderLocalPos;

                        helper.sliderBar.valueMin    = cfg.min;
                        helper.sliderBar.valueLimit  = cfg.max;
                        helper.sliderBar.labelFormat = "F3";
                        helper.sliderBar.labelSuffix = "";
                        helper.sliderBar.spriteFill.color = cfg.fill_color;

                        helper.sliderBar.callbackOnAdjustment = new UICallback(value =>
                        {
                            UpdateLiveryFromSliders();
                            RefreshSphereAndMechPreviews();
                        }, 0f);

                        helper.toggleHolder.SetActive(false);
                        helper.levelHolder.SetActive(false);
                        helper.sliderHolder.SetActive(true);

                        slider_helpers.Add(key, helper);
                    }

                    paneGO.SetActive(true); // todo: paneGO.SetActive(!paneGO.activeSelf);
                }
                
                SyncSlidersFromLivery(GetSelectedLivery());
            }//Postfix()

            private static DataContainerEquipmentLivery GetSelectedLivery()
            {
                string key = CIViewBaseLoadout.selectedUnitLivery;
                if (string.IsNullOrEmpty(key))
                    return null;

                return DataMultiLinker<DataContainerEquipmentLivery>.GetEntry(key, false);
            }

            static void SyncSlidersFromLivery(DataContainerEquipmentLivery livery)
            {
                if (livery == null) return;
                slider_helpers["PrimaryR"].sliderBar.valueRaw   = livery.colorPrimary.r;
                slider_helpers["PrimaryG"].sliderBar.valueRaw   = livery.colorPrimary.g;
                slider_helpers["PrimaryB"].sliderBar.valueRaw   = livery.colorPrimary.b;
                slider_helpers["PrimaryA"].sliderBar.valueRaw   = livery.colorPrimary.a;
                slider_helpers["SecondaryR"].sliderBar.valueRaw = livery.colorSecondary.r;
                slider_helpers["SecondaryG"].sliderBar.valueRaw = livery.colorSecondary.g;
                slider_helpers["SecondaryB"].sliderBar.valueRaw = livery.colorSecondary.b;
                slider_helpers["SecondaryA"].sliderBar.valueRaw = livery.colorSecondary.a;
                slider_helpers["TertiaryR"].sliderBar.valueRaw  = livery.colorTertiary.r;
                slider_helpers["TertiaryG"].sliderBar.valueRaw  = livery.colorTertiary.g;
                slider_helpers["TertiaryB"].sliderBar.valueRaw  = livery.colorTertiary.b;
                slider_helpers["TertiaryA"].sliderBar.valueRaw  = livery.colorTertiary.a;
                slider_helpers["PrimaryX"].sliderBar.valueRaw   = livery.materialPrimary.x;
                slider_helpers["PrimaryY"].sliderBar.valueRaw   = livery.materialPrimary.y;
                slider_helpers["PrimaryZ"].sliderBar.valueRaw   = livery.materialPrimary.z;
                slider_helpers["PrimaryW"].sliderBar.valueRaw   = livery.materialPrimary.w;
                slider_helpers["SecondaryX"].sliderBar.valueRaw = livery.materialSecondary.x;
                slider_helpers["SecondaryY"].sliderBar.valueRaw = livery.materialSecondary.y;
                slider_helpers["SecondaryZ"].sliderBar.valueRaw = livery.materialSecondary.z;
                slider_helpers["SecondaryW"].sliderBar.valueRaw = livery.materialSecondary.w;
                slider_helpers["TertiaryX"].sliderBar.valueRaw  = livery.materialTertiary.x;
                slider_helpers["TertiaryY"].sliderBar.valueRaw  = livery.materialTertiary.y;
                slider_helpers["TertiaryZ"].sliderBar.valueRaw  = livery.materialTertiary.z;
                slider_helpers["TertiaryW"].sliderBar.valueRaw  = livery.materialTertiary.w;
                slider_helpers["EffectX"].sliderBar.valueRaw    = livery.effect.x;
                slider_helpers["EffectY"].sliderBar.valueRaw    = livery.effect.y;
                slider_helpers["EffectZ"].sliderBar.valueRaw    = livery.effect.z;
                slider_helpers["EffectW"].sliderBar.valueRaw    = livery.effect.w;
            }

            static void UpdateLiveryFromSliders() {
                var livery = GetSelectedLivery();
                if (livery == null)
                    return;
                livery.colorPrimary.r      = slider_helpers["PrimaryR"].sliderBar.valueRaw;
                livery.colorPrimary.g      = slider_helpers["PrimaryG"].sliderBar.valueRaw;
                livery.colorPrimary.b      = slider_helpers["PrimaryB"].sliderBar.valueRaw;
                livery.colorPrimary.a      = slider_helpers["PrimaryA"].sliderBar.valueRaw;
                livery.colorSecondary.r    = slider_helpers["SecondaryR"].sliderBar.valueRaw;
                livery.colorSecondary.g    = slider_helpers["SecondaryG"].sliderBar.valueRaw;
                livery.colorSecondary.b    = slider_helpers["SecondaryB"].sliderBar.valueRaw;
                livery.colorSecondary.a    = slider_helpers["SecondaryA"].sliderBar.valueRaw;
                livery.colorTertiary.r     = slider_helpers["TertiaryR"].sliderBar.valueRaw;
                livery.colorTertiary.g     = slider_helpers["TertiaryG"].sliderBar.valueRaw;
                livery.colorTertiary.b     = slider_helpers["TertiaryB"].sliderBar.valueRaw;
                livery.colorTertiary.a     = slider_helpers["TertiaryA"].sliderBar.valueRaw;
                livery.materialPrimary.x   = slider_helpers["PrimaryX"].sliderBar.valueRaw;
                livery.materialPrimary.y   = slider_helpers["PrimaryY"].sliderBar.valueRaw;
                livery.materialPrimary.z   = slider_helpers["PrimaryZ"].sliderBar.valueRaw;
                livery.materialPrimary.w   = slider_helpers["PrimaryW"].sliderBar.valueRaw;
                livery.materialSecondary.x = slider_helpers["SecondaryX"].sliderBar.valueRaw;
                livery.materialSecondary.y = slider_helpers["SecondaryY"].sliderBar.valueRaw;
                livery.materialSecondary.z = slider_helpers["SecondaryZ"].sliderBar.valueRaw;
                livery.materialSecondary.w = slider_helpers["SecondaryW"].sliderBar.valueRaw;
                livery.materialTertiary.x  = slider_helpers["TertiaryX"].sliderBar.valueRaw;
                livery.materialTertiary.y  = slider_helpers["TertiaryY"].sliderBar.valueRaw;
                livery.materialTertiary.z  = slider_helpers["TertiaryZ"].sliderBar.valueRaw;
                livery.materialTertiary.w  = slider_helpers["TertiaryW"].sliderBar.valueRaw;
                livery.effect.x            = slider_helpers["EffectX"].sliderBar.valueRaw;
                livery.effect.y            = slider_helpers["EffectY"].sliderBar.valueRaw;
                livery.effect.z            = slider_helpers["EffectZ"].sliderBar.valueRaw;
                livery.effect.w            = slider_helpers["EffectW"].sliderBar.valueRaw;
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
