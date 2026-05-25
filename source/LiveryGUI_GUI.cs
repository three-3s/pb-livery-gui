using HarmonyLib;
using PhantomBrigade;
using PhantomBrigade.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LiveryGUIMod {
    public enum SliderKind {
        Continuous,
        Discrete,
        // Toggle -- supported by the 'helper', but not needed/implemented in LiveryGUI
    }

    // each of these is used to initialize one slider-bar
    public class SliderConfig {
        public string name;
        public string label;
        public Color fillColor;
        public float min;
        public float max;
        public SliderKind sliderKind;
        public SliderConfig(string name, string label, Color fillColor, float min = 0f, float max = 1f, SliderKind sliderKind = SliderKind.Continuous) {
            this.name = name;
            this.label = label;
            this.fillColor = fillColor;
            this.min = min;
            this.max = max;
            this.sliderKind = sliderKind;
        }
    }

    public class LegendConfig {
        public string spriteName;
        public Vector3 localPosition;
        public string tooltipText;
        public LegendConfig(string spriteName, Vector3 localPosition, string tooltipText) {
            this.spriteName = spriteName;
            this.localPosition = localPosition;
            this.tooltipText = tooltipText;
        }
    }

    // relating the SliderKind.Discrete selection to the livery's float contentParameters.w
    public class ContentW {
        public static string[] names = { "none (w=0.0)", "dots (w=1.0)", "lines (w=2.0)", "sheen (w=3.0)" };
        public static int idx = 0;
        public static void SetLevel(float val) {
            idx = (int) Math.Min(Math.Max(0f, val + 0.1f), (float)names.Length - 1f);
        }
        public static float GetLevelValue() {
            return (float)idx;
        }
        public static string GetLevelName() {
            return names[idx];
        }
    }

    public class PilotModePortraitVisibilityTicker : MonoBehaviour {
        void Update() {
            LiveryGUIMod.GUI.UpdatePilotModePortraitVisibility();
        }
    }

    public class GUI {
        public static GameObject paneGO = null;
        public static Dictionary<string, CIHelperSetting> sliderHelpers = null; // key = livery's key
        public static Dictionary<string, SliderConfig> sliderConfigs = null; // key = livery's key
        public static CIButton toggleLiveryGUIButton;
        public static CIButton pilotModeToggleButton;
        public static CIButton pilotModePrevButton;
        public static CIButton pilotModeNextButton;
        public static CIButton pilotModeBaseToggleButton;
        public static UILabel pilotModeNameLabel;
        public static GameObject pilotModePortraitGO;
        public static CIButton resetLiveryButton;
        public static CIButton saveLiveryButton;
        public static CIButton cloneLiveryButton;
        public static CIButton favoriteLiveryButton; // (same as built-in right-click on the livery icons)
        public static UIInput liveryNameInput;
        static readonly Color activeButtonFGColor = new Color(0.6f, 0.7f, 1f, 1f);
        static readonly Color activeButtonBGColor = new Color(0.25f, 0.36f, 0.58f, 0.7f);
        static readonly Color grayedOutButtonFGColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        static readonly Color grayedOutButtonBGColor = new Color(0.0f, 0.0f, 0.0f, 0.7f);
        static readonly Color activeRedButtonFGColor = new Color(1f, 0.38f, 0.38f, 1f);
        static readonly Color activeRedButtonBGColor = new Color(0.61f, 0.22f, 0.22f, 1f);
        static readonly Color pilotLiverySlotMarkerColor = new Color(0.32f, 0.58f, 1f, 0.95f);
        static readonly Color mechLiverySlotMarkerColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
        static SliderRightClickHandler sliderRightClickHandler = null;

        static readonly string spriteNameStarFilled = "s_icon_l32_star_filled";
        static readonly string spriteNameStarOutline = "s_icon_l32_star_outline";
        static readonly string liverySlotLayerMarkerName = "LiveryGUI_LayerMarker";
        static readonly string pilotSymbolSpriteName = "s_icon_l32_pilot1";
        static readonly string pilotModeLivePortraitViewId = "combatTarget";
        static readonly string mechLiverySlotMarkerSpriteName = "icon_mech2";
        static readonly string liverySlotChildMarkerName = "LiveryGUI_ChildMarker";
        static readonly string liverySlotHierarchyLineNamePrefix = "LiveryGUI_HierarchyLine";
        static readonly string liverySlotChildMarkerSpriteName = "s_icon_sm_plus";
        static readonly string liverySlotHierarchyLineSpriteName = "line_vertical_4px";
        static readonly Vector3 liverySlotLayerMarkerOffset = new Vector3(180f, 0f, 0f);
        static readonly string pilotLiverySlotMarkerTooltipHeader = "Pilot livery";
        static readonly string pilotLiverySlotMarkerTooltipContent = "The pilot's own livery-set is applied to this slot. This livery will be applied to this slot of any mech that is piloted by this pilot. Note: Any slot marked with a '+' has child slots, which will be affected by the parent slot.";
        static readonly string mechLiverySlotMarkerTooltipHeader = "Mech base livery";
        static readonly string mechLiverySlotMarkerTooltipContent = "No pilot livery has been assigned to this slot. The mech's own base livery will control this slot. Assigning a livery to this slot will assign the livery to the pilot's livery-set.";
        static readonly Color liverySlotChildMarkerColor = new Color(0.45f, 0.45f, 0.45f, 0.95f);
        static readonly Color liverySlotHierarchyLineColor = new Color(0.7f, 0.7f, 0.7f, 0.75f);
        static readonly FieldInfo liveryHelpersPerSocketField = AccessTools.Field(typeof(CIViewBaseLoadout), "liveryHelpersPerSocket");
        static readonly FieldInfo liveryHelpersPerHardpointField = AccessTools.Field(typeof(CIViewBaseLoadout), "liveryHelpersPerHardpoint");

        public static bool modPrevPilotModeActive = false; // persists whether pilot-edit-mode is active, including when mod-gui is off.
        static bool reapplyLiverySetInProgress = false;
        static bool suppressLiverySlotRecording = false;
        static bool pilotModeMechBaseVisible = true;
        static string pilotModePilotId = null;
        static int pilotModePilotMechId = -99;
        static readonly List<string> pilotModePilotIds = new List<string>();
        static bool pilotModeLivePortraitActive = false;
        static string pilotModeLivePortraitPilotIdLast = null;
        static readonly Dictionary<Animator, AnimatorUpdateMode> pilotModeLivePortraitAnimatorUpdateModes = new Dictionary<Animator, AnimatorUpdateMode>();
        public static bool IsPBLiveryViewActive() { return CIViewBaseLoadout.ins != null && CIViewBaseLoadout.ins.IsEntered() && CIViewBaseLoadout.liveryMode; }
        public static bool IsModPaneActive() { return paneGO != null && paneGO.activeSelf; }
        public static bool IsPilotModeActive() { return IsPBLiveryViewActive() && IsModPaneActive() && modPrevPilotModeActive; }
        public static bool IsPilotModeMechBaseVisible() { return pilotModeMechBaseVisible; }
        public static bool IsLiverySlotRecordingSuppressed() { return suppressLiverySlotRecording; }

        // widget positions
        class Positions {
            public static readonly float pxGapAbovePaneGO = 81f;

            public static readonly float topRowY = +72f;
            public static readonly Vector3 posStep = new Vector3(80f, 0f, 0f);
            public static readonly Vector3 smallPosStep = new Vector3(55f, 0f, 0f);
            public static readonly Vector3 minimalPosStep = new Vector3(50f, 0f, 0f);
            public static readonly Vector3 posStepOverNameInputField = new Vector3(418f, 0f, 0f);

            public static readonly Vector3 liveryGuiToggle = new Vector3(562f, topRowY, 0f); // in a blank spot on the top left of the livery sidebar that's on left portion of screen

            public static readonly Vector3 favoriteButton = new Vector3(645f, topRowY, 0f); // top-left corner of the mech-preview region (just to the right of the liveryGuiToggle)
            public static readonly Vector3 pilotToggle = favoriteButton + posStep;
            public static readonly Vector3 pilotBaseToggleButton = pilotToggle + minimalPosStep;
            public static readonly Vector3 resetButton = pilotBaseToggleButton + posStep;
            public static readonly Vector3 liveryName = resetButton + new Vector3(55f, 0f, 0f); // (when not moved off-screen)
            public static readonly Vector3 liveryName_hiddenOffscreen = new Vector3(20000f, 20000f, 0f);
            public static readonly Vector3 saveButton = resetButton + posStepOverNameInputField;
            public static readonly Vector3 cloneButton = saveButton + smallPosStep;

            public static readonly Vector3 legendGroup1Item1 = cloneButton + 1f * posStep;
            public static readonly Vector3 legendGroup2Item1 = cloneButton + 2f * posStep;
            public static readonly Vector3 legendGroup2Item2 = cloneButton + 2f * posStep + 1f * minimalPosStep;

            public static readonly float pilotPanelLeft = favoriteButton[0] - 23f;
            public static readonly int pilotPortraitDim = 100;
            public static readonly Vector3 pilotPrevButtonOffset = new Vector3(pilotPortraitDim + 4f, 0f, 0f);
            public static readonly Vector3 pilotNextButtonOffset = pilotPrevButtonOffset + smallPosStep;
            public static readonly Vector3 pilotNameOffset = pilotNextButtonOffset + smallPosStep + new Vector3(-3f, -35f, 0f);
        }

        //==============================================================================
        public static void RedrawLiveryGUI(CIViewBaseLoadout __instance) {
            Initialize(__instance);

            UpdateWidgetPositioning(__instance);
            UpdatePilotModeButtonVisuals();
            ResetLiveryGUIWidgetsToMatchLivery(GetSelectedLivery());
            UpdateLiverySlotLayerMarkers(__instance);
        }

        //==============================================================================
        static void Initialize(CIViewBaseLoadout __instance) {
            if (paneGO != null)
                return;

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
            paneGO.AddComponent<PilotModePortraitVisibilityTicker>();

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
                    //todo consider making space for additional info like a small "?" button for each with tool explaining what the field does (ideally with translations).
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
            }//foreach

            ////////////////////////////////////////////////////////////////////////////////
            // toggle-button for Livery GUI visibility
            GameObject toggleLiveryGUIButtonGO = GameObject.Instantiate(CIViewBaseLoadout.ins.headerButtonLivery.gameObject, uiRoot, false);
            toggleLiveryGUIButtonGO.name = "LiveryAdvancedToggle";
            toggleLiveryGUIButtonGO.transform.localPosition = Positions.liveryGuiToggle;
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
            toggleLiveryGUIButton.tooltipUsed = true;
            toggleLiveryGUIButton.AddTooltip("Livery GUI", "Show/hide advanced livery customization options");
            toggleLiveryGUIButton.tooltipDelay = false;
            toggleLiveryGUIButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            toggleLiveryGUIButton.tooltipOffset = new Vector3(44f, -62f, 0f);
            toggleLiveryGUIButton.callbackOnClick = new UICallback(() =>
            {
                bool newState = !paneGO.activeSelf;
                paneGO.SetActive(newState);

                // first time wasn't updating the text-input-field with the livery-name. i don't know why.
                // INVOKE ALL THE REFRESH. MULTIPLY. STILL DOESN'T WORK FIRST TIME. ONLY SECOND TIME. ONLY SECOND TIME.
                string origLiveryKey = CIViewBaseLoadout.selectedUnitLivery;
                suppressLiverySlotRecording = true;
                if (origLiveryKey == null && DataMultiLinkerEquipmentLivery.data.Count > 0)
                    SelectLivery(DataMultiLinkerEquipmentLivery.data.First().Key);
                else
                    SelectLivery(null);
                SelectLivery(origLiveryKey);
                suppressLiverySlotRecording = false;
                liveryNameInput.ForceSelection(true);
                liveryNameInput.ForceSelection(false);
                liveryNameInput.label.MarkAsChanged();
                RefreshSphereAndMechPreviews();
                UpdateWidgetPositioning(__instance);
                ResetLiveryGUIWidgetsToMatchLivery(GetSelectedLivery());
                RefreshSphereAndMechPreviews();
                UpdatePilotModeButtonVisuals();
                ReapplyLiverySet(CIViewBaseLoadout.selectedUnitID);

                // todo: some of this refresh-spam seems to be causing the audio to play multiple times (ie louder). have a good enough workaround for the text-input field now, ie resolved elsewhere. clean this up.
            });

            toggleLiveryGUIButtonGO.SetActive(true);

            ////////////////////////////////////////////////////////////////////////////////
            // toggle-button for Pilot-Mode
            GameObject pilotModeToggleGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
            pilotModeToggleGO.name = "pilotModeToggleGO";
            pilotModeToggleGO.transform.localPosition = Positions.pilotToggle;

            pilotModeToggleButton = pilotModeToggleGO.GetComponent<CIButton>();
            pilotModeToggleButton.AddTooltip("Toggle Pilot Livery-Set Editing Mode", "In that mode, the selected PILOT's livery-set is edited by assigning liveries to the mech's parts. Mech parts that have no pilot-livery are transparent and will show the mech's livery. For example, it is possible to paint a mech gray in MECH edit mode, and then enable PILOT edit mode and assign a red livery to the upper body. Then, whenever that pilot is piloting that mech, the mech will be gray with a red upper body. Any mech this pilot is assigned to will have a red upper body.");
            pilotModeToggleButton.tooltipDelay = false;
            pilotModeToggleButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            pilotModeToggleButton.tooltipOffset = new Vector3(44f, -62f, 0f);
            pilotModeToggleButton.callbackOnClick = new UICallback(() =>
            {
                modPrevPilotModeActive = !modPrevPilotModeActive;
                EnsurePilotModePilot(CIViewBaseLoadout.selectedUnitID);
                Debug.Log($"[LiveryGUI] PilotModeActive={modPrevPilotModeActive}");
                UpdatePilotModeButtonVisuals();
                ReapplyLiverySet(CIViewBaseLoadout.selectedUnitID);
            });

            pilotModeToggleGO.SetActive(true);
            UpdatePilotModeButtonVisuals();

            ////////////////////////////////////////////////////////////////////////////////
            // Pilot mode: toggle-button for showing/hiding mech base livery
            GameObject pilotModeBaseToggleGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
            pilotModeBaseToggleGO.name = "pilotModeBaseToggleGO";
            pilotModeBaseToggleGO.transform.localPosition = Positions.pilotBaseToggleButton;
            pilotModeBaseToggleGO.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            var pilotBaseIcon = pilotModeBaseToggleGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (pilotBaseIcon != null) { pilotBaseIcon.color = new Color(0.9f, 0.9f, 0.9f, 0.8f); pilotBaseIcon.spriteName = "icon_mech2"; } //3todo.later check/cleanup visuals
            pilotModeBaseToggleButton = pilotModeBaseToggleGO.GetComponent<CIButton>();
            pilotModeBaseToggleButton.callbackOnClick = new UICallback(() =>
            {
                pilotModeMechBaseVisible = !pilotModeMechBaseVisible;
                Debug.Log($"[LiveryGUI] PilotModeMechBaseVisible={pilotModeMechBaseVisible}");
                UpdatePilotModeButtonVisuals();
                ReapplyLiverySet(CIViewBaseLoadout.selectedUnitID);
            });
            pilotModeBaseToggleButton.tooltipUsed = true;
            pilotModeBaseToggleButton.AddTooltip("Show/Hide Mech Livery", "Shows or hides the mech's base livery underneath the pilot's livery. Only the view in this editor is affected. Right click on livery slots to unassign pilot's livery from that slot. Note: Assigning a livery to a slot with a '+' also affects all parts contained within that slot.");
            pilotModeBaseToggleButton.tooltipDelay = false;
            pilotModeBaseToggleButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            pilotModeBaseToggleButton.tooltipOffset = new Vector3(44f, -62f, 0f);
            UpdatePilotModeButtonVisuals();

            ////////////////////////////////////////////////////////////////////////////////
            // pilot: previous-button, current-info, next-button
            GameObject pilotModePrevGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
            pilotModePrevGO.name = "pilotModePrevGO";
            var pilotPrevIcon = pilotModePrevGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (pilotPrevIcon != null) { pilotPrevIcon.color = new Color(0.9f, 0.9f, 0.9f, 0.8f); pilotPrevIcon.spriteName = "icon_arrow_back_2x"; pilotPrevIcon.flip = UIBasicSprite.Flip.Nothing; }
            pilotModePrevButton = pilotModePrevGO.GetComponent<CIButton>();
            pilotModePrevButton.callbackOnClick = new UICallback(() =>
            {
                CyclePilotModePilot(-1);
            });
            pilotModePrevButton.tooltipUsed = true;
            pilotModePrevButton.AddTooltip("Previous Pilot", "Select the previous pilot. This controls which pilot's livery-set is being edited in 'pilot livery-set editing mode'. (This does not assign the pilot to the mech. Assigning a pilot to a mech is done in mission briefing.)");
            pilotModePrevButton.tooltipDelay = false;
            pilotModePrevButton.tooltipOffset = new Vector3(45f, 0f, 0f);
            pilotModePrevButton.tooltipPivot = UIWidget.Pivot.BottomLeft;

            GameObject pilotModeNextGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
            pilotModeNextGO.name = "pilotModeNextGO";
            var pilotNextIcon = pilotModeNextGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (pilotNextIcon != null) { pilotNextIcon.color = new Color(0.9f, 0.9f, 0.9f, 0.8f); pilotNextIcon.spriteName = "icon_arrow_back_2x"; pilotNextIcon.flip = UIBasicSprite.Flip.Horizontally; }
            pilotModeNextButton = pilotModeNextGO.GetComponent<CIButton>();
            pilotModeNextButton.callbackOnClick = new UICallback(() =>
            {
                CyclePilotModePilot(+1);
            });
            pilotModeNextButton.tooltipUsed = true;
            pilotModeNextButton.AddTooltip("Next Pilot", "Select the next pilot. This controls which pilot's livery-set is being edited in 'pilot livery-set editing mode'. (This does not assign the pilot to the mech. Assigning a pilot to a mech is done in mission briefing.)");
            pilotModeNextButton.tooltipDelay = false;
            pilotModeNextButton.tooltipOffset = new Vector3(45f, 0f, 0f);
            pilotModeNextButton.tooltipPivot = UIWidget.Pivot.BottomLeft;

            CreatePilotModeReadoutWidgets(helperPrefab.sharedLabelName);

            ////////////////////////////////////////////////////////////////////////////////
            // Livery GUI buttons: 'revert changes', 'clone livery', 'save livery to disk'
            GameObject resetLiveryButtonGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
            resetLiveryButtonGO.name = "resetLiveryButtonGO";
            resetLiveryButtonGO.transform.localPosition = Positions.resetButton;
            var resetIcon = resetLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (resetIcon != null) { resetIcon.color = new Color(0.5f, 0.6f, 0.8f, 0.8f); resetIcon.spriteName = "s_icon_l32_retreat"; }

            GameObject saveLiveryButtonGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
            saveLiveryButtonGO.name = "saveLiveryButtonGO";
            saveLiveryButtonGO.transform.localPosition = Positions.saveButton;
            var saveIcon = saveLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (saveIcon != null) { saveIcon.color = new Color(0.5f, 0.6f, 0.8f, 0.8f); saveIcon.spriteName = "s_icon_l32_lc_save1"; }

            GameObject cloneLiveryButtonGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
            cloneLiveryButtonGO.name = "cloneLiveryButtonGO";
            cloneLiveryButtonGO.transform.localPosition = Positions.cloneButton;
            var cloneIcon = cloneLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (cloneIcon != null) { cloneIcon.color = new Color(0.5f, 0.8f, 0.6f, 0.8f); cloneIcon.spriteName = "s_icon_l32_lc_grid_plus"; }

            resetLiveryButton = resetLiveryButtonGO.GetComponent<CIButton>();
            resetLiveryButton.AddTooltip("Revert Changes", "Reset this livery to its last saved values.");
            resetLiveryButton.tooltipDelay = false;
            resetLiveryButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            resetLiveryButton.tooltipOffset = new Vector3(44f, -62f, 0f);
            resetLiveryButton.callbackOnClick = new UICallback(() =>
            {
                OnResetLiveryClicked();
            });

            saveLiveryButton = saveLiveryButtonGO.GetComponent<CIButton>();
            saveLiveryButton.AddTooltip("Save Livery", "Save this livery, using the given name. The name used must not match any built-in livery, nor any livery provided by a mod. Using a name that does not exist yet will save to a new livery with that name.");
            saveLiveryButton.tooltipDelay = false;
            saveLiveryButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            saveLiveryButton.tooltipOffset = new Vector3(44f, -62f, 0f);
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
            cloneLiveryButton.AddTooltip("Clone Livery", "Creates a copy of the current livery, using the given name. There must not already be a livery with that name. Remember to also save the livery.");
            cloneLiveryButton.tooltipDelay = false;
            cloneLiveryButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            cloneLiveryButton.tooltipOffset = new Vector3(44f, -62f, 0f);
            cloneLiveryButton.callbackOnClick = new UICallback(() =>
            {
                OnCloneLiveryClicked();
            });

            ////////////////////////////////////////////////////////////////////////////////
            // Livery GUI text-input field: for seeing & renaming the livery-key/file-name
            GameObject liveryNameInputGO = GameObject.Instantiate(CIViewBaseLoadout.ins.headerInputUnitName.gameObject, uiRoot/*paneGO.transform*/, false); // bug workaround: using uiRoot to make this "visible" as a workaround for the text-display refusing to initially populate until it gets displayed AND THEN user clicks on something (after which it starts updating/working fine). We manually move this widget off-screen while LiveryGUI is toggled off.
            liveryNameInputGO.name = "liveryNameInputGO";
            liveryNameInputGO.transform.localPosition = Positions.liveryName_hiddenOffscreen;
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
            // More Livery GUI buttons: 'toggle as favorited'
            GameObject favoriteLiveryButtonGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
            favoriteLiveryButtonGO.name = "favoriteLiveryButton";
            favoriteLiveryButtonGO.transform.localPosition = Positions.favoriteButton;
            var favoriteIcon = favoriteLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (favoriteIcon != null) { favoriteIcon.color = new Color(1f, 1f, 0.2f, 0.8f); favoriteIcon.spriteName = spriteNameStarOutline; } // (we'll toggle between Outline & Filled)

            favoriteLiveryButton = favoriteLiveryButtonGO.GetComponent<CIButton>();
            favoriteLiveryButton.AddTooltip("Toggle as Favorite", "Marks this livery as a favorite. (Favorites are kept at the front of the list of liveries.)");
            favoriteLiveryButton.tooltipDelay = false;
            favoriteLiveryButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            favoriteLiveryButton.tooltipOffset = new Vector3(44f, -62f, 0f);
            favoriteLiveryButton.callbackOnClick = new UICallback(() =>
            {
                object[] args = { CIViewBaseLoadout.selectedUnitLivery };
                AccessTools.Method(typeof(CIViewBaseLoadout), "OnLiveryFavoriteToggle").Invoke(CIViewBaseLoadout.ins, args); // (the built-in on-click handler when player right-clicks on a livery in the list)
                UpdateButtonColors();
                //3todo.later: BUG: there was something wrong here, but SSD failed and now I forgot what the exact issue was. button was left gray (or non-gray?) when it should have been colored (or not-colored). there's a bug where after clone, the Reset button remains red="i'm clickable", but doesn't actually do anything. (maybe that was it?)
            });

            ////////////////////////////////////////////////////////////////////////////////
            // row of no-op buttons, as a crude legend/hint about usable controls.
            const string tooltip1 = "Click on sliders or click-and-drag to set.\n\nRemember to save.\nNormal values for sliders are between 0 and 1. Other values might work, but they might cause problems.";
            const string tooltip2 = "For precise adjustments:\nRight-click-and-hold, and move mouse <--->\nChange speed: Hold SHIFT, ALT, or CTRL.";
            LegendConfig[] legendConfigs = {
                // note: the button icons seem to get left/right mirror'd
                new LegendConfig("mouse_right_outline", Positions.legendGroup1Item1, tooltip1),
                new LegendConfig("mouse_left_outline",  Positions.legendGroup2Item1, tooltip2),
                new LegendConfig("mouse_horizontal",    Positions.legendGroup2Item2, tooltip2),
            };
            Color legendColor = new Color(1f, 0f, 0f, 0.2f);

            foreach (LegendConfig legendConfig in legendConfigs)
            {
                GameObject buttonGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
                buttonGO.name = "legend" + legendConfig.spriteName;
                buttonGO.transform.localPosition = legendConfig.localPosition;
                var buttonIcon = buttonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                var buttonFrame = buttonGO.transform.Find("Sprite_Frame")?.GetComponent<UISprite>();
                var buttonFillIdle = buttonGO.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
                var buttonFillHover = buttonGO.transform.Find("Sprite_Fill_Hover")?.GetComponent<UISprite>();
                if (buttonIcon != null) { buttonIcon.color = new Color(1f, 1f, 1f, 0.8f); buttonIcon.spriteName = legendConfig.spriteName; }
                if (buttonFrame != null) buttonFrame.color = legendColor;
                if (buttonFillIdle != null) buttonFillIdle.color = legendColor;
                if (buttonFillHover != null) buttonFillHover.color = legendColor;
                var button = new CIButton();
                button = buttonGO.GetComponent<CIButton>();
                button.callbackOnClick = null;

                // 3todo.later update this with better description. ideally would support translations (and for any other tooltips/text).
                button.tooltipUsed = true;
                button.tooltipKey = null;
                button.AddTooltip("Hints", legendConfig.tooltipText);
                button.tooltipDelay = false;
                button.tooltipPivot = UIWidget.Pivot.TopRight;
                button.tooltipOffset = new Vector3(-77f + 100f -29f, -132f +84f -19f);
                button.tooltipColor = legendColor;
                button.tooltipColorCustom = true;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Listener for right-click drag & release, for sliders.
            // (Just want one instance. Attached to Save button, somewhat arbitrarily.)
            sliderRightClickHandler = saveLiveryButtonGO.AddComponent<SliderRightClickHandler>();

            ////////////////////////////////////////////////////////////////////////////////
            // Livery GUI initial visibility
            paneGO.SetActive(false);
        }//Initialize()

        //==============================================================================
        static void CreatePilotModeReadoutWidgets(UILabel labelTemplate) {
            if (labelTemplate != null)
            {
                GameObject labelGO = GameObject.Instantiate(labelTemplate.gameObject, paneGO.transform, false);
                labelGO.name = "pilotModeNameLabelGO";
                labelGO.transform.localScale = Vector3.one;
                pilotModeNameLabel = labelGO.GetComponent<UILabel>();
                pilotModeNameLabel.text = "[none]";
                pilotModeNameLabel.fontSize += 10;
                pilotModeNameLabel.width = 500;
                pilotModeNameLabel.height = 60;
                pilotModeNameLabel.depth = 1200;
                pilotModeNameLabel.pivot = UIWidget.Pivot.Left;
                pilotModeNameLabel.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
                pilotModeNameLabel.overflowMethod = UILabel.Overflow.ShrinkContent;
                pilotModeNameLabel.gameObject.SetActive(false);
            }

            pilotModePortraitGO = new GameObject("pilotModePortraitGO");
            pilotModePortraitGO.transform.SetParent(paneGO.transform, false);
            pilotModePortraitGO.transform.localScale = Vector3.one;
            pilotModePortraitGO.SetActive(false);
        }

        //==============================================================================
        static public void ReapplyLiverySet(int mechId) {
            if (reapplyLiverySetInProgress)
                return;

            if (mechId < 0)
                mechId = CIViewBaseLoadout.selectedUnitID;
            if (mechId < 0)
                return;

            EnsurePilotModePilot(mechId);

            reapplyLiverySetInProgress = true;
            LiverySetsDB.ReassertMechLiverySet(mechId);

            if (CIViewBaseLoadout.ins != null && CIViewBaseLoadout.liveryMode)
                CIViewBaseLoadout.ins.Redraw(CIViewBaseLoadout.selectedUnitSocket, CIViewBaseLoadout.selectedUnitHardpoint, false);

            reapplyLiverySetInProgress = false;
            UpdatePilotModeButtonVisuals();
        }

        //==============================================================================
        static void UpdateLiverySlotLayerMarkers(CIViewBaseLoadout loadoutView) {
            if (loadoutView == null)
                return;

            bool showMarkers = IsPilotModeActive() && CIViewBaseLoadout.liveryMode;
            bool showHierarchyDecorations = CIViewBaseLoadout.liveryMode;
            int mechId = CIViewBaseLoadout.selectedUnitID;
            if (mechId < 0) {
                PersistentEntity selectedUnit = IDUtility.GetPersistentEntity(CIViewBaseLoadout.selectedUnitName);
                mechId = selectedUnit != null && selectedUnit.hasId ? selectedUnit.id.id : -1;
            }
            PersistentEntity unit = mechId >= 0 ? IDUtility.GetPersistentEntity(mechId) : null;

            string pilotId = showMarkers ? GetPilotModePilotId(mechId) : null;

            var socketHelpers = GetLiverySlotHelpers(loadoutView, liveryHelpersPerSocketField);
            var hardpointHelpers = GetLiverySlotHelpers(loadoutView, liveryHelpersPerHardpointField);
            bool rootHasChildren = HasVisibleLiverySlot(socketHelpers);

            UpdateLiverySlotDecorations(loadoutView.liveryHelperRoot, LiverySetsDB.PartKey(null, null), showMarkers && mechId >= 0, mechId, pilotId, showHierarchyDecorations && IsLiverySlotVisible(loadoutView.liveryHelperRoot), 0, rootHasChildren, loadoutView.liverySlotHeight);

            if (socketHelpers != null) {
                foreach (var kv in socketHelpers) {
                    CIHelperLoadoutLiverySlot slot = kv.Value;
                    bool slotVisible = IsLiverySlotVisible(slot);
                    bool hasSubparts = DoesSocketHaveVisibleLiverySubparts(unit, kv.Key);
                    UpdateLiverySlotDecorations(slot, LiverySetsDB.PartKey(kv.Key, null), showMarkers && slotVisible && mechId >= 0, mechId, pilotId, showHierarchyDecorations && slotVisible, 1, hasSubparts, loadoutView.liverySlotHeight);
                }
            }

            if (hardpointHelpers != null) {
                string socket = CIViewBaseLoadout.selectedUnitSocket;
                foreach (var kv in hardpointHelpers) {
                    CIHelperLoadoutLiverySlot slot = kv.Value;
                    bool slotVisible = !string.IsNullOrEmpty(socket) && IsLiverySlotVisible(slot);
                    UpdateLiverySlotDecorations(slot, LiverySetsDB.PartKey(socket, kv.Key), showMarkers && slotVisible && mechId >= 0, mechId, pilotId, showHierarchyDecorations && slotVisible, 2, false, loadoutView.liverySlotHeight);
                }
            }
        }

        //==============================================================================
        static Dictionary<string, CIHelperLoadoutLiverySlot> GetLiverySlotHelpers(CIViewBaseLoadout loadoutView, FieldInfo field) {
            if (loadoutView == null || field == null)
                return null;

            return field.GetValue(loadoutView) as Dictionary<string, CIHelperLoadoutLiverySlot>;
        }

        //==============================================================================
        static bool IsLiverySlotVisible(CIHelperLoadoutLiverySlot slot) {
            return slot != null && slot.gameObject.activeSelf && slot.targetAlpha > 0.01f;
        }

        //==============================================================================
        static bool HasVisibleLiverySlot(Dictionary<string, CIHelperLoadoutLiverySlot> slots) {
            if (slots == null)
                return false;

            foreach (var kv in slots)
                if (IsLiverySlotVisible(kv.Value))
                    return true;

            return false;
        }

        //==============================================================================
        static bool DoesSocketHaveVisibleLiverySubparts(PersistentEntity unit, string socket) {
            if (unit == null || string.IsNullOrEmpty(socket))
                return false;

            EquipmentEntity part = EquipmentUtility.GetPartInUnit(unit, socket, false, null);
            if (part == null || part.partBlueprint == null || part.partBlueprint.hardpoints == null)
                return false;

            HashSet<EquipmentEntity> subsystemsInPart = EquipmentUtility.GetSubsystemsInPart(part);
            foreach (string hardpoint in part.partBlueprint.hardpoints) {
                EquipmentEntity subsystem = EquipmentUtility.GetSubsystemInPart(part, hardpoint, false, subsystemsInPart);
                if (subsystem == null)
                    continue;

                DataContainerSubsystemHardpoint hardpointData = DataMultiLinker<DataContainerSubsystemHardpoint>.GetEntry(hardpoint, true);
                if (hardpointData != null && !hardpointData.isInternal && !string.IsNullOrEmpty(hardpointData.visualGroup))
                    return true;
            }

            return false;
        }

        //==============================================================================
        static void UpdateLiverySlotDecorations(CIHelperLoadoutLiverySlot slot, string partKey, bool layerMarkerVisible, int mechId, string pilotId, bool hierarchyVisible, int hierarchyDepth, bool hasSubparts, int slotHeight) {
            UpdateLiverySlotLayerMarker(slot, partKey, layerMarkerVisible, mechId, pilotId);
            UpdateLiverySlotChildMarker(slot, hierarchyVisible && hasSubparts);
            UpdateLiverySlotHierarchyLine(slot, hierarchyVisible, hierarchyDepth, slotHeight);
        }

        //==============================================================================
        static void UpdateLiverySlotLayerMarker(CIHelperLoadoutLiverySlot slot, string partKey, bool visible, int mechId, string pilotId) {
            UISprite marker = GetOrCreateLiverySlotLayerMarker(slot, visible);
            if (marker == null)
                return;

            marker.gameObject.SetActive(visible);
            if (!visible)
                return;

            bool pilotControlled = LiverySetsDB.IsPilotLiveryResponsibleForPart(mechId, partKey, pilotId);
            marker.spriteName = pilotControlled ? pilotSymbolSpriteName : mechLiverySlotMarkerSpriteName;
            marker.color = pilotControlled ? pilotLiverySlotMarkerColor : mechLiverySlotMarkerColor;
            marker.alpha = Mathf.Clamp01(slot.targetAlpha);
            ConfigureLiverySlotLayerMarkerTooltip(slot, marker, pilotControlled);
            marker.MarkAsChanged();
        }

        //==============================================================================
        static UISprite GetOrCreateLiverySlotLayerMarker(CIHelperLoadoutLiverySlot slot, bool createIfMissing) {
            if (slot == null)
                return null;

            Transform existing = slot.transform.Find(liverySlotLayerMarkerName);
            if (existing != null)
                return existing.GetComponent<UISprite>();

            if (!createIfMissing || slot.spriteIcon == null)
                return null;

            GameObject markerGO = GameObject.Instantiate(slot.spriteIcon.gameObject, slot.transform, false);
            markerGO.name = liverySlotLayerMarkerName;
            markerGO.transform.localScale = Vector3.one;
            markerGO.transform.localPosition = slot.spriteIcon.transform.localPosition + liverySlotLayerMarkerOffset;

            UISprite marker = markerGO.GetComponent<UISprite>();
            if (marker != null) {
                marker.width = 30;
                marker.height = 30;
                marker.depth = slot.spriteIcon.depth + 12;
            }
            return marker;
        }

        //==============================================================================
        static void ConfigureLiverySlotLayerMarkerTooltip(CIHelperLoadoutLiverySlot slot, UISprite marker, bool pilotControlled) {
            if (slot == null || marker == null)
                return;

            Collider collider = marker.gameObject.GetComponent<Collider>() ?? marker.gameObject.AddComponent<BoxCollider>();

            Bounds bounds = marker.CalculateBounds();
            BoxCollider boxCollider = collider as BoxCollider;
            if (boxCollider != null) {
                boxCollider.center = bounds.center;
                boxCollider.size = bounds.size;
            }

            CIButton button = marker.gameObject.GetComponent<CIButton>() ?? marker.gameObject.AddComponent<CIButton>();

            if (button.elements          == null) button.elements          = new List<CIElement>();
            if (button.panels            == null) button.panels            = new List<CIPanel>();
            if (button.longPressElements == null) button.longPressElements = new List<CIButtonFill>();

            button.audio = new PhantomBrigade.Game.DataBlockAudioEventsInButton { enabled = false };
            button.tooltipFromLibrary = false;
            button.tooltipDelay = false;
            button.tooltipPivot = UIWidget.Pivot.BottomLeft;
            button.tooltipOffset = new Vector3(30f, 21f, 0f);
            button.tooltipWidth = 280;
            button.AddTooltip(
                pilotControlled ? pilotLiverySlotMarkerTooltipHeader : mechLiverySlotMarkerTooltipHeader,
                pilotControlled ? pilotLiverySlotMarkerTooltipContent : mechLiverySlotMarkerTooltipContent);

            if (slot.button != null) {
                button.callbackOnClick = slot.button.callbackOnClick;
                button.callbackOnClickSecondary = slot.button.callbackOnClickSecondary;
                button.callbackOnClickLong = slot.button.callbackOnClickLong;
            }
        }

        //==============================================================================
        static void UpdateLiverySlotChildMarker(CIHelperLoadoutLiverySlot slot, bool visible) {
            UISprite marker = GetOrCreateLiverySlotSprite(slot, liverySlotChildMarkerName, visible, slot?.spriteIcon);
            if (marker == null)
                return;

            marker.gameObject.SetActive(visible);
            if (!visible)
                return;

            if (!TryGetLiverySlotBackgroundBounds(slot, out Vector3 bottomLeft, out Vector3 topRight)) {
                marker.gameObject.SetActive(false);
                return;
            }

            marker.spriteName = liverySlotChildMarkerSpriteName;
            marker.width = 18;
            marker.height = 18;
            marker.depth = slot.spriteBackground.depth + 8;
            marker.color = liverySlotChildMarkerColor;
            marker.alpha = Mathf.Clamp01(slot.targetAlpha);
            marker.transform.localScale = Vector3.one;
            marker.transform.localPosition = new Vector3(bottomLeft.x + 6f, (bottomLeft.y + topRight.y) * 0.5f, 0f);
            marker.MarkAsChanged();
        }

        //==============================================================================
        static void UpdateLiverySlotHierarchyLine(CIHelperLoadoutLiverySlot slot, bool visible, int hierarchyDepth, int slotHeight) {
            HideLiverySlotSprite(slot, liverySlotHierarchyLineNamePrefix + "1");

            bool lineVisible = visible && hierarchyDepth >= 2;
            UISprite line = GetOrCreateLiverySlotSprite(slot, liverySlotHierarchyLineNamePrefix + "0", lineVisible, slot?.spriteIcon);
            if (line == null)
                return;

            line.gameObject.SetActive(lineVisible);
            if (!lineVisible)
                return;

            if (!TryGetLiverySlotBackgroundBounds(slot, out Vector3 bottomLeft, out Vector3 topRight)) {
                line.gameObject.SetActive(false);
                return;
            }

            int height = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(slotHeight, topRight.y - bottomLeft.y)));
            line.spriteName = liverySlotHierarchyLineSpriteName;
            line.width = 4;
            line.height = height;
            line.depth = slot.spriteBackground.depth + 8;
            line.color = liverySlotHierarchyLineColor;
            line.alpha = Mathf.Clamp01(slot.targetAlpha);
            line.transform.localScale = Vector3.one;
            line.transform.localPosition = new Vector3(bottomLeft.x + 6f, (bottomLeft.y + topRight.y) * 0.5f, 0f);
            line.MarkAsChanged();
        }

        //==============================================================================
        static void HideLiverySlotSprite(CIHelperLoadoutLiverySlot slot, string name) {
            if (slot == null || string.IsNullOrEmpty(name))
                return;

            Transform existing = slot.transform.Find(name);
            existing?.gameObject.SetActive(false);
        }

        //==============================================================================
        static UISprite GetOrCreateLiverySlotSprite(CIHelperLoadoutLiverySlot slot, string name, bool createIfMissing, UISprite template) {
            if (slot == null || string.IsNullOrEmpty(name))
                return null;

            Transform existing = slot.transform.Find(name);
            if (existing != null)
                return existing.GetComponent<UISprite>();

            if (!createIfMissing || template == null)
                return null;

            GameObject spriteGO = GameObject.Instantiate(template.gameObject, slot.transform, false);
            spriteGO.name = name;
            spriteGO.transform.localScale = Vector3.one;
            return spriteGO.GetComponent<UISprite>();
        }

        //==============================================================================
        static bool TryGetLiverySlotBackgroundBounds(CIHelperLoadoutLiverySlot slot, out Vector3 bottomLeft, out Vector3 topRight) {
            bottomLeft = Vector3.zero;
            topRight = Vector3.zero;
            if (slot == null || slot.spriteBackground == null)
                return false;

            Vector3[] corners = slot.spriteBackground.localCorners;
            bottomLeft = slot.transform.InverseTransformPoint(slot.spriteBackground.transform.TransformPoint(corners[0]));
            topRight = slot.transform.InverseTransformPoint(slot.spriteBackground.transform.TransformPoint(corners[2]));
            return true;
        }

        //==============================================================================
        public static string GetPilotModePilotId(int mechId) {
            EnsurePilotModePilot(mechId);
            return pilotModePilotId;
        }

        //==============================================================================
        static void EnsurePilotModePilot(int mechId) {
            RefreshPilotModePilotIds();
            if (pilotModePilotIds.Count == 0)
            {
                pilotModePilotId = null;
                pilotModePilotMechId = mechId;
                return;
            }

            bool mechChanged = mechId >= 0 && mechId != pilotModePilotMechId;
            if (!mechChanged && !string.IsNullOrEmpty(pilotModePilotId) && pilotModePilotIds.Contains(pilotModePilotId))
                return;

            string assignedPilotId = LiverySetsDB.ResolvePilotIdForMech(mechId);
            if (!string.IsNullOrEmpty(assignedPilotId) && pilotModePilotIds.Contains(assignedPilotId))
            {
                pilotModePilotId = assignedPilotId;
                pilotModePilotMechId = mechId;
                return;
            }

            pilotModePilotId = pilotModePilotIds[0];
            pilotModePilotMechId = mechId;
        }

        //==============================================================================
        static void RefreshPilotModePilotIds() {
            pilotModePilotIds.Clear();

            List<PersistentEntity> pilots = PilotUtility.GetPilotsAtBase();
            if (pilots == null)
                return;

            foreach (PersistentEntity pilot in pilots)
            {
                if (pilot == null || !pilot.isPilotTag || pilot.isDestroyed || !pilot.hasNameInternal)
                    continue;

                pilotModePilotIds.Add(pilot.nameInternal.s);
            }

            pilotModePilotIds.Sort(StringComparer.Ordinal);
        }

        //==============================================================================
        static void CyclePilotModePilot(int direction) {
            int mechId = CIViewBaseLoadout.selectedUnitID;
            RefreshPilotModePilotIds();
            if (pilotModePilotIds.Count == 0)
            {
                pilotModePilotId = null;
                UpdatePilotModeButtonVisuals();
                return;
            }

            EnsurePilotModePilot(mechId);
            int pilotIndex = pilotModePilotIds.IndexOf(pilotModePilotId);
            if (pilotIndex < 0)
                pilotIndex = 0;

            pilotIndex = (pilotIndex + direction) % pilotModePilotIds.Count;
            if (pilotIndex < 0)
                pilotIndex += pilotModePilotIds.Count;

            pilotModePilotId = pilotModePilotIds[pilotIndex];
            pilotModePilotMechId = mechId;
            Debug.Log($"[LiveryGUI] Pilot livery edit target: {pilotModePilotId}");
            CIViewOverworldLog.AddMessage($"Pilot livery target: {GetPilotDisplayName(pilotModePilotId)} [sp={pilotSymbolSpriteName}]");
            ReapplyLiverySet(mechId);
        }

        //==============================================================================
        static string GetPilotDisplayName(string pilotId) {
            PersistentEntity pilot = IDUtility.GetPersistentEntity(pilotId);
            if (pilot == null)
                return pilotId ?? "<none>";
            TextUtility.GetPilotIdentificationText(pilot, out _, out string nameSecondary);
            return string.IsNullOrEmpty(nameSecondary) ? pilotId : nameSecondary;
        }

        //==============================================================================
        static void UpdatePilotModeReadout() {
            bool pilotModeActive = IsPilotModeActive();
            string pilotName = string.IsNullOrEmpty(pilotModePilotId) ? "[none]" : GetPilotDisplayName(pilotModePilotId);
            if (pilotModeNameLabel != null)
            {
                pilotModeNameLabel.text = pilotName;
                pilotModeNameLabel.gameObject.SetActive(pilotModeActive);
            }
        }

        //==============================================================================
        static Rect GetPilotModeLivePortraitRect() {
            UIRoot root = CIViewBaseLoadout.ins.gameObject.GetComponentInParent<UIRoot>();
            Vector2Int uiResolution = UIHelper.GetUIResolution(root, false);
            Vector2 localSize = new Vector2(Positions.pilotPortraitDim, Positions.pilotPortraitDim);
            Transform rootTransform = root.transform;
            Vector3 center = rootTransform.InverseTransformPoint(pilotModePortraitGO.transform.position);
            Vector3 right = rootTransform.InverseTransformPoint(pilotModePortraitGO.transform.TransformPoint(new Vector3(localSize.x * 0.5f, 0f, 0f)));
            Vector3 top = rootTransform.InverseTransformPoint(pilotModePortraitGO.transform.TransformPoint(new Vector3(0f, localSize.y * 0.5f, 0f)));
            float width = Mathf.Abs(right.x - center.x) * 2f;
            float height = Mathf.Abs(top.y - center.y) * 2f;
            float left = center.x - width * 0.5f;
            float bottom = center.y - height * 0.5f;
            float x = ((float)uiResolution.x * 0.5f + left) / (float)uiResolution.x;
            float y = ((float)uiResolution.y * 0.5f + bottom) / (float)uiResolution.y;
            return new Rect(x, y, width / (float)uiResolution.x, height / (float)uiResolution.y);
        }

        //==============================================================================
        static void UpdatePilotModeLivePortrait() {
            if (!IsPBLiveryViewActive() || pilotModePortraitGO == null)
            {
                DeactivatePilotModeLivePortrait();
                return;
            }

            PersistentEntity pilot = IDUtility.GetPersistentEntity(pilotModePilotId);
            if (pilot == null || !pilot.isPilotTag || !pilot.hasPilotAppearance)
            {
                DeactivatePilotModeLivePortrait();
                return;
            }

            PilotView pilotView = PilotView.Get(pilotModeLivePortraitViewId);
            if (pilotView == null || CIViewBaseEditor.ins == null)
                return;

            pilotView.SetActive(true);
            pilotView.OutputDirectly(GetPilotModeLivePortraitRect(), CIViewBaseEditor.ins.cameraDepth);
            if (!pilotModeLivePortraitActive || !string.Equals(pilotModeLivePortraitPilotIdLast, pilotModePilotId, StringComparison.Ordinal))
                pilotView.OnPilotChanged(pilot, null, false, false, null, null, 1f);
            SetPilotModeLivePortraitAnimatorUpdateMode(pilotView, AnimatorUpdateMode.UnscaledTime);

            pilotModeLivePortraitActive = true;
            pilotModeLivePortraitPilotIdLast = pilotModePilotId;
        }

        //==============================================================================
        static void SetPilotModeLivePortraitAnimatorUpdateMode(PilotView pilotView, AnimatorUpdateMode updateMode) {
            if (pilotView == null || pilotView.models == null)
                return;

            foreach (PilotViewBase.PilotModel model in pilotView.models)
            {
                Animator animator = model?.animator;
                if (animator == null)
                    continue;

                if (!pilotModeLivePortraitAnimatorUpdateModes.ContainsKey(animator))
                    pilotModeLivePortraitAnimatorUpdateModes[animator] = animator.updateMode;
                animator.updateMode = updateMode;
            }
        }

        //==============================================================================
        static void RestorePilotModeLivePortraitAnimatorUpdateModes() {
            foreach (KeyValuePair<Animator, AnimatorUpdateMode> item in pilotModeLivePortraitAnimatorUpdateModes)
            {
                if (item.Key != null)
                    item.Key.updateMode = item.Value;
            }
            pilotModeLivePortraitAnimatorUpdateModes.Clear();
        }

        //==============================================================================
        public static void DeactivatePilotModeLivePortrait(bool force = false) {
            if (!pilotModeLivePortraitActive && !force)
                return;

            PilotView pilotView = PilotView.Get(pilotModeLivePortraitViewId);
            RestorePilotModeLivePortraitAnimatorUpdateModes();
            if (pilotView != null)
            {
                pilotView.OutputToRenderTexture();
                pilotView.SetActive(false);
            }

            pilotModeLivePortraitActive = false;
            pilotModeLivePortraitPilotIdLast = null;
        }

        //==============================================================================
        public static void UpdatePilotModePortraitVisibility() {
            bool pilotModeActive = IsPilotModeActive();
            if (!pilotModeActive)
            {
                pilotModeNameLabel?.gameObject.SetActive(false);
                pilotModePortraitGO?.SetActive(false);
                DeactivatePilotModeLivePortrait();
                return;
            }

            UpdatePilotModeReadout();
            pilotModePortraitGO?.SetActive(false);
            UpdatePilotModeLivePortrait();
        }

        //==============================================================================
        static void OnLiveryNameInput() {
            // It would be possible to do something like (dangerous?) rename the livery, including changing its dictionary key.
            // This could leave dangling references.
            // Instead, we'll have the on-input field do nothing itself, and let the 'save' & 'create clone' buttons use this
            // field's value as the new key/name for the newly cloned livery.

            UpdateButtonColors();
            Contexts.sharedInstance.game.isInputBlocked = true; // (we've handled/consumed the input-event(s) this frame)
            return;
        }

        //==============================================================================
        static void UpdateWidgetPositioning(CIViewBaseLoadout __instance) {
            UIRoot root = __instance.gameObject.GetComponentInParent<UIRoot>();
            float pixelSizeAdj = root.pixelSizeAdjustment;

            var uiRoot = __instance.liveryRootObject.transform;
            float uiScale = uiRoot.lossyScale.x;
            int activeHeight = root.activeHeight;
            _ = uiRoot;
            _ = uiScale;
            _ = activeHeight;

            // calculate positions
            const float yStep = 38f;
            float[] x = {
                    613f,
                    (Screen.width * pixelSizeAdj - 334f)
                    // When adjusting this "pixel offset from edge", eg if I measure 271px that I want to move something,
                    // manually multiply that number by whatever pixelSizeAdj. So if I want to move 271px left, I take
                    // -271 and multiply by my current pixelSizeAdj (which is related to UI-scale factor from Display
                    // options menu). So the actual number to add here is -271 realPx * 0.75 codePx/realPx = -203 codePx.
                };
            Dev.Log($"[Livery-GUI] gui-scale-stuff Screen.width={Screen.width}, uiScale={uiScale}, x= {x[0]}, {x[1]}, activeHeight={activeHeight}, pixelSizeAdj={pixelSizeAdj}");
            // Example: gui-scale-stuff Screen.width=2560, uiScale=0.001851852, x= 613, 1586, activeHeight=1080, pixelSizeAdj=0.75
            float[] y = {
                    -yStep *  0.0f,
                    -yStep *  1.0f,
                    -yStep *  2.0f,
                    -yStep *  3.0f,
                    -yStep *  4.7f, // +0.7 gap
                    -yStep *  5.7f,
                    -yStep *  6.7f,
                    -yStep *  7.7f,
                    -yStep *  9.4f, // +0.7 gap
                    -yStep * 10.4f,
                    -yStep * 11.4f,
                    -yStep * 12.4f,
                    -yStep * 14.1f, // +0.7 gap
                    -yStep * 15.1f,
                    -yStep * 16.1f,
                    -yStep * 17.1f,
                };
            sliderHelpers["PrimaryR"  ].gameObject.transform.localPosition = new Vector3(x[0], y[ 0]);
            sliderHelpers["PrimaryG"  ].gameObject.transform.localPosition = new Vector3(x[0], y[ 1]);
            sliderHelpers["PrimaryB"  ].gameObject.transform.localPosition = new Vector3(x[0], y[ 2]);
            sliderHelpers["PrimaryA"  ].gameObject.transform.localPosition = new Vector3(x[0], y[ 3]);
            sliderHelpers["SecondaryR"].gameObject.transform.localPosition = new Vector3(x[0], y[ 4]);
            sliderHelpers["SecondaryG"].gameObject.transform.localPosition = new Vector3(x[0], y[ 5]);
            sliderHelpers["SecondaryB"].gameObject.transform.localPosition = new Vector3(x[0], y[ 6]);
            sliderHelpers["SecondaryA"].gameObject.transform.localPosition = new Vector3(x[0], y[ 7]);
            sliderHelpers["TertiaryR" ].gameObject.transform.localPosition = new Vector3(x[0], y[ 8]);
            sliderHelpers["TertiaryG" ].gameObject.transform.localPosition = new Vector3(x[0], y[ 9]);
            sliderHelpers["TertiaryB" ].gameObject.transform.localPosition = new Vector3(x[0], y[10]);
            sliderHelpers["TertiaryA" ].gameObject.transform.localPosition = new Vector3(x[0], y[11]);
            sliderHelpers["ContentX"  ].gameObject.transform.localPosition = new Vector3(x[0], y[12]);
            sliderHelpers["ContentY"  ].gameObject.transform.localPosition = new Vector3(x[0], y[13]);
            sliderHelpers["ContentZ"  ].gameObject.transform.localPosition = new Vector3(x[0], y[14]);
            sliderHelpers["ContentW"  ].gameObject.transform.localPosition = new Vector3(x[0], y[15]);
            sliderHelpers["PrimaryX"  ].gameObject.transform.localPosition = new Vector3(x[1], y[ 0]);
            sliderHelpers["PrimaryY"  ].gameObject.transform.localPosition = new Vector3(x[1], y[ 1]);
            sliderHelpers["PrimaryZ"  ].gameObject.transform.localPosition = new Vector3(x[1], y[ 2]);
            sliderHelpers["PrimaryW"  ].gameObject.transform.localPosition = new Vector3(x[1], y[ 3]);
            sliderHelpers["SecondaryX"].gameObject.transform.localPosition = new Vector3(x[1], y[ 4]);
            sliderHelpers["SecondaryY"].gameObject.transform.localPosition = new Vector3(x[1], y[ 5]);
            sliderHelpers["SecondaryZ"].gameObject.transform.localPosition = new Vector3(x[1], y[ 6]);
            sliderHelpers["SecondaryW"].gameObject.transform.localPosition = new Vector3(x[1], y[ 7]);
            sliderHelpers["TertiaryX" ].gameObject.transform.localPosition = new Vector3(x[1], y[ 8]);
            sliderHelpers["TertiaryY" ].gameObject.transform.localPosition = new Vector3(x[1], y[ 9]);
            sliderHelpers["TertiaryZ" ].gameObject.transform.localPosition = new Vector3(x[1], y[10]);
            sliderHelpers["TertiaryW" ].gameObject.transform.localPosition = new Vector3(x[1], y[11]);
            sliderHelpers["EffectX"   ].gameObject.transform.localPosition = new Vector3(x[1], y[12]);
            sliderHelpers["EffectY"   ].gameObject.transform.localPosition = new Vector3(x[1], y[13]);
            sliderHelpers["EffectZ"   ].gameObject.transform.localPosition = new Vector3(x[1], y[14]);
            sliderHelpers["EffectW"   ].gameObject.transform.localPosition = new Vector3(x[1], y[15]);

            float pilotCyclerBottomY = -(Screen.height * pixelSizeAdj) + Positions.pxGapAbovePaneGO + 66f;
            Vector3 pilotCyclerPos = new Vector3(Positions.pilotPanelLeft, pilotCyclerBottomY, 0f);
            pilotModePortraitGO.transform.localPosition = pilotCyclerPos + new Vector3(Positions.pilotPortraitDim/2f, -12f);
            pilotModePrevButton.gameObject.transform.localPosition = pilotCyclerPos + Positions.pilotPrevButtonOffset;
            pilotModeNextButton.gameObject.transform.localPosition = pilotCyclerPos + Positions.pilotNextButtonOffset;
            pilotModeNameLabel.gameObject.transform.localPosition  = pilotCyclerPos + Positions.pilotNameOffset;

            liveryNameInput.gameObject.transform.localPosition = paneGO.activeSelf ? Positions.liveryName : Positions.liveryName_hiddenOffscreen;
        }

        //==============================================================================
        static void OnClickSecondary(CIHelperSetting helper) {
            sliderRightClickHandler?.CaptureRightClickFor(helper.sliderBar);
        }

        //==============================================================================
        static void OnCloneLiveryClicked() {
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
        static string CloneSelectedLivery() {
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

            LiverySnapshotDB.originalLiveries.TryGetValue(origKey, out var lastSavedLivery);
            LiverySnapshotDB.AddLiveryDataSnapshot(newKey, lastSavedLivery.onDiskDat, true);

            Debug.Log($"[LiveryGUI] INFO: cloned livery {origKey} to {newKey}");
            CIViewOverworldLog.AddMessage($"Created a new copy of that livery. [sp=s_icon_l32_lc_grid_plus]");
            return newKey;
        }

        //==============================================================================
        static void SelectLivery(string liveryKey) {
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
        static DataContainerEquipmentLivery GetSelectedLivery() {
            string key = CIViewBaseLoadout.selectedUnitLivery;
            if (key.IsNullOrEmpty())
                return null;

            return DataMultiLinker<DataContainerEquipmentLivery>.GetEntry(key, false);
        }

        //==============================================================================
        static bool SelectedLiveryIsFavorited() {
            if (CIViewBaseLoadout.selectedUnitLivery.IsNullOrEmpty())
                return false;
            return (CIViewBaseLoadout.liveryKeysFavorite.Contains(CIViewBaseLoadout.selectedUnitLivery));
        }

        //==============================================================================
        static void ResetLiveryGUIWidgetsToMatchLivery(DataContainerEquipmentLivery livery) {
            LiverySetsDB.OnMaybeMechSelectionChanged();

            //Dev.Log($"[LiveryGUI] DEBUG-SPAM: ResetLiveryGUIWidgetsToMatchLivery CIViewBaseLoadout.selectedUnitLivery={CIViewBaseLoadout.selectedUnitLivery}");
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
                sliderHelpers["PrimaryR"  ].sliderBar.valueRaw = livery.colorPrimary.r;
                sliderHelpers["PrimaryG"  ].sliderBar.valueRaw = livery.colorPrimary.g;
                sliderHelpers["PrimaryB"  ].sliderBar.valueRaw = livery.colorPrimary.b;
                sliderHelpers["PrimaryA"  ].sliderBar.valueRaw = livery.colorPrimary.a;
                sliderHelpers["SecondaryR"].sliderBar.valueRaw = livery.colorSecondary.r;
                sliderHelpers["SecondaryG"].sliderBar.valueRaw = livery.colorSecondary.g;
                sliderHelpers["SecondaryB"].sliderBar.valueRaw = livery.colorSecondary.b;
                sliderHelpers["SecondaryA"].sliderBar.valueRaw = livery.colorSecondary.a;
                sliderHelpers["TertiaryR" ].sliderBar.valueRaw = livery.colorTertiary.r;
                sliderHelpers["TertiaryG" ].sliderBar.valueRaw = livery.colorTertiary.g;
                sliderHelpers["TertiaryB" ].sliderBar.valueRaw = livery.colorTertiary.b;
                sliderHelpers["TertiaryA" ].sliderBar.valueRaw = livery.colorTertiary.a;
                sliderHelpers["ContentX"  ].sliderBar.valueRaw = livery.contentParameters.x;
                sliderHelpers["ContentY"  ].sliderBar.valueRaw = livery.contentParameters.y;
                sliderHelpers["ContentZ"  ].sliderBar.valueRaw = livery.contentParameters.z;
                //sliderHelpers["ContentW"].sliderBar.valueRaw = livery.contentParameters.w;
                sliderHelpers["PrimaryX"  ].sliderBar.valueRaw = livery.materialPrimary.x;
                sliderHelpers["PrimaryY"  ].sliderBar.valueRaw = livery.materialPrimary.y;
                sliderHelpers["PrimaryZ"  ].sliderBar.valueRaw = livery.materialPrimary.z;
                sliderHelpers["PrimaryW"  ].sliderBar.valueRaw = livery.materialPrimary.w;
                sliderHelpers["SecondaryX"].sliderBar.valueRaw = livery.materialSecondary.x;
                sliderHelpers["SecondaryY"].sliderBar.valueRaw = livery.materialSecondary.y;
                sliderHelpers["SecondaryZ"].sliderBar.valueRaw = livery.materialSecondary.z;
                sliderHelpers["SecondaryW"].sliderBar.valueRaw = livery.materialSecondary.w;
                sliderHelpers["TertiaryX" ].sliderBar.valueRaw = livery.materialTertiary.x;
                sliderHelpers["TertiaryY" ].sliderBar.valueRaw = livery.materialTertiary.y;
                sliderHelpers["TertiaryZ" ].sliderBar.valueRaw = livery.materialTertiary.z;
                sliderHelpers["TertiaryW" ].sliderBar.valueRaw = livery.materialTertiary.w;
                sliderHelpers["EffectX"   ].sliderBar.valueRaw = livery.effect.x;
                sliderHelpers["EffectY"   ].sliderBar.valueRaw = livery.effect.y;
                sliderHelpers["EffectZ"   ].sliderBar.valueRaw = livery.effect.z;
                sliderHelpers["EffectW"   ].sliderBar.valueRaw = livery.effect.w;

                ContentW.SetLevel(livery.contentParameters.w);
                sliderHelpers["ContentW"].levelLabel.text = ContentW.GetLevelName();
                sliderHelpers["ContentW"].levelLabel.MarkAsChanged();
            }

            UpdateButtonColors();
        }

        //==============================================================================
        static void UpdateButtonColors() {
            bool liveryDataIsModified = LiverySnapshotDB.IsCurrentLiveryModified();
            bool liveryNameIsModified = (CIViewBaseLoadout.selectedUnitLivery != liveryNameInput.value);
            bool liveryIsModified = (liveryDataIsModified || liveryNameIsModified);

            var saveIcon = saveLiveryButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            var saveFillIdle = saveLiveryButton.gameObject.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
            if (saveIcon != null && saveFillIdle != null)
            {
                if (liveryIsModified &&
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
                if (liveryIsModified)
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

            var favoriteIcon = favoriteLiveryButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (favoriteIcon != null)
                favoriteIcon.spriteName = (SelectedLiveryIsFavorited()) ? spriteNameStarFilled : spriteNameStarOutline;
        }

        //==============================================================================
        static void UpdatePilotModeButtonVisuals() {
            if (pilotModeToggleButton == null)
                return;

            bool pilotModeActive = IsPilotModeActive();

            var go = pilotModeToggleButton.gameObject;
            var pilotFillIdle = go.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
            var pilotIcon = go.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();

            if (pilotFillIdle != null) { pilotFillIdle.color = pilotModeActive ? activeButtonFGColor : grayedOutButtonFGColor; } //todo.pilot-mode: fix color, or change to a toggle-slider or something. maybe two immediately-adjacent buttons, and shrink+gray the inactive one? click on either is a toggle of collective state?
            if (pilotIcon != null) { pilotIcon.color = new Color(0.9f, 0.9f, 0.9f, 0.8f); pilotIcon.spriteName = pilotSymbolSpriteName; }

            bool canCyclePilots = pilotModePilotIds.Count > 1;
            if (pilotModePrevButton != null)
            {
                pilotModePrevButton.gameObject.SetActive(pilotModeActive);
                pilotModePrevButton.available = canCyclePilots;
            }
            if (pilotModeNextButton != null)
            {
                pilotModeNextButton.gameObject.SetActive(pilotModeActive);
                pilotModeNextButton.available = canCyclePilots;
            }
            if (pilotModeBaseToggleButton != null)
            {
                pilotModeBaseToggleButton.gameObject.SetActive(pilotModeActive);
                pilotModeBaseToggleButton.available = true;

                var baseIcon = pilotModeBaseToggleButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                var baseFillIdle = pilotModeBaseToggleButton.gameObject.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
                if (baseIcon != null)
                    baseIcon.color = pilotModeMechBaseVisible ? activeButtonFGColor : grayedOutButtonFGColor;
                if (baseFillIdle != null)
                    baseFillIdle.color = pilotModeMechBaseVisible ? activeButtonBGColor : grayedOutButtonBGColor;
            }
            UpdatePilotModePortraitVisibility();
        }

        //==============================================================================
        static void UpdateLiveryFromSliders() {
            var livery = GetSelectedLivery();
            if (livery == null)
                return;
            livery.colorPrimary.r        = sliderHelpers["PrimaryR"  ].sliderBar.valueRaw;
            livery.colorPrimary.g        = sliderHelpers["PrimaryG"  ].sliderBar.valueRaw;
            livery.colorPrimary.b        = sliderHelpers["PrimaryB"  ].sliderBar.valueRaw;
            livery.colorPrimary.a        = sliderHelpers["PrimaryA"  ].sliderBar.valueRaw;
            livery.colorSecondary.r      = sliderHelpers["SecondaryR"].sliderBar.valueRaw;
            livery.colorSecondary.g      = sliderHelpers["SecondaryG"].sliderBar.valueRaw;
            livery.colorSecondary.b      = sliderHelpers["SecondaryB"].sliderBar.valueRaw;
            livery.colorSecondary.a      = sliderHelpers["SecondaryA"].sliderBar.valueRaw;
            livery.colorTertiary.r       = sliderHelpers["TertiaryR" ].sliderBar.valueRaw;
            livery.colorTertiary.g       = sliderHelpers["TertiaryG" ].sliderBar.valueRaw;
            livery.colorTertiary.b       = sliderHelpers["TertiaryB" ].sliderBar.valueRaw;
            livery.colorTertiary.a       = sliderHelpers["TertiaryA" ].sliderBar.valueRaw;
            livery.contentParameters.x   = sliderHelpers["ContentX"  ].sliderBar.valueRaw;
            livery.contentParameters.y   = sliderHelpers["ContentY"  ].sliderBar.valueRaw;
            livery.contentParameters.z   = sliderHelpers["ContentZ"  ].sliderBar.valueRaw;
            //livery.contentParameters.w = sliderHelpers["ContentW"  ].sliderBar.valueRaw;
            livery.materialPrimary.x     = sliderHelpers["PrimaryX"  ].sliderBar.valueRaw;
            livery.materialPrimary.y     = sliderHelpers["PrimaryY"  ].sliderBar.valueRaw;
            livery.materialPrimary.z     = sliderHelpers["PrimaryZ"  ].sliderBar.valueRaw;
            livery.materialPrimary.w     = sliderHelpers["PrimaryW"  ].sliderBar.valueRaw;
            livery.materialSecondary.x   = sliderHelpers["SecondaryX"].sliderBar.valueRaw;
            livery.materialSecondary.y   = sliderHelpers["SecondaryY"].sliderBar.valueRaw;
            livery.materialSecondary.z   = sliderHelpers["SecondaryZ"].sliderBar.valueRaw;
            livery.materialSecondary.w   = sliderHelpers["SecondaryW"].sliderBar.valueRaw;
            livery.materialTertiary.x    = sliderHelpers["TertiaryX" ].sliderBar.valueRaw;
            livery.materialTertiary.y    = sliderHelpers["TertiaryY" ].sliderBar.valueRaw;
            livery.materialTertiary.z    = sliderHelpers["TertiaryZ" ].sliderBar.valueRaw;
            livery.materialTertiary.w    = sliderHelpers["TertiaryW" ].sliderBar.valueRaw;
            livery.effect.x              = sliderHelpers["EffectX"   ].sliderBar.valueRaw;
            livery.effect.y              = sliderHelpers["EffectY"   ].sliderBar.valueRaw;
            livery.effect.z              = sliderHelpers["EffectZ"   ].sliderBar.valueRaw;
            livery.effect.w              = sliderHelpers["EffectW"   ].sliderBar.valueRaw;

            livery.contentParameters.w = ContentW.GetLevelValue();

            //todo: there's some circumstances that can cause severe graphical glitches in the form of multiple overlapping flickering large white circles with fuzzy edges, often with a black square in the middle.
            //  seems to affect light gray / white moreso than other colors, but significantly tied to some interplay between xary.W and xary.effect. (sum of color channels affects susceptibility?).
            //  cap 1ary.W to -3..+3. but the loss of +4..+5 is visible loss and non-ideal. but +5 is very unfriendly to effect.
            //  cap 1ary.effect to -4..+4
            //  constrain 1ary.W with respect to 1ary.effect: 1ary.W=+1.5 is about the max safe value while 1ary.effect is -4.
            //  constrain 1ary.W when adjusting 1ary.effect: as 1ary.effect goes from 0..-5, cap 1ary.W to be at most 5..1 (reduce positive-magnitude of this, while negative-magnitude of the other increases)
            //  constrain 1ary.W when adjusting 1ary.effect: as 1ary.effect goes from 0..+5, cap 1ary.W to be more than -5..-1 (reduce neg-magnitude of this, while pos-magnitude of the other increases)
            //  also constrain 1ary.effect while 1ary.W is being adjusted.
            //  also apply to 2ary and 3ary.
            //
            // suppose i limit the R+G+B for a part to sum to 2.1 or so. how gray is that, can it be improved via negative-alpha/negative-metal. simple linear sum rather than perceptual? and would need to figure out how to hook that up so as to not cause feedback issues while actively dragging the color sliders. Can go down to about RGB=.5,.5,.5 and use negative alpha to brighten it to apparently full-white. Perhaps a larger-negative alpha cap could further work around this?
        }

        //==============================================================================
        static void RefreshSphereAndMechPreviews() {
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
