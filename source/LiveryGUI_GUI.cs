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

namespace LiveryGUIMod
{
    public enum SliderKind
    {
        Continuous,
        Discrete,
        // Toggle -- supported by the 'helper', but not needed/implemented in LiveryGUI
    }

    // each of these is used to initialize one slider-bar
    public class SliderConfig
    {
        public string name;
        public string label;
        public Color fillColor;
        public float min;
        public float max;
        public SliderKind sliderKind;
        public SliderConfig(string nameIn, string labelIn, Color fillColorIn, float minIn = 0f, float maxIn = 1f, SliderKind sliderKindIn = SliderKind.Continuous)
        {
            name = nameIn;
            label = labelIn;
            fillColor = fillColorIn;
            min = minIn;
            max = maxIn;
            sliderKind = sliderKindIn;
        }
    }

    // relating the SliderKind.Discrete selection to the livery's float contentParameters.w
    public class ContentW
    {
        public static string[] names = { "none (w=0.0)", "dots (w=1.0)", "lines (w=2.0)", "sheen (w=3.0)" };
        public static int idx = 0;
        public static void SetLevel(float val)
        {
            idx = (int) Math.Min(Math.Max(0f, val + 0.1f), (float)names.Length - 1f);
        }
        public static float GetLevelValue()
        {
            return (float)idx;
        }
        public static string GetLevelName()
        {
            return names[idx];
        }
    }

    public class GUI
    {
        public static GameObject paneGO = null;
        public static Dictionary<string, CIHelperSetting> sliderHelpers = null; // key = livery's key
        public static Dictionary<string, SliderConfig> sliderConfigs = null; // key = livery's key
        public static CIButton toggleLiveryGUIButton;
        public static CIButton resetLiveryButton;
        public static CIButton saveLiveryButton;
        public static CIButton cloneLiveryButton;
        public static UIInput liveryNameInput;
        static readonly Color activeButtonFGColor = new Color(0.6f, 0.7f, 1f, 1f);
        static readonly Color activeButtonBGColor = new Color(0.25f, 0.36f, 0.58f, 0.7f);
        static readonly Color grayedOutButtonFGColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        static readonly Color grayedOutButtonBGColor = new Color(0.0f, 0.0f, 0.0f, 0.7f);
        static readonly Color activeRedButtonFGColor = new Color(1f, 0.38f, 0.38f, 1f);
        static readonly Color activeRedButtonBGColor = new Color(0.61f, 0.22f, 0.22f, 1f);
        static SliderRightClickHandler sliderRightClickHandler = null;

        static readonly string spriteNameStarFilled = "s_icon_l32_star_filled";
        static readonly string spriteNameStarOutline = "s_icon_l32_star_outline";

        //==============================================================================
        public static void RedrawLiveryGUI(CIViewBaseLoadout __instance)
        {
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
                paneGO.transform.localPosition = new Vector3(0f, 0f, 0f);

                ////////////////////////////////////////////////////////////////////////////////
                // slider-bars for customizing the current livery
                const float loC = 0f; // min color-slider val (color-component < 0 causes the whole color to be black)
                const float hiC = +2f; // max color-slider val
                const float loS = -0.5f; // min shininess
                const float hiS = +1.5f; // max shininess
                const float loA = -20f; // min alpha
                const float hiA = +5f;  // max alpha
                const float loNC = -5f; // min non-color-slider
                const float hiNC = +5f; // max non-color-slider
                Color R = new Color(0.6f, 0.1f, 0.1f, 0.5f);
                Color G = new Color(0.1f, 0.6f, 0.1f, 0.5f);
                Color B = new Color(0.1f, 0.1f, 0.8f, 0.5f);
                Color A = new Color(1f, 1f, 1f, 0.37f);
                sliderHelpers = new Dictionary<string, CIHelperSetting>();
                sliderConfigs = new Dictionary<string, SliderConfig>() {
                        { "PrimaryR",   new SliderConfig("PrimaryR",   "Primary R",   R, loC,  hiC) },
                        { "PrimaryG",   new SliderConfig("PrimaryG",   "Primary G",   G, loC,  hiC) },
                        { "PrimaryB",   new SliderConfig("PrimaryB",   "Primary B",   B, loC,  hiC) },
                        { "PrimaryA",   new SliderConfig("PrimaryA",   "Primary A",   A, loA,  hiA) },
                        { "SecondaryR", new SliderConfig("SecondaryR", "Secondary R", R, loC,  hiC) },
                        { "SecondaryG", new SliderConfig("SecondaryG", "Secondary G", G, loC,  hiC) },
                        { "SecondaryB", new SliderConfig("SecondaryB", "Secondary B", B, loC,  hiC) },
                        { "SecondaryA", new SliderConfig("SecondaryA", "Secondary A", A, loA,  hiA) },
                        { "TertiaryR",  new SliderConfig("TertiaryR",  "Tertiary R",  R, loC,  hiC) },
                        { "TertiaryG",  new SliderConfig("TertiaryG",  "Tertiary G",  G, loC,  hiC) },
                        { "TertiaryB",  new SliderConfig("TertiaryB",  "Tertiary B",  B, loC,  hiC) },
                        { "TertiaryA",  new SliderConfig("TertiaryA",  "Tertiary A",  A, loA,  hiA) },
                        { "ContentX",   new SliderConfig("ContentX",   "Supporter DLC X", R, loNC, hiNC) },
                        { "ContentY",   new SliderConfig("ContentY",   "Supporter DLC Y", G, loNC, hiNC) },
                        { "ContentZ",   new SliderConfig("ContentZ",   "Supporter DLC Z", B, loNC, hiNC) },
                        { "ContentW",   new SliderConfig("ContentW",   "Supporter DLC W", A, loNC, hiNC, SliderKind.Discrete) },
                        { "PrimaryX",   new SliderConfig("PrimaryX",   "Primary X",   A, loS,  hiS)  },
                        { "PrimaryY",   new SliderConfig("PrimaryY",   "Primary Y",   A, loS,  hiS)  },
                        { "PrimaryZ",   new SliderConfig("PrimaryZ",   "Primary Z",   A, loS,  hiS)  },
                        { "PrimaryW",   new SliderConfig("PrimaryW",   "Primary W",   B, loNC, hiNC) },
                        { "SecondaryX", new SliderConfig("SecondaryX", "Secondary X", A, loS,  hiS)  },
                        { "SecondaryY", new SliderConfig("SecondaryY", "Secondary Y", A, loS,  hiS)  },
                        { "SecondaryZ", new SliderConfig("SecondaryZ", "Secondary Z", A, loS,  hiS)  },
                        { "SecondaryW", new SliderConfig("SecondaryW", "Secondary W", B, loNC, hiNC) },
                        { "TertiaryX",  new SliderConfig("TertiaryX",  "Tertiary X",  A, loS,  hiS)  },
                        { "TertiaryY",  new SliderConfig("TertiaryY",  "Tertiary Y",  A, loS,  hiS)  },
                        { "TertiaryZ",  new SliderConfig("TertiaryZ",  "Tertiary Z",  A, loS,  hiS)  },
                        { "TertiaryW",  new SliderConfig("TertiaryW",  "Tertiary W",  B, loNC, hiNC) },
                        { "EffectX",    new SliderConfig("EffectX",    "Effect X",    A, loNC, hiNC) },
                        { "EffectY",    new SliderConfig("EffectY",    "Effect Y",    A, loNC, hiNC) },
                        { "EffectZ",    new SliderConfig("EffectZ",    "Effect Z",    A, loNC, hiNC) },
                        { "EffectW",    new SliderConfig("EffectW",    "Effect W",    B, loNC, hiNC) },
                    };

                // add sliders to pane
                // (by cloning the 'options menu' prefab sliders)
                var helperPrefab = CIViewPauseOptions.ins.settingPrefab;

                foreach (var item in sliderConfigs)
                {
                    string key = item.Key;
                    SliderConfig cfg = item.Value;

                    var helperGO = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);
                    helperGO.name = cfg.name;
                    CIHelperSetting helper = helperGO.GetComponent<CIHelperSetting>();
                    helper.sharedLabelName.text = cfg.label;

                    helper.sliderBar.callbackOnClickSecondary = new UICallback(value =>
                    {
                        OnClickSecondary(helper);
                    }, 0f);

                    Vector3 sliderLocalPos = helper.sliderHolder.transform.localPosition;
                    sliderLocalPos.x -= 262f; // move the slider-bar left so it's under the label-text
                    helper.sliderHolder.transform.localPosition = sliderLocalPos;

                    helper.sliderBar.valueMin = cfg.min;
                    helper.sliderBar.valueLimit = cfg.max;
                    helper.sliderBar.labelFormat = "F3";
                    helper.sliderBar.labelSuffix = "";
                    helper.sliderBar.spriteFill.color = cfg.fillColor;

                    helper.sharedSpriteBackground.gameObject.SetActive(false);
                    helper.sharedSpriteGradient.gameObject.SetActive(false);
                    helper.scrollElement.buttonBody.tooltipUsed = false;

                    helper.sliderBar.callbackOnAdjustment = new UICallback(value =>
                    {
                        UpdateLiveryFromSliders();
                        RefreshSphereAndMechPreviews();
                    }, 0f);

                    // which aspect of the widget should be visible
                    helper.sliderHolder.SetActive(false);
                    helper.levelHolder.SetActive(false);
                    helper.toggleHolder.SetActive(false);
                    if (cfg.sliderKind == SliderKind.Continuous)
                    {
                        helper.sliderHolder.SetActive(true);
                    } else if (cfg.sliderKind == SliderKind.Discrete) {
                        helper.levelHolder.SetActive(true);
                        helper.levelHolder.transform.localPosition = sliderLocalPos + new Vector3(68f, 5f);
                        helper.levelButtonLeft.transform.localPosition += new Vector3(68f, -5f);
                        helper.levelButtonRight.transform.localPosition += new Vector3(-68f, -5f);
                        //helper.sharedSpriteGradient.transform.localScale = new Vector3(0.518f, 0.919f); // not sure changing the sharedSpriteGradient doesn't affect Options menu
                        //helper.sharedSpriteGradient.SetRGBColor(new Color(0.0f, 0.0f, 0.0f, 1f)); // very faint
                        //helper.sharedSpriteGradient.gameObject.SetActive(true);
                        helper.levelButtonLeft.callbackOnClick = new UICallback(() =>
                        {
                            ContentW.idx = (ContentW.idx - 1 + ContentW.names.Length) % ContentW.names.Length;
                            UpdateLiveryFromSliders();
                            RefreshSphereAndMechPreviews();
                        });
                        helper.levelButtonRight.callbackOnClick = new UICallback(() =>
                        {
                            ContentW.idx = (ContentW.idx + 1) % ContentW.names.Length;
                            UpdateLiveryFromSliders();
                            RefreshSphereAndMechPreviews();
                        });
                    }

                    sliderHelpers.Add(key, helper);
                }

                ////////////////////////////////////////////////////////////////////////////////
                // toggle-button for Livery GUI visibility
                GameObject toggleLiveryGUIButtonGO = GameObject.Instantiate(CIViewBaseLoadout.ins.headerButtonLivery.gameObject, uiRoot, false);
                toggleLiveryGUIButtonGO.name = "LiveryAdvancedToggle";
                toggleLiveryGUIButtonGO.transform.localPosition = new Vector3(645f, +70f, 0f);
                toggleLiveryGUIButtonGO.transform.localScale = Vector3.one;

                var toggleIcon = toggleLiveryGUIButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                var toggleFrame = toggleLiveryGUIButtonGO.transform.Find("Sprite_Frame")?.GetComponent<UISprite>();
                var toggleFillIdle = toggleLiveryGUIButtonGO.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
                var toggleFillHover = toggleLiveryGUIButtonGO.transform.Find("Sprite_Fill_Hover")?.GetComponent<UISprite>();
                if (toggleIcon != null) { toggleIcon.color = new Color(0.9f, 0.9f, 0.9f, 0.8f); toggleIcon.spriteName = "s_icon_l32_menu_edit4"; }
                if (toggleFrame != null) toggleFrame.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                if (toggleFillIdle != null) toggleFillIdle.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
                if (toggleFillHover != null) toggleFillHover.color = new Color(0.7f, 0.7f, 0.7f, 0.4f);

                toggleLiveryGUIButton = toggleLiveryGUIButtonGO.GetComponent<CIButton>();
                toggleLiveryGUIButton.callbackOnClick = new UICallback(() =>
                {
                    paneGO.SetActive(!paneGO.activeSelf);

                    // first time wasn't updating the text-input-field with the livery-name. i don't know why.
                    // INVOKE ALL THE REFRESH. MULTIPLY. STILL DOESN'T WORK FIRST TIME. ONLY SECOND TIME. ONLY SECOND TIME.
                    string origLiveryKey = CIViewBaseLoadout.selectedUnitLivery;
                    if (origLiveryKey == null && DataMultiLinkerEquipmentLivery.data.Count > 0)
                        SelectLivery(DataMultiLinkerEquipmentLivery.data.First().Key);
                    else
                        SelectLivery(null);
                    SelectLivery(origLiveryKey);
                    liveryNameInput.ForceSelection(true);
                    liveryNameInput.ForceSelection(false);
                    liveryNameInput.label.MarkAsChanged();
                    RefreshSphereAndMechPreviews();
                    UpdateLiveryListTooltips();
                    UpdateWidgetPositioning(__instance);
                    ResetLiveryGUIWidgetsToMatchLivery(GetSelectedLivery());
                    RefreshSphereAndMechPreviews();

                    // todo: some of this refresh-spam seems to be causing the audio to play multiple times (ie louder).
                    // todo: maybe try letting the text-input always be active (on the livery page), just shoved offscreen
                });

                toggleLiveryGUIButtonGO.SetActive(true);

                ////////////////////////////////////////////////////////////////////////////////
                // Livery GUI buttons: 'revert changes', 'clone livery', 'save livery to disk'
                Vector3 posStep = new Vector3(80f, 0f, 0f);

                GameObject resetLiveryButtonGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
                resetLiveryButtonGO.name = "resetLiveryButtonGO";
                resetLiveryButtonGO.transform.localPosition += posStep;
                var resetIcon = resetLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                if (resetIcon != null) { resetIcon.color = new Color(0.5f, 0.6f, 0.8f, 0.8f); resetIcon.spriteName = "s_icon_l32_retreat"; }

                GameObject saveLiveryButtonGO = GameObject.Instantiate(resetLiveryButtonGO, paneGO.transform, false);
                saveLiveryButtonGO.name = "saveLiveryButtonGO";
                saveLiveryButtonGO.transform.localPosition += posStep;
                var saveIcon = saveLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                if (saveIcon != null) { saveIcon.color = new Color(0.5f, 0.6f, 0.8f, 0.8f); saveIcon.spriteName = "s_icon_l32_lc_save1"; }

                GameObject cloneLiveryButtonGO = GameObject.Instantiate(saveLiveryButtonGO, paneGO.transform, false);
                cloneLiveryButtonGO.name = "cloneLiveryButtonGO";
                cloneLiveryButtonGO.transform.localPosition += posStep;
                var cloneIcon = cloneLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                if (cloneIcon != null) { cloneIcon.color = new Color(0.5f, 0.8f, 0.6f, 0.8f); cloneIcon.spriteName = "s_icon_l32_lc_grid_plus"; }

                resetLiveryButton = resetLiveryButtonGO.GetComponent<CIButton>();
                resetLiveryButton.callbackOnClick = new UICallback(() =>
                {
                    OnResetLiveryClicked();
                });

                saveLiveryButton = saveLiveryButtonGO.GetComponent<CIButton>();
                saveLiveryButton.callbackOnClick = new UICallback(() =>
                {
                    if (CIViewBaseLoadout.selectedUnitLivery != liveryNameInput.value)
                        OnCloneLiveryClicked(); // new key/name was given; clone livery before saving it
                    if (CIViewBaseLoadout.selectedUnitLivery == liveryNameInput.value)
                    {
                        LoadAndSave.SaveLiveryToFile(CIViewBaseLoadout.selectedUnitLivery, GetSelectedLivery());
                        UpdateButtonColors();
                    }
                });

                cloneLiveryButton = cloneLiveryButtonGO.GetComponent<CIButton>();
                cloneLiveryButton.callbackOnClick = new UICallback(() =>
                {
                    OnCloneLiveryClicked();
                });

                ////////////////////////////////////////////////////////////////////////////////
                // Livery GUI text-input field: for seeing & renaming the livery-key/file-name
                GameObject liveryNameInputGO = GameObject.Instantiate(CIViewBaseLoadout.ins.headerInputUnitName.gameObject, uiRoot/*paneGO.transform*/, false); // using uiRoot to make this initially-visible as a workaround for the text-display refusing to initially populate until it gets displayed AND THEN user clicks on something (after which it starts updating/working fine)
                liveryNameInputGO.name = "liveryNameInputGO";
                liveryNameInputGO.transform.localPosition = cloneLiveryButtonGO.transform.localPosition + new Vector3(56f, 0f, 0f);
                liveryNameInputGO.transform.localScale = Vector3.one;

                liveryNameInput = liveryNameInputGO.GetComponent<UIInput>();
                liveryNameInput.onChange = new List<EventDelegate>() { new EventDelegate(new EventDelegate.Callback(OnLiveryNameInput)) };
                liveryNameInput.onReturnKey = UIInput.OnReturnKey.Submit;
                liveryNameInput.onSubmit = new List<EventDelegate> { new EventDelegate(new EventDelegate.Callback(OnLiveryNameInput)) };
                liveryNameInput.label.text = "Livery Name Goes Here";
                liveryNameInput.label.ProcessText();
                liveryNameInput.characterLimit += 10;
                liveryNameInput.defaultText = "default?"; // (ends up unused?)

                liveryNameInputGO.SetActive(true);

                ////////////////////////////////////////////////////////////////////////////////
                // Listener for right-click drag & release, for sliders.
                // (Just want one instance. Attached to Save button, somewhat arbitrarily.)
                sliderRightClickHandler = saveLiveryButtonGO.AddComponent<SliderRightClickHandler>();

                ////////////////////////////////////////////////////////////////////////////////
                // Livery GUI initial visibility
                paneGO.SetActive(false);
            }//if(doOnce)

            UpdateLiveryListTooltips();
            UpdateWidgetPositioning(__instance);
            ResetLiveryGUIWidgetsToMatchLivery(GetSelectedLivery());
        }//Postfix()

        //==============================================================================
        static void OnLiveryNameInput()
        {
            // It would be possible to do something like (dangerous?) rename the livery, including changing its dictionary key.
            // This could leave dangling references.
            // Instead, we'll have the on-input field do nothing itself, and let the 'save' & 'create clone' buttons use this
            // field's value as the new key/name for the newly cloned livery.

            UpdateButtonColors();
            Contexts.sharedInstance.game.isInputBlocked = true; // (we've handled/consumed the input-event(s) this frame)
            return;
        }

        //==============================================================================
        static void UpdateWidgetPositioning(CIViewBaseLoadout __instance)
        {
            UIRoot root = __instance.gameObject.GetComponentInParent<UIRoot>();
            float pixelSizeAdj = root.pixelSizeAdjustment;

            var uiRoot = __instance.liveryRootObject.transform;
            float uiScale = uiRoot.lossyScale.x;
            int activeHeight = root.activeHeight;
            _ = uiRoot;
            _ = uiScale;
            _ = activeHeight;

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
            sliderHelpers["PrimaryR"].gameObject.transform.localPosition = new Vector3(x[0], y[0]);
            sliderHelpers["PrimaryG"].gameObject.transform.localPosition = new Vector3(x[0], y[1]);
            sliderHelpers["PrimaryB"].gameObject.transform.localPosition = new Vector3(x[0], y[2]);
            sliderHelpers["PrimaryA"].gameObject.transform.localPosition = new Vector3(x[0], y[3]);
            sliderHelpers["SecondaryR"].gameObject.transform.localPosition = new Vector3(x[0], y[4]);
            sliderHelpers["SecondaryG"].gameObject.transform.localPosition = new Vector3(x[0], y[5]);
            sliderHelpers["SecondaryB"].gameObject.transform.localPosition = new Vector3(x[0], y[6]);
            sliderHelpers["SecondaryA"].gameObject.transform.localPosition = new Vector3(x[0], y[7]);
            sliderHelpers["TertiaryR"].gameObject.transform.localPosition = new Vector3(x[0], y[8]);
            sliderHelpers["TertiaryG"].gameObject.transform.localPosition = new Vector3(x[0], y[9]);
            sliderHelpers["TertiaryB"].gameObject.transform.localPosition = new Vector3(x[0], y[10]);
            sliderHelpers["TertiaryA"].gameObject.transform.localPosition = new Vector3(x[0], y[11]);
            sliderHelpers["ContentX"].gameObject.transform.localPosition = new Vector3(x[0], y[12]);
            sliderHelpers["ContentY"].gameObject.transform.localPosition = new Vector3(x[0], y[13]);
            sliderHelpers["ContentZ"].gameObject.transform.localPosition = new Vector3(x[0], y[14]);
            sliderHelpers["ContentW"].gameObject.transform.localPosition = new Vector3(x[0], y[15]);
            sliderHelpers["PrimaryX"].gameObject.transform.localPosition = new Vector3(x[1], y[0]);
            sliderHelpers["PrimaryY"].gameObject.transform.localPosition = new Vector3(x[1], y[1]);
            sliderHelpers["PrimaryZ"].gameObject.transform.localPosition = new Vector3(x[1], y[2]);
            sliderHelpers["PrimaryW"].gameObject.transform.localPosition = new Vector3(x[1], y[3]);
            sliderHelpers["SecondaryX"].gameObject.transform.localPosition = new Vector3(x[1], y[4]);
            sliderHelpers["SecondaryY"].gameObject.transform.localPosition = new Vector3(x[1], y[5]);
            sliderHelpers["SecondaryZ"].gameObject.transform.localPosition = new Vector3(x[1], y[6]);
            sliderHelpers["SecondaryW"].gameObject.transform.localPosition = new Vector3(x[1], y[7]);
            sliderHelpers["TertiaryX"].gameObject.transform.localPosition = new Vector3(x[1], y[8]);
            sliderHelpers["TertiaryY"].gameObject.transform.localPosition = new Vector3(x[1], y[9]);
            sliderHelpers["TertiaryZ"].gameObject.transform.localPosition = new Vector3(x[1], y[10]);
            sliderHelpers["TertiaryW"].gameObject.transform.localPosition = new Vector3(x[1], y[11]);
            sliderHelpers["EffectX"].gameObject.transform.localPosition = new Vector3(x[1], y[12]);
            sliderHelpers["EffectY"].gameObject.transform.localPosition = new Vector3(x[1], y[13]);
            sliderHelpers["EffectZ"].gameObject.transform.localPosition = new Vector3(x[1], y[14]);
            sliderHelpers["EffectW"].gameObject.transform.localPosition = new Vector3(x[1], y[15]);
        }

        //==============================================================================
        static void UpdateLiveryListTooltips()
        {
#if false // disabled due to buggy, sometimes get stuck up, sometimes stops working until you click. and it's kind of in the way of navigation and not super valuable while browsing
                var liveryOptionInstances = (Dictionary<string, CIHelperLoadoutLivery>) AccessTools.Field(typeof(CIViewBaseLoadout), "liveryOptionInstances")?.GetValue(CIViewBaseLoadout.ins);
                if (liveryOptionInstances == null)
                {
                    Debug.Warning($"[LiveryGUI] BUG: null liveryOptionInstances. isNull={AccessTools.Field(typeof(CIViewBaseLoadout), "liveryOptionInstances")==null},{liveryOptionInstances==null}");
                    return;
                }

                foreach (var item in liveryOptionInstances) {
                    string key = item.Key;
                    CIHelperLoadoutLivery liveryHelper = item.Value;
                    if (key == null || liveryHelper == null)
                    {
                        Debug.Warning($"[LiveryGUI] BUG(?): null key/val encountered: {key}, valIsNull={liveryHelper==null}");
                        continue;
                    }
                    liveryHelper.button.tooltipUsed = true;
                    liveryHelper.button.tooltipKey = null;
                    liveryHelper.button.AddTooltip(key, liveryHelper.name); // the 'title' is REALLY BIG AND CAPITALIZED, so probably leave title null and only use the text (?)
                }
#endif
        }

        //==============================================================================
        static void OnClickSecondary(CIHelperSetting helper)
        {
            //Debug.Log($"[LiveryGUI] OnClickSecondary {helper?.name ?? "{none}"}");
            sliderRightClickHandler?.CaptureRightClickFor(helper.sliderBar);
        }

        //==============================================================================
        static void OnCloneLiveryClicked()
        {
            string newLiveryKey = CloneSelectedLivery();
            if (newLiveryKey == liveryNameInput.value)
            {
                SelectLivery(newLiveryKey);
            }
            RefreshSphereAndMechPreviews();
        }

        //==============================================================================
        static void OnResetLiveryClicked() {
            DataContainerEquipmentLivery origLivery;
            string activeLiveryKeyForLookup;
            if (string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitLivery) ||
                !LiverySnapshotDB.originalLiveries.ContainsKey(CIViewBaseLoadout.selectedUnitLivery))
            {
                Debug.Log($"[LiveryGUI] OnResetLiveryClicked(): livery has null/empty/unknown key={CIViewBaseLoadout.selectedUnitLivery ?? "<null>"}");
                return;
                //liveryNameInput.Set("null");
                //origLivery = LiverySnapshotDB.defaultLivery.onDiskDat;
                //activeLiveryKeyForLookup = "default";
            } else
            {
                liveryNameInput.Set(CIViewBaseLoadout.selectedUnitLivery);
                origLivery = LiverySnapshotDB.originalLiveries[CIViewBaseLoadout.selectedUnitLivery].onDiskDat;
                activeLiveryKeyForLookup = CIViewBaseLoadout.selectedUnitLivery;
            }

            if (!DataMultiLinkerEquipmentLivery.data.ContainsKey(activeLiveryKeyForLookup))
            {
                Debug.LogWarning($"[LiveryGUI] BUG: Couldn't find DataMultiLinkerEquipmentLivery.data key={activeLiveryKeyForLookup}");
            }
            else
            {
                DataContainerEquipmentLivery dstLivery = DataMultiLinkerEquipmentLivery.data[activeLiveryKeyForLookup];
                dstLivery.colorPrimary.r      = origLivery.colorPrimary.r;
                dstLivery.colorPrimary.g      = origLivery.colorPrimary.g;
                dstLivery.colorPrimary.b      = origLivery.colorPrimary.b;
                dstLivery.colorPrimary.a      = origLivery.colorPrimary.a;
                dstLivery.colorSecondary.r    = origLivery.colorSecondary.r;
                dstLivery.colorSecondary.g    = origLivery.colorSecondary.g;
                dstLivery.colorSecondary.b    = origLivery.colorSecondary.b;
                dstLivery.colorSecondary.a    = origLivery.colorSecondary.a;
                dstLivery.colorTertiary.r     = origLivery.colorTertiary.r;
                dstLivery.colorTertiary.g     = origLivery.colorTertiary.g;
                dstLivery.colorTertiary.b     = origLivery.colorTertiary.b;
                dstLivery.colorTertiary.a     = origLivery.colorTertiary.a;
                dstLivery.contentParameters.x = origLivery.contentParameters.x;
                dstLivery.contentParameters.y = origLivery.contentParameters.y;
                dstLivery.contentParameters.z = origLivery.contentParameters.z;
                dstLivery.contentParameters.w = origLivery.contentParameters.w;
                dstLivery.materialPrimary.x   = origLivery.materialPrimary.x;
                dstLivery.materialPrimary.y   = origLivery.materialPrimary.y;
                dstLivery.materialPrimary.z   = origLivery.materialPrimary.z;
                dstLivery.materialPrimary.w   = origLivery.materialPrimary.w;
                dstLivery.materialSecondary.x = origLivery.materialSecondary.x;
                dstLivery.materialSecondary.y = origLivery.materialSecondary.y;
                dstLivery.materialSecondary.z = origLivery.materialSecondary.z;
                dstLivery.materialSecondary.w = origLivery.materialSecondary.w;
                dstLivery.materialTertiary.x  = origLivery.materialTertiary.x;
                dstLivery.materialTertiary.y  = origLivery.materialTertiary.y;
                dstLivery.materialTertiary.z  = origLivery.materialTertiary.z;
                dstLivery.materialTertiary.w  = origLivery.materialTertiary.w;
                dstLivery.effect.x            = origLivery.effect.x;
                dstLivery.effect.y            = origLivery.effect.y;
                dstLivery.effect.z            = origLivery.effect.z;
                dstLivery.effect.w            = origLivery.effect.w;
                CIViewOverworldLog.AddMessage($"Reset livery to last saved version. [sp=s_icon_l32_retreat]");
            }

            RefreshSphereAndMechPreviews();
        }

        //==============================================================================
        static string CloneSelectedLivery()
        {
            if (CIViewBaseLoadout.ins == null) return null;

            var liveriesDict = DataMultiLinkerEquipmentLivery.data;
            string newKey = liveryNameInput.value; // (from the text-input field)
            if (string.IsNullOrEmpty(newKey))
                return null;

            if (liveriesDict.ContainsKey(newKey))
            {
                Debug.LogWarning($"[LiveryGUI] USAGE: Refusing to clone livery {newKey}: That key already exists.");
                CIViewOverworldLog.AddMessage($"No. A livery with that Name already exists. [sp=s_icon_l32_cancel]");
                return null;
            }

            DataContainerEquipmentLivery newCopy;

            string origKey = CIViewBaseLoadout.selectedUnitLivery;
            if (string.IsNullOrEmpty(origKey) || !liveriesDict.TryGetValue(origKey, out var original))
            {
                newCopy = new DataContainerEquipmentLivery();
            }
            else
            {
                newCopy = LiverySnapshotDB.DeepCopyLiveryDat(original);
            }

            newCopy.key = newKey;
            newCopy.textName = newKey;
            newCopy.source = $"LiveryGUI";

            liveriesDict[newKey] = newCopy;
            DataMultiLinkerEquipmentLivery.OnAfterDeserialization(); // (triggers rebuilding its .dataSorted)

            LiverySnapshotDB.AddLiveryDataSnapshot(newKey, newCopy, true);

            Debug.Log($"[LiveryGUI] INFO: cloned livery {origKey} to {newKey}");
            CIViewOverworldLog.AddMessage($"Created a new copy of that livery. [sp=s_icon_l32_lc_grid_plus]");
            return newKey;
        }

        //==============================================================================
        static void SelectLivery(string liveryKey)
        {
            if (string.IsNullOrEmpty(liveryKey))
            {
                if(!string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitLivery))
                    liveryKey = CIViewBaseLoadout.selectedUnitLivery; // This "re-selects" it, which per the below call to OnLiveryAttachAttempt, toggles/de-selects it. (Passing a null key gets ignored.)
                // else: selected livery is already null
            }
            else
            {
                if (liveryKey == CIViewBaseLoadout.selectedUnitLivery) // (avoid re-selecting the livery, which deselects/toggles-off the selection)
                    liveryKey = null;
                // else: go ahead and select the new liveryKey
            }

            CIViewBaseLoadout.selectedUnitLivery = liveryKey;
            object[] args = { liveryKey };
            AccessTools.Method(typeof(CIViewBaseLoadout), "OnLiveryAttachAttempt").Invoke(CIViewBaseLoadout.ins, args); // (the built-in on-click handler when player clicks on a livery in the list)

            CIViewBaseLoadout.ins.Redraw(CIViewBaseLoadout.selectedUnitSocket, CIViewBaseLoadout.selectedUnitHardpoint, false);
        }

        //==============================================================================
        static DataContainerEquipmentLivery GetSelectedLivery()
        {
            string key = CIViewBaseLoadout.selectedUnitLivery;
            if (string.IsNullOrEmpty(key))
                return null;

            return DataMultiLinker<DataContainerEquipmentLivery>.GetEntry(key, false);
        }

        //==============================================================================
        static void ResetLiveryGUIWidgetsToMatchLivery(DataContainerEquipmentLivery livery)
        {
            //Debug.Log($"[LiveryGUI] DEBUG-SPAM: ResetLiveryGUIWidgetsToMatchLivery CIViewBaseLoadout.selectedUnitLivery={CIViewBaseLoadout.selectedUnitLivery}");
            if (string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitLivery))
            {
                liveryNameInput.Set("null");
            }
            else
            {
                liveryNameInput.Set(CIViewBaseLoadout.selectedUnitLivery);
            }
            liveryNameInput.UpdateLabel();

            if (livery != null)
            {
                sliderHelpers["PrimaryR"].sliderBar.valueRaw = livery.colorPrimary.r;
                sliderHelpers["PrimaryG"].sliderBar.valueRaw = livery.colorPrimary.g;
                sliderHelpers["PrimaryB"].sliderBar.valueRaw = livery.colorPrimary.b;
                sliderHelpers["PrimaryA"].sliderBar.valueRaw = livery.colorPrimary.a;
                sliderHelpers["SecondaryR"].sliderBar.valueRaw = livery.colorSecondary.r;
                sliderHelpers["SecondaryG"].sliderBar.valueRaw = livery.colorSecondary.g;
                sliderHelpers["SecondaryB"].sliderBar.valueRaw = livery.colorSecondary.b;
                sliderHelpers["SecondaryA"].sliderBar.valueRaw = livery.colorSecondary.a;
                sliderHelpers["TertiaryR"].sliderBar.valueRaw = livery.colorTertiary.r;
                sliderHelpers["TertiaryG"].sliderBar.valueRaw = livery.colorTertiary.g;
                sliderHelpers["TertiaryB"].sliderBar.valueRaw = livery.colorTertiary.b;
                sliderHelpers["TertiaryA"].sliderBar.valueRaw = livery.colorTertiary.a;
                sliderHelpers["ContentX"].sliderBar.valueRaw = livery.contentParameters.x;
                sliderHelpers["ContentY"].sliderBar.valueRaw = livery.contentParameters.y;
                sliderHelpers["ContentZ"].sliderBar.valueRaw = livery.contentParameters.z;
                //sliderHelpers["ContentW"].sliderBar.valueRaw = livery.contentParameters.w;
                sliderHelpers["PrimaryX"].sliderBar.valueRaw = livery.materialPrimary.x;
                sliderHelpers["PrimaryY"].sliderBar.valueRaw = livery.materialPrimary.y;
                sliderHelpers["PrimaryZ"].sliderBar.valueRaw = livery.materialPrimary.z;
                sliderHelpers["PrimaryW"].sliderBar.valueRaw = livery.materialPrimary.w;
                sliderHelpers["SecondaryX"].sliderBar.valueRaw = livery.materialSecondary.x;
                sliderHelpers["SecondaryY"].sliderBar.valueRaw = livery.materialSecondary.y;
                sliderHelpers["SecondaryZ"].sliderBar.valueRaw = livery.materialSecondary.z;
                sliderHelpers["SecondaryW"].sliderBar.valueRaw = livery.materialSecondary.w;
                sliderHelpers["TertiaryX"].sliderBar.valueRaw = livery.materialTertiary.x;
                sliderHelpers["TertiaryY"].sliderBar.valueRaw = livery.materialTertiary.y;
                sliderHelpers["TertiaryZ"].sliderBar.valueRaw = livery.materialTertiary.z;
                sliderHelpers["TertiaryW"].sliderBar.valueRaw = livery.materialTertiary.w;
                sliderHelpers["EffectX"].sliderBar.valueRaw = livery.effect.x;
                sliderHelpers["EffectY"].sliderBar.valueRaw = livery.effect.y;
                sliderHelpers["EffectZ"].sliderBar.valueRaw = livery.effect.z;
                sliderHelpers["EffectW"].sliderBar.valueRaw = livery.effect.w;

                ContentW.SetLevel(livery.contentParameters.w);
                sliderHelpers["ContentW"].levelLabel.text = ContentW.GetLevelName();
                sliderHelpers["ContentW"].levelLabel.MarkAsChanged();
            }

            UpdateButtonColors();
        }

        //==============================================================================
        static void UpdateButtonColors()
        {
            bool liveryDataIsModified = LiverySnapshotDB.IsCurrentLiveryModified();
            bool liveryNameIsModified = (CIViewBaseLoadout.selectedUnitLivery != liveryNameInput.value);
            bool liveryIsModifed = (liveryDataIsModified || liveryNameIsModified);

            var saveIcon = saveLiveryButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            var saveFillIdle = saveLiveryButton.gameObject.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
            if (saveIcon != null && saveFillIdle != null)
            {
                if (liveryIsModifed &&
                    (!DataMultiLinkerEquipmentLivery.data.ContainsKey(liveryNameInput.value) ||
                    (LiverySnapshotDB.originalLiveries.ContainsKey(liveryNameInput.value) && LiverySnapshotDB.originalLiveries[liveryNameInput.value].ownedByLiveryGUI)))
                {
                    saveIcon.color = activeButtonFGColor;
                    saveFillIdle.color = activeButtonBGColor;
                }
                else
                {
                    saveIcon.color = grayedOutButtonFGColor;
                    saveFillIdle.color = grayedOutButtonBGColor;
                }
            }

            var cloneIcon = cloneLiveryButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            var cloneFillIdle = cloneLiveryButton.gameObject.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
            if (cloneIcon != null && cloneFillIdle != null)
            {
                if (liveryNameIsModified && !string.IsNullOrEmpty(liveryNameInput.value) && !DataMultiLinkerEquipmentLivery.data.ContainsKey(liveryNameInput.value))
                {
                    cloneIcon.color = activeButtonFGColor;
                    cloneFillIdle.color = activeButtonBGColor;
                }
                else
                {
                    cloneIcon.color = grayedOutButtonFGColor;
                    cloneFillIdle.color = grayedOutButtonBGColor;
                }
            }

            var resetIcon = resetLiveryButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            var resetFillIdle = resetLiveryButton.gameObject.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
            if (resetIcon != null && resetFillIdle != null)
            {
                if (liveryIsModifed)
                {
                    resetIcon.color = activeRedButtonFGColor;
                    resetFillIdle.color = activeRedButtonBGColor;
                }
                else
                {
                    resetIcon.color = grayedOutButtonFGColor;
                    resetFillIdle.color = grayedOutButtonBGColor;
                }
            }
        }

        //==============================================================================
        static void UpdateLiveryFromSliders()
        {
            var livery = GetSelectedLivery();
            if (livery == null)
                return;
            livery.colorPrimary.r = sliderHelpers["PrimaryR"].sliderBar.valueRaw;
            livery.colorPrimary.g = sliderHelpers["PrimaryG"].sliderBar.valueRaw;
            livery.colorPrimary.b = sliderHelpers["PrimaryB"].sliderBar.valueRaw;
            livery.colorPrimary.a = sliderHelpers["PrimaryA"].sliderBar.valueRaw;
            livery.colorSecondary.r = sliderHelpers["SecondaryR"].sliderBar.valueRaw;
            livery.colorSecondary.g = sliderHelpers["SecondaryG"].sliderBar.valueRaw;
            livery.colorSecondary.b = sliderHelpers["SecondaryB"].sliderBar.valueRaw;
            livery.colorSecondary.a = sliderHelpers["SecondaryA"].sliderBar.valueRaw;
            livery.colorTertiary.r = sliderHelpers["TertiaryR"].sliderBar.valueRaw;
            livery.colorTertiary.g = sliderHelpers["TertiaryG"].sliderBar.valueRaw;
            livery.colorTertiary.b = sliderHelpers["TertiaryB"].sliderBar.valueRaw;
            livery.colorTertiary.a = sliderHelpers["TertiaryA"].sliderBar.valueRaw;
            livery.contentParameters.x = sliderHelpers["ContentX"].sliderBar.valueRaw;
            livery.contentParameters.y = sliderHelpers["ContentY"].sliderBar.valueRaw;
            livery.contentParameters.z = sliderHelpers["ContentZ"].sliderBar.valueRaw;
            //livery.contentParameters.w = sliderHelpers["ContentW"].sliderBar.valueRaw;
            livery.materialPrimary.x = sliderHelpers["PrimaryX"].sliderBar.valueRaw;
            livery.materialPrimary.y = sliderHelpers["PrimaryY"].sliderBar.valueRaw;
            livery.materialPrimary.z = sliderHelpers["PrimaryZ"].sliderBar.valueRaw;
            livery.materialPrimary.w = sliderHelpers["PrimaryW"].sliderBar.valueRaw;
            livery.materialSecondary.x = sliderHelpers["SecondaryX"].sliderBar.valueRaw;
            livery.materialSecondary.y = sliderHelpers["SecondaryY"].sliderBar.valueRaw;
            livery.materialSecondary.z = sliderHelpers["SecondaryZ"].sliderBar.valueRaw;
            livery.materialSecondary.w = sliderHelpers["SecondaryW"].sliderBar.valueRaw;
            livery.materialTertiary.x = sliderHelpers["TertiaryX"].sliderBar.valueRaw;
            livery.materialTertiary.y = sliderHelpers["TertiaryY"].sliderBar.valueRaw;
            livery.materialTertiary.z = sliderHelpers["TertiaryZ"].sliderBar.valueRaw;
            livery.materialTertiary.w = sliderHelpers["TertiaryW"].sliderBar.valueRaw;
            livery.effect.x = sliderHelpers["EffectX"].sliderBar.valueRaw;
            livery.effect.y = sliderHelpers["EffectY"].sliderBar.valueRaw;
            livery.effect.z = sliderHelpers["EffectZ"].sliderBar.valueRaw;
            livery.effect.w = sliderHelpers["EffectW"].sliderBar.valueRaw;

            livery.contentParameters.w = ContentW.GetLevelValue();

            //todo: there's some circumstances that can cause severe graphical glitches in the form of multiple overlapping flickering large white circles with fuzzy edges, often with a black square in the middle. 
            //todo: seems to affect light gray / white moreso than other colors, but significantly tied to some interplay between xary.W and xary.effect. (sum of color channels affects susceptibility?).
            //todo: cap 1ary.W to -3..+3. but the loss of +4..+5 is visible loss and non-ideal. but +5 is very unfriendly to effect.
            //todo: cap 1ary.effect to -4..+4
            //todo: constrain 1ary.W with respect to 1ary.effect: 1ary.W=+1.5 is about the max safe value while 1ary.effect is -4.
            //todo: constrain 1ary.W when adjusting 1ary.effect: as 1ary.effect goes from 0..-5, cap 1ary.W to be at most 5..1 (reduce positive-magnitude of this, while negative-magnitude of the other increases)
            //todo: constrain 1ary.W when adjusting 1ary.effect: as 1ary.effect goes from 0..+5, cap 1ary.W to be more than -5..-1 (reduce neg-magnitude of this, while pos-magnitude of the other increases)
            //todo: also constrain 1ary.effect while 1ary.W is being adjusted.
            //todo: also apply to 2ary and 3ary.

            // suppose i limit the R+G+B for a part to sum to 2.1 or so. how gray is that, can it be improved via negative-alpha/negative-metal. simple linear sum rather than perceptual? and would need to figure out how to hook that up so as to not cause feedback issues while actively dragging the color sliders. Can go down to about RGB=.5,.5,.5 and use negative alpha to brighten it to apparently full-white. Perhaps a larger-negative alpha cap could further work around this?
        }

        //==============================================================================
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

            // trigger refresh of the whole mech, all parts. (only seems to redo ALL parts if the top-level of the mech is selected, so we do that temporarily)
            var origSocket = CIViewBaseLoadout.selectedUnitSocket;
            var origHardpoint = CIViewBaseLoadout.selectedUnitHardpoint;
            CIViewBaseLoadout.selectedUnitSocket = null;
            CIViewBaseLoadout.selectedUnitHardpoint = null;
            AccessTools.Method(typeof(CIViewBaseLoadout), "OnLiveryHoverEnd").Invoke(CIViewBaseLoadout.ins, null);
            CIViewBaseLoadout.selectedUnitSocket = origSocket;
            CIViewBaseLoadout.selectedUnitHardpoint = origHardpoint;
        }
    }
}
