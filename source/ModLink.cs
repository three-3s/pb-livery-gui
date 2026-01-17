using Entitas;
using HarmonyLib;
using PhantomBrigade;
using PhantomBrigade.Data;
using PhantomBrigade.Mods;
using System.Collections.Generic;
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

// TODO:
//  - Need to avoid modifying the built-in liveries. Would like a copy-to-new button. Maybe save
//    all the liveries to a master .yml file in the AppData (or a folder with multiple). And load
//    those at startup.

namespace ModExtensions
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
            Debug.Log($"OnLoad | Mod: {modID} | Index: {modIndexPreload} | Path: {modPath}");
        }
#endif
    }//class

    // marker for whether we've initialized our LiveryGUI sliders into a given root-GUI-thingy yet
    class LiveryAdvancedPaneMarker : MonoBehaviour {}

    public class SliderConfig {
        public string name;
        public string label;
        public Color fill_color;
        public float min;
        public float max;
        public SliderConfig(string name_in, string label_in, Color fill_color_in, float min_in = 0f, float max_in = 1f) {
            name = name_in;
            label = label_in;
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
        static CIButton toggleLiveryGUIButton;

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
                //Debug.Log($"RedrawLiveryGUI(socketTarget={socketTarget},hardpointTarget={hardpointTarget},closeOnRepeat={closeOnRepeat})");

                if (paneGO == null)
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    // create a new pane
                    var uiRoot = __instance.liveryRootObject.transform;
                    float uiScale = uiRoot.lossyScale.x;
                    UIRoot root = __instance.gameObject.GetComponentInParent<UIRoot>();
                    int activeHeight = root.activeHeight;
                    float pixelSizeAdj = root.pixelSizeAdjustment;

                    paneGO = new GameObject("LiveryAdvancedPane");
                    paneGO.transform.SetParent(uiRoot, false);
                    paneGO.AddComponent<LiveryAdvancedPaneMarker>();
                    paneGO.transform.localPosition = new Vector3(0f, -4f, 0f); // instead, anchor to edge of screen? particularly the right-side column?

                    ////////////////////////////////////////////////////////////////////////////////
                    // slider-bars for customizing the current livery
                    const float loC  =  0f; // min color-slider val (color-component < 0 causes the whole color to be black)
                    const float hiC  =  2f; // max color-slider val
                    const float loNC = -2f; // min non-color-slider
                    const float hiNC =  2f; // max non-color-slider
                    Color R = new Color(0.6f, 0.1f, 0.1f, 0.5f);
                    Color G = new Color(0.1f, 0.6f, 0.1f, 0.5f);
                    Color B = new Color(0.1f, 0.1f, 0.8f, 0.5f);
                    Color A = new Color(1f, 1f, 1f, 0.37f);
                    slider_helpers = new Dictionary<string, CIHelperSetting>();
                    slider_configs = new Dictionary<string, SliderConfig>() {
                        { "PrimaryR",   new SliderConfig("PrimaryR",   "Primary R",   R, loC,  hiC) },
                        { "PrimaryG",   new SliderConfig("PrimaryG",   "Primary G",   G, loC,  hiC) },
                        { "PrimaryB",   new SliderConfig("PrimaryB",   "Primary B",   B, loC,  hiC) },
                        { "PrimaryA",   new SliderConfig("PrimaryA",   "Primary A",   A, loNC, hiNC) },
                        { "SecondaryR", new SliderConfig("SecondaryR", "Secondary R", R, loC,  hiC) },
                        { "SecondaryG", new SliderConfig("SecondaryG", "Secondary G", G, loC,  hiC) },
                        { "SecondaryB", new SliderConfig("SecondaryB", "Secondary B", B, loC,  hiC) },
                        { "SecondaryA", new SliderConfig("SecondaryA", "Secondary A", A, loNC, hiNC) },
                        { "TertiaryR",  new SliderConfig("TertiaryR",  "Tertiary R",  R, loC,  hiC) },
                        { "TertiaryG",  new SliderConfig("TertiaryG",  "Tertiary G",  G, loC,  hiC) },
                        { "TertiaryB",  new SliderConfig("TertiaryB",  "Tertiary B",  B, loC,  hiC) },
                        { "TertiaryA",  new SliderConfig("TertiaryA",  "Tertiary A",  A, loNC, hiNC) },
                        { "PrimaryX",   new SliderConfig("PrimaryX",   "Primary X",   A, loNC, hiNC) },
                        { "PrimaryY",   new SliderConfig("PrimaryY",   "Primary Y",   A, loNC, hiNC) },
                        { "PrimaryZ",   new SliderConfig("PrimaryZ",   "Primary Z",   A, loNC, hiNC) },
                        { "PrimaryW",   new SliderConfig("PrimaryW",   "Primary W",   B, loNC, hiNC) },
                        { "SecondaryX", new SliderConfig("SecondaryX", "Secondary X", A, loNC, hiNC) },
                        { "SecondaryY", new SliderConfig("SecondaryY", "Secondary Y", A, loNC, hiNC) },
                        { "SecondaryZ", new SliderConfig("SecondaryZ", "Secondary Z", A, loNC, hiNC) },
                        { "SecondaryW", new SliderConfig("SecondaryW", "Secondary W", B, loNC, hiNC) },
                        { "TertiaryX",  new SliderConfig("TertiaryX",  "Tertiary X",  A, loNC, hiNC) },
                        { "TertiaryY",  new SliderConfig("TertiaryY",  "Tertiary Y",  A, loNC, hiNC) },
                        { "TertiaryZ",  new SliderConfig("TertiaryZ",  "Tertiary Z",  A, loNC, hiNC) },
                        { "TertiaryW",  new SliderConfig("TertiaryW",  "Tertiary W",  B, loNC, hiNC) },
                        { "EffectX",    new SliderConfig("EffectX",    "Effect X",    A, loNC, hiNC) },
                        { "EffectY",    new SliderConfig("EffectY",    "Effect Y",    A, loNC, hiNC) },
                        { "EffectZ",    new SliderConfig("EffectZ",    "Effect Z",    A, loNC, hiNC) },
                        { "EffectW",    new SliderConfig("EffectW",    "Effect W",    B, loNC, hiNC) },
                    };

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

                        Vector3 sliderLocalPos = helper.sliderHolder.transform.localPosition;
                        sliderLocalPos.x -= 262f; // move the slider-bar left so it's under the label-text
                        helper.sliderHolder.transform.localPosition = sliderLocalPos;

                        helper.sliderBar.valueMin    = cfg.min;
                        helper.sliderBar.valueLimit  = cfg.max;
                        helper.sliderBar.labelFormat = "F3";
                        helper.sliderBar.labelSuffix = "";
                        helper.sliderBar.spriteFill.color = cfg.fill_color;

                        helper.sharedSpriteBackground.gameObject.SetActive(false);
                        helper.sharedSpriteGradient.gameObject.SetActive(false);
                        helper.scrollElement.buttonBody.tooltipUsed = false;

                        helper.sliderBar.callbackOnAdjustment = new UICallback(value =>
                        {
                            UpdateLiveryFromSliders();
                            RefreshSphereAndMechPreviews();
                        }, 0f);

                        helper.toggleHolder.SetActive(false);
                        helper.levelHolder.SetActive(false);
                        helper.sliderHolder.SetActive(true); // (affects visibility of the sliderBar itself)

                        slider_helpers.Add(key, helper);
                    }

                    paneGO.SetActive(true);

                    ////////////////////////////////////////////////////////////////////////////////
                    // toggle-button for Livery GUI visibility
                    GameObject toggleLiveryGUIButtonGO = GameObject.Instantiate(CIViewBaseLoadout.ins.headerButtonLivery.gameObject, uiRoot, false);
                    toggleLiveryGUIButtonGO.name = "LiveryAdvancedToggle";
                    toggleLiveryGUIButtonGO.transform.localPosition = new Vector3(645f, +70f, 0f);
                    toggleLiveryGUIButtonGO.transform.localScale = Vector3.one;
                    toggleLiveryGUIButton = toggleLiveryGUIButtonGO.GetComponent<CIButton>();

                    var toggle_icon = toggleLiveryGUIButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                    var toggle_frame = toggleLiveryGUIButtonGO.transform.Find("Sprite_Frame")?.GetComponent<UISprite>();
                    var toggle_fill_idle = toggleLiveryGUIButtonGO.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
                    var toggle_fill_hover = toggleLiveryGUIButtonGO.transform.Find("Sprite_Fill_Hover")?.GetComponent<UISprite>();
                    if (toggle_icon       != null) toggle_icon.color       = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                    if (toggle_frame      != null) toggle_frame.color      = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                    if (toggle_fill_idle  != null) toggle_fill_idle.color  = new Color(0.2f, 0.2f, 0.2f, 0.4f);
                    if (toggle_fill_hover != null) toggle_fill_hover.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);

                    toggleLiveryGUIButton.callbackOnClick = new UICallback(() =>
                    {
                        paneGO.SetActive(!paneGO.activeSelf);
                    });

                    toggleLiveryGUIButtonGO.SetActive(true);
                }//if(do_once)
                
                UpdateWidgetPositioning(__instance);
                SyncSlidersFromLivery(GetSelectedLivery());
            }//Postfix()

            private static void UpdateWidgetPositioning(CIViewBaseLoadout __instance) {
                var uiRoot = __instance.liveryRootObject.transform;
                float uiScale = uiRoot.lossyScale.x;
                UIRoot root = __instance.gameObject.GetComponentInParent<UIRoot>();
                int activeHeight = root.activeHeight;
                float pixelSizeAdj = root.pixelSizeAdjustment;

                // calculate positions
                const float yStep = 40f;
                float[] x = {
                    613f,
                    (Screen.width * pixelSizeAdj - 334f)
                    // wWen adjusting this "pixel offset from edge", eg if I measure 271px that I want to move something,
                    // manually multiply that number by whatever pixelSizeAdj. So if I want to move 271px left, I take
                    // -271 and multiply by my current pixelSizeAdj (which is related to UI-scale factor from Display
                    // options menu). So the actual number to add here is -271 realPx * 0.75 codePx/realPx = -203 codePx.
                };
                //Debug.Log($"gui-scale-stuff Screen.width={Screen.width}, uiScale={uiScale}, x= {x[0]}, {x[1]}, activeHeight={activeHeight}, pixelSizeAdj={pixelSizeAdj}");
                // Example: gui-scale-stuff Screen.width=2560, uiScale=0.001851852, x= 613, 1586, activeHeight=1080, pixelSizeAdj=0.75
                float[] y = {
                    -yStep *  0.0f,
                    -yStep *  1.0f,
                    -yStep *  2.0f,
                    -yStep *  3.0f,
                    -yStep *  5.0f, // +1.0 gap
                    -yStep *  6.0f,
                    -yStep *  7.0f,
                    -yStep *  8.0f,
                    -yStep * 10.0f, // +1.0 gap
                    -yStep * 11.0f,
                    -yStep * 12.0f,
                    -yStep * 13.0f,
                    -yStep * 15.0f, // +1.0 gap
                    -yStep * 16.0f,
                    -yStep * 17.0f,
                    -yStep * 18.0f,
                };
                slider_helpers["PrimaryR"].gameObject.transform.localPosition   = new Vector3(x[0], y[0]);
                slider_helpers["PrimaryG"].gameObject.transform.localPosition   = new Vector3(x[0], y[1]);
                slider_helpers["PrimaryB"].gameObject.transform.localPosition   = new Vector3(x[0], y[2]);
                slider_helpers["PrimaryA"].gameObject.transform.localPosition   = new Vector3(x[0], y[3]);
                slider_helpers["SecondaryR"].gameObject.transform.localPosition = new Vector3(x[0], y[4]);
                slider_helpers["SecondaryG"].gameObject.transform.localPosition = new Vector3(x[0], y[5]);
                slider_helpers["SecondaryB"].gameObject.transform.localPosition = new Vector3(x[0], y[6]);
                slider_helpers["SecondaryA"].gameObject.transform.localPosition = new Vector3(x[0], y[7]);
                slider_helpers["TertiaryR"].gameObject.transform.localPosition  = new Vector3(x[0], y[8]);
                slider_helpers["TertiaryG"].gameObject.transform.localPosition  = new Vector3(x[0], y[9]);
                slider_helpers["TertiaryB"].gameObject.transform.localPosition  = new Vector3(x[0], y[10]);
                slider_helpers["TertiaryA"].gameObject.transform.localPosition  = new Vector3(x[0], y[11]);
                slider_helpers["PrimaryX"].gameObject.transform.localPosition   = new Vector3(x[1], y[0]);
                slider_helpers["PrimaryY"].gameObject.transform.localPosition   = new Vector3(x[1], y[1]);
                slider_helpers["PrimaryZ"].gameObject.transform.localPosition   = new Vector3(x[1], y[2]);
                slider_helpers["PrimaryW"].gameObject.transform.localPosition   = new Vector3(x[1], y[3]);
                slider_helpers["SecondaryX"].gameObject.transform.localPosition = new Vector3(x[1], y[4]);
                slider_helpers["SecondaryY"].gameObject.transform.localPosition = new Vector3(x[1], y[5]);
                slider_helpers["SecondaryZ"].gameObject.transform.localPosition = new Vector3(x[1], y[6]);
                slider_helpers["SecondaryW"].gameObject.transform.localPosition = new Vector3(x[1], y[7]);
                slider_helpers["TertiaryX"].gameObject.transform.localPosition  = new Vector3(x[1], y[8]);
                slider_helpers["TertiaryY"].gameObject.transform.localPosition  = new Vector3(x[1], y[9]);
                slider_helpers["TertiaryZ"].gameObject.transform.localPosition  = new Vector3(x[1], y[10]);
                slider_helpers["TertiaryW"].gameObject.transform.localPosition  = new Vector3(x[1], y[11]);
                slider_helpers["EffectX"].gameObject.transform.localPosition    = new Vector3(x[1], y[12]);
                slider_helpers["EffectY"].gameObject.transform.localPosition    = new Vector3(x[1], y[13]);
                slider_helpers["EffectZ"].gameObject.transform.localPosition    = new Vector3(x[1], y[14]);
                slider_helpers["EffectW"].gameObject.transform.localPosition    = new Vector3(x[1], y[15]);
            }

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
                CIViewBaseLoadout.ins.Redraw(CIViewBaseLoadout.selectedUnitSocket, CIViewBaseLoadout.selectedUnitHardpoint, false);

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
