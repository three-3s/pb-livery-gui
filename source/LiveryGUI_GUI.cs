using HarmonyLib;
using PhantomBrigade;
using PhantomBrigade.Data;
using System;
using System.Collections.Generic;
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
        public int column;
        public int row;
        public Color fillColor;
        public float min;
        public float max;
        public SliderKind sliderKind;
        public SliderConfig(string name, string label, int column, int row, Color fillColor, float min = 0f, float max = 1f, SliderKind sliderKind = SliderKind.Continuous) {
            this.name = name;
            this.label = label;
            this.column = column;
            this.row = row;
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
            LiveryGUIMod.GUI.UpdatePilotModeBasePulseLivery();
            LiveryGUIMod.GUI.UpdatePilotModePortraitVisibility();
        }
    }

    public class GUI {
        public static GameObject paneGO = null;
        public static Dictionary<string, CIHelperSetting> sliderHelpers = null; // key = livery's key
        public static Dictionary<string, SliderConfig> sliderConfigs = null; // key = livery's key
        static Dictionary<string, CIButton> settingTooltipButtons = null; // key = livery's key. (one per sliderHelpers[i])
        public static CIButton toggleLiveryGUIButton;
        public static CIButton pilotModeToggleButton;
        public static CIButton pilotModePrevButton;
        public static CIButton pilotModeNextButton;
        public static CIButton pilotModeBaseToggleButton;
        public static UILabel pilotModeNameLabel;
        public static GameObject pilotModePortraitGO;
        public static CIButton resetLiveryButton;
        public static CIButton saveLiveryButton;
        public static CIButton favoriteLiveryButton; // (same as built-in right-click on the livery icons)
        public static UIInput liveryNameInput;
        static readonly Color activeButtonFGColor = new Color(0.6f, 0.7f, 1f, 1f);
        static readonly Color activeButtonBGColor = new Color(0.25f, 0.36f, 0.58f, 0.7f);
        static readonly Color grayedOutButtonFGColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        static readonly Color grayedOutButtonBGColor = new Color(0.0f, 0.0f, 0.0f, 0.7f);
        static readonly Color activeRedButtonFGColor = new Color(1f, 0.38f, 0.38f, 1f);
        static readonly Color activeRedButtonBGColor = new Color(0.61f, 0.22f, 0.22f, 1f);
        static readonly Color pilotLiverySlotMarkerColor = new Color(0.32f, 0.58f, 1f, 0.95f);
        static readonly Color mechLiverySlotMarkerColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
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
        static readonly string settingTooltipButtonSpriteName = "icon_help_outline";
        const int settingLowerRowsStart = 8;
        static readonly Vector3 liverySlotLayerMarkerOffset = new Vector3(180f, 0f, 0f);
        static readonly string pilotLiverySlotMarkerTooltipHeader = "Pilot livery";
        static readonly string pilotLiverySlotMarkerTooltipContent = "The pilot's own livery-set is applied to this slot. This livery will be applied to this slot of any mech that is piloted by this pilot. Note: Any slot marked with a '+' has child slots, which will be affected by the parent slot.";
        static readonly string mechLiverySlotMarkerTooltipHeader = "Mech base livery";
        static readonly string mechLiverySlotMarkerTooltipContent = "No pilot livery has been assigned to this slot. The mech's own base livery will control this slot. Assigning a livery to this slot will assign the livery to the pilot's livery-set.";
        static readonly Color liverySlotChildMarkerColor = new Color(0.45f, 0.45f, 0.45f, 0.95f);
        static readonly Color liverySlotChildMarkerAssignedColor = new Color(1f, 1f, 1f, 1f);
        static readonly Color liverySlotHierarchyLineColor = new Color(0.7f, 0.7f, 0.7f, 0.75f);
        const float pilotModeBasePulsePeriod = 2.5f;
        const float pilotModeBasePulseMin = 0.5f;
        const float pilotModeBasePulseMax = 0.8f;
        static readonly FieldInfo liveryHelpersPerSocketField = AccessTools.Field(typeof(CIViewBaseLoadout), "liveryHelpersPerSocket");
        static readonly FieldInfo liveryHelpersPerHardpointField = AccessTools.Field(typeof(CIViewBaseLoadout), "liveryHelpersPerHardpoint");

        public static bool modPrevPilotModeActive = false; // persists whether pilot-edit-mode is active, including when mod-gui is off.
        static bool reapplyLiverySetInProgress = false;
        static readonly bool suppressLiverySlotRecording = false; // (currently unused)
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

            public static readonly Vector3 legendGroup1Item1 = saveButton + 1f * posStep;
            public static readonly Vector3 legendGroup2Item1 = saveButton + 2f * posStep;
            public static readonly Vector3 legendGroup2Item2 = saveButton + 2f * posStep + 1f * minimalPosStep;

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
            Color R = new Color(0.8f, 0.3f, 0.3f, 0.5f);
            Color G = new Color(0.3f, 0.8f, 0.3f, 0.5f);
            Color B = new Color(0.3f, 0.3f, 0.8f, 0.6f);
            Color A = new Color(1f, 1f, 1f, 0.37f);
            sliderHelpers = new Dictionary<string, CIHelperSetting>();
            settingTooltipButtons = new Dictionary<string, CIButton>();
            sliderConfigs = new Dictionary<string, SliderConfig>() {
                    { "PrimaryR",   new SliderConfig("PrimaryR",   "R              Primary Color",   0,  0, R, loC,  hiC) },
                    { "PrimaryG",   new SliderConfig("PrimaryG",   "G",                              0,  1, G, loC,  hiC) },
                    { "PrimaryB",   new SliderConfig("PrimaryB",   "B",                              0,  2, B, loC,  hiC) },
                    { "PrimaryA",   new SliderConfig("PrimaryA",   "A",                              0,  3, A, loA,  hiA) },
                    { "SecondaryR", new SliderConfig("SecondaryR", "R              Secondary Color", 0,  4, R, loC,  hiC) },
                    { "SecondaryG", new SliderConfig("SecondaryG", "G",                              0,  5, G, loC,  hiC) },
                    { "SecondaryB", new SliderConfig("SecondaryB", "B",                              0,  6, B, loC,  hiC) },
                    { "SecondaryA", new SliderConfig("SecondaryA", "A",                              0,  7, A, loA,  hiA) },
                    { "TertiaryR",  new SliderConfig("TertiaryR",  "R              Tertiary Color",  0,  8, R, loC,  hiC) },
                    { "TertiaryG",  new SliderConfig("TertiaryG",  "G",                              0,  9, G, loC,  hiC) },
                    { "TertiaryB",  new SliderConfig("TertiaryB",  "B",                              0, 10, B, loC,  hiC) },
                    { "TertiaryA",  new SliderConfig("TertiaryA",  "A",                              0, 11, A, loA,  hiA) },
                    { "ContentX",   new SliderConfig("ContentX",   "R         Supporter DLC Color",  0, 12, R, loNC, hiNC) },
                    { "ContentY",   new SliderConfig("ContentY",   "G",                              0, 13, G, loNC, hiNC) },
                    { "ContentZ",   new SliderConfig("ContentZ",   "B",                              0, 14, B, loNC, hiNC) },
                    { "ContentW",   new SliderConfig("ContentW",   "Sup. DLC Pattern",               0, 15, A, loNC, hiNC, SliderKind.Discrete) },
                    { "PrimaryX",   new SliderConfig("PrimaryX",   "low       Primary Shininess",    1,  0, A, loS,  hiS)  },
                    { "PrimaryY",   new SliderConfig("PrimaryY",   "mid",                            1,  1, A, loS,  hiS)  },
                    { "PrimaryZ",   new SliderConfig("PrimaryZ",   "high",                           1,  2, A, loS,  hiS)  },
                    { "PrimaryW",   new SliderConfig("PrimaryW",   "           Metalness",           1,  3, B, loNC, hiNC) },
                    { "SecondaryX", new SliderConfig("SecondaryX", "low       Secondary Shininess",  1,  4, A, loS,  hiS)  },
                    { "SecondaryY", new SliderConfig("SecondaryY", "mid",                            1,  5, A, loS,  hiS)  },
                    { "SecondaryZ", new SliderConfig("SecondaryZ", "high",                           1,  6, A, loS,  hiS)  },
                    { "SecondaryW", new SliderConfig("SecondaryW", "           Metalness",           1,  7, B, loNC, hiNC) },
                    { "TertiaryX",  new SliderConfig("TertiaryX",  "low       Tertiary Shininess",   1,  8, A, loS,  hiS)  },
                    { "TertiaryY",  new SliderConfig("TertiaryY",  "mid",                            1,  9, A, loS,  hiS)  },
                    { "TertiaryZ",  new SliderConfig("TertiaryZ",  "high",                           1, 10, A, loS,  hiS)  },
                    { "TertiaryW",  new SliderConfig("TertiaryW",  "           Metalness",           1, 11, B, loNC, hiNC) },
                    { "EffectX",    new SliderConfig("EffectX",    "Primary            Iridescence", 1, 12, A, loNC, hiNC) },
                    { "EffectY",    new SliderConfig("EffectY",    "Secondary",                      1, 13, A, loNC, hiNC) },
                    { "EffectZ",    new SliderConfig("EffectZ",    "Tertiary",                       1, 14, A, loNC, hiNC) },
                    { "EffectW",    new SliderConfig("EffectW",    "Unused",                         1, 15, B, loNC, hiNC) },
                };

            // add sliders to pane
            // (by cloning the 'options menu' prefab sliders)
            var helperPrefab = CIViewPauseOptions.ins.settingPrefab;

            foreach (var item in sliderConfigs) {
                string key = item.Key;
                SliderConfig cfg = item.Value;

                var helperGO = GameObject.Instantiate(helperPrefab.gameObject, paneGO.transform, false);
                helperGO.name = cfg.name;
                CIHelperSetting helper = helperGO.GetComponent<CIHelperSetting>();
                helper.sharedLabelName.text = cfg.label;

                helper.sliderBar.callbackOnClickSecondary = new UICallback(value => {
                    OnClickSecondary(helper);
                }, 0f);

                Vector3 sliderLocalPos = helper.sliderHolder.transform.localPosition;
                sliderLocalPos.x -= 262f; // move the slider-bar left so it's under the label-text
                sliderLocalPos.y += 1f;
                helper.sliderHolder.transform.localPosition = sliderLocalPos;

                helper.sliderBar.valueMin = cfg.min;
                helper.sliderBar.valueLimit = cfg.max;
                helper.sliderBar.labelFormat = "F3";
                helper.sliderBar.labelSuffix = "";
                MoveSliderValueReadout(helper.sliderBar);
                helper.sliderBar.spriteFill.color = cfg.fillColor;

                helper.sharedSpriteBackground.gameObject.SetActive(false);
                helper.sharedSpriteGradient.gameObject.SetActive(false);
                helper.scrollElement.buttonBody.tooltipUsed = false;

                helper.sliderBar.callbackOnAdjustment = new UICallback(value => {
                    UpdateLiveryFromSliders();
                    RefreshSphereAndMechPreviews();
                }, 0f);

                // which aspect of the widget should be visible
                helper.sliderHolder.SetActive(false);
                helper.levelHolder.SetActive(false);
                helper.toggleHolder.SetActive(false);
                if (cfg.sliderKind == SliderKind.Continuous) {
                    helper.sliderHolder.SetActive(true);
                } else if (cfg.sliderKind == SliderKind.Discrete) {
                    helper.levelHolder.SetActive(true);
                    helper.levelHolder.transform.localPosition = sliderLocalPos + new Vector3(68f, 5f);
                    helper.levelButtonLeft.transform.localPosition += new Vector3(68f, -5f);
                    helper.levelButtonRight.transform.localPosition += new Vector3(-68f, -5f);
                    //helper.sharedSpriteGradient.transform.localScale = new Vector3(0.518f, 0.919f); // not sure changing the sharedSpriteGradient doesn't affect Options menu
                    //helper.sharedSpriteGradient.SetRGBColor(new Color(0.0f, 0.0f, 0.0f, 1f)); // very faint
                    //helper.sharedSpriteGradient.gameObject.SetActive(true);
                    helper.levelButtonLeft.callbackOnClick = new UICallback(() => {
                        ContentW.idx = (ContentW.idx - 1 + ContentW.names.Length) % ContentW.names.Length;
                        UpdateContentWLevelWidget();
                        UpdateLiveryFromSliders();
                        RefreshSphereAndMechPreviews();
                    });
                    helper.levelButtonRight.callbackOnClick = new UICallback(() => {
                        ContentW.idx = (ContentW.idx + 1) % ContentW.names.Length;
                        UpdateContentWLevelWidget();
                        UpdateLiveryFromSliders();
                        RefreshSphereAndMechPreviews();
                    });
                    UpdateDiscreteLevelWidget(helper, ContentW.idx, ContentW.names.Length, ContentW.GetLevelName());
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
            toggleLiveryGUIButton.callbackOnClick = new UICallback(() => {
                paneGO.SetActive(!paneGO.activeSelf);
                if (CIViewBaseLoadout.selectedUnitID >= 0)
                    ReapplyLiverySet(CIViewBaseLoadout.selectedUnitID);
                else {
                    UpdateWidgetPositioning(__instance);
                    ResetLiveryGUIWidgetsToMatchLivery(GetSelectedLivery());
                    UpdatePilotModeButtonVisuals();
                }
            });

            CreateSettingTooltipButtons(buttonTemplate: toggleLiveryGUIButtonGO);
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
            pilotModeToggleButton.callbackOnClick = new UICallback(() => {
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
            pilotModeBaseToggleButton.callbackOnClick = new UICallback(() => {
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
            pilotModePrevButton.callbackOnClick = new UICallback(() => {
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
            pilotModeNextButton.callbackOnClick = new UICallback(() => {
                CyclePilotModePilot(+1);
            });
            pilotModeNextButton.tooltipUsed = true;
            pilotModeNextButton.AddTooltip("Next Pilot", "Select the next pilot. This controls which pilot's livery-set is being edited in 'pilot livery-set editing mode'. (This does not assign the pilot to the mech. Assigning a pilot to a mech is done in mission briefing.)");
            pilotModeNextButton.tooltipDelay = false;
            pilotModeNextButton.tooltipOffset = new Vector3(45f, 0f, 0f);
            pilotModeNextButton.tooltipPivot = UIWidget.Pivot.BottomLeft;

            CreatePilotModeReadoutWidgets(labelTemplate: helperPrefab.sharedLabelName);

            ////////////////////////////////////////////////////////////////////////////////
            // Livery GUI buttons: 'revert changes', 'save livery to disk'
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

            resetLiveryButton = resetLiveryButtonGO.GetComponent<CIButton>();
            resetLiveryButton.AddTooltip("Revert Changes", "Reset this livery to its last saved values.");
            resetLiveryButton.tooltipDelay = false;
            resetLiveryButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            resetLiveryButton.tooltipOffset = new Vector3(44f, -62f, 0f);
            resetLiveryButton.callbackOnClick = new UICallback(() => {
                OnResetLiveryClicked();
            });

            saveLiveryButton = saveLiveryButtonGO.GetComponent<CIButton>();
            saveLiveryButton.AddTooltip("Save Livery", "Save this livery, using the given name. The name used must not match any built-in livery, nor any livery provided by a mod. Using a name that does not exist yet will save to a new livery with that name.");
            saveLiveryButton.tooltipDelay = false;
            saveLiveryButton.tooltipPivot = UIWidget.Pivot.TopLeft;
            saveLiveryButton.tooltipOffset = new Vector3(44f, -62f, 0f);
            saveLiveryButton.callbackOnClick = new UICallback(() => {
                if (!HasSelectedLivery()) {
                    UpdateButtonColors();
                    return;
                }

                if (CIViewBaseLoadout.selectedUnitLivery != liveryNameInput.value)
                    CreateCopyForEditedName(); // new key/name was given; create it before saving it
                if (CIViewBaseLoadout.selectedUnitLivery == liveryNameInput.value) {
                    LoadAndSave.SaveLiveryToFile(CIViewBaseLoadout.selectedUnitLivery, GetSelectedLivery());
                    UpdateButtonColors();
                }
            });

            ////////////////////////////////////////////////////////////////////////////////
            // Livery GUI text-input field: for seeing & renaming the livery-key/file-name
            GameObject liveryNameInputGO = GameObject.Instantiate(CIViewBaseLoadout.ins.headerInputUnitName.gameObject, uiRoot/*paneGO.transform*/, false); // bug workaround: using uiRoot to make this "visible" as a workaround for the text-display refusing to initially populate until it gets displayed AND THEN user clicks on something (after which it starts updating/working fine). We manually move this widget off-screen while LiveryGUI is toggled off.
            liveryNameInputGO.name = "liveryNameInputGO";
            liveryNameInputGO.transform.localPosition = Positions.liveryName_hiddenOffscreen;
            liveryNameInputGO.transform.localScale = Vector3.one;

            liveryNameInput = liveryNameInputGO.GetComponent<UIInput>();
            ConfigureLiveryNameInput(liveryNameInputGO);
            liveryNameInput.onChange = new List<EventDelegate>() { new EventDelegate(new EventDelegate.Callback(OnLiveryNameInput)) };
            liveryNameInput.onReturnKey = UIInput.OnReturnKey.Submit;
            liveryNameInput.onSubmit = new List<EventDelegate> { new EventDelegate(new EventDelegate.Callback(OnLiveryNameInput)) };
            liveryNameInput.validation = UIInput.Validation.None;
            liveryNameInput.characterLimit += 10;
            liveryNameInput.defaultText = "Livery Name";

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
            favoriteLiveryButton.callbackOnClick = new UICallback(() => {
                object[] args = { CIViewBaseLoadout.selectedUnitLivery };
                AccessTools.Method(typeof(CIViewBaseLoadout), "OnLiveryFavoriteToggle").Invoke(CIViewBaseLoadout.ins, args); // (the built-in on-click handler when player right-clicks on a livery in the list)
                UpdateButtonColors();
            });

            ////////////////////////////////////////////////////////////////////////////////
            // row of no-op buttons, as a crude legend/hint about usable controls.
            const string tooltip1 = "Click on sliders or click-and-drag to set.\n\nRemember to save.\nNormal values for sliders are between 0 and 1. Other values might work, but they might cause problems.\n\nYou can use keyboard \"Move Camera\" (Move Up/Down/Left/Right) keys to adjust view.";
            const string tooltip2 = "For precise adjustments:\nRight-click-and-hold, and move mouse <--->\nChange speed: Hold SHIFT, ALT, or CTRL.";
            LegendConfig[] legendConfigs = {
                // note: the button icons seem to get left/right mirror'd
                new LegendConfig("mouse_right_outline", Positions.legendGroup1Item1, tooltip1),
                new LegendConfig("mouse_left_outline",  Positions.legendGroup2Item1, tooltip2),
                new LegendConfig("mouse_horizontal",    Positions.legendGroup2Item2, tooltip2),
            };
            Color legendColor = new Color(1f, 0f, 0f, 0.2f);

            foreach (LegendConfig legendConfig in legendConfigs) {
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
        static void ConfigureLiveryNameInput(GameObject inputGO) {
            if (liveryNameInput == null || inputGO == null)
                return;

            UILabel[] labels = inputGO.GetComponentsInChildren<UILabel>(true);
            foreach (UILabel label in labels) {
                if (label == null)
                    continue;

                label.modifier = UILabel.Modifier.None;
                if (label == liveryNameInput.label && string.Equals(label.text, "Name", StringComparison.OrdinalIgnoreCase))
                    label.text = "Livery Name";
                if (label != liveryNameInput.label && string.Equals(label.text, "Name", StringComparison.OrdinalIgnoreCase)) {
                    label.text = "Livery Name";
                    label.MarkAsChanged();
                }
            }

            if (liveryNameInput.label != null) {
                liveryNameInput.label.modifier = UILabel.Modifier.None;
                liveryNameInput.label.MarkAsChanged();
            }
        }

        //==============================================================================
        static void MoveSliderValueReadout(Component sliderBar) {
            if (sliderBar == null)
                return;

            UILabel[] labels = sliderBar.GetComponentsInChildren<UILabel>(true);
            foreach (UILabel label in labels) {
                if (label == null)
                    continue;

                Transform labelTransform = label.transform;
                labelTransform.localPosition += new Vector3(+119f, 0f, 0f);
            }
        }

        //==============================================================================
        static void CreateSettingTooltipButtons(GameObject buttonTemplate) {
            if (buttonTemplate == null || sliderConfigs == null || settingTooltipButtons == null)
                return;

            foreach (KeyValuePair<string, SliderConfig> item in sliderConfigs) {
                string key = item.Key;
                SliderConfig cfg = item.Value;
                string content = GetSettingTooltipContent(key);
                if (string.IsNullOrEmpty(content))
                    continue;

                GameObject buttonGO = GameObject.Instantiate(buttonTemplate, paneGO.transform, false);
                buttonGO.name = "SettingTooltipButton_" + key;
                buttonGO.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                var icon = buttonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                var frame = buttonGO.transform.Find("Sprite_Frame")?.GetComponent<UISprite>();
                var fillIdle = buttonGO.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
                var fillHover = buttonGO.transform.Find("Sprite_Fill_Hover")?.GetComponent<UISprite>();
                if (icon != null) {
                    icon.spriteName = settingTooltipButtonSpriteName;
                    icon.flip = UIBasicSprite.Flip.Nothing;
                    icon.color = new Color(0.85f, 0.9f, 1f, 0.8f);
                }
                if (frame != null)
                    frame.color = new Color(0.6f, 0.65f, 0.75f, 0.45f);
                if (fillIdle != null)
                    fillIdle.color = new Color(0f, 0f, 0f, 0.3f);
                if (fillHover != null)
                    fillHover.color = new Color(0.55f, 0.65f, 0.85f, 0.35f);

                CIButton button = buttonGO.GetComponent<CIButton>();
                button.callbackOnClick = null;
                button.callbackOnClickLong = null;
                button.callbackOnClickSecondary = null;
                if (button.audio != null)
                    button.audio.enabled = false;
                ConfigureSettingTooltip(button, cfg, content);
                settingTooltipButtons[key] = button;
                buttonGO.SetActive(true);
            }
        }

        //==============================================================================
        static void ConfigureSettingTooltip(CIButton button, SliderConfig cfg, string content) {
            if (button == null || cfg == null || string.IsNullOrEmpty(content))
                return;

            button.tooltipUsed = true;
            button.tooltipKey = null;
            button.AddTooltip(cfg.label, content);
            button.tooltipDelay = false;
            button.tooltipPivot  = GetSettingTooltipPivot(cfg);
            button.tooltipOffset = GetSettingTooltipOffset(cfg);
        }

        //==============================================================================
        static UIWidget.Pivot GetSettingTooltipPivot(SliderConfig cfg) {
            bool rightColumn = IsRightColumnSetting(cfg);
            bool lowerRows = IsLowerSettingRow(cfg);
            if (rightColumn)
                return lowerRows ? UIWidget.Pivot.BottomRight : UIWidget.Pivot.TopRight;
            return lowerRows ? UIWidget.Pivot.BottomLeft : UIWidget.Pivot.TopLeft;
        }

        //==============================================================================
        static Vector3 GetSettingTooltipOffset(SliderConfig cfg) {
            if (IsRightColumnSetting(cfg)) {
                if (IsLowerSettingRow(cfg))
                    return new Vector3( -8f,  +11f, 0f);  // button-loc [ .]   tooltip-pos *[?]
                else
                    return new Vector3( -6f, -137f, 0f);  // button-loc [ *]   tooltip-pos .[?]
            } else {
                if (IsLowerSettingRow(cfg))
                    return new Vector3(+54f,   +9f, 0f);  // button-loc [. ]   tooltip-pos  [?]*
                else
                    return new Vector3(+52f,  -75f, 0f);  // button-loc [* ]   tooltip-pos  [?].
            }
        }

        //==============================================================================
        static bool IsRightColumnSetting(SliderConfig cfg) {
            return cfg != null && cfg.column > 0;
        }

        //==============================================================================
        static bool IsLowerSettingRow(SliderConfig cfg) {
            return cfg != null && cfg.row >= settingLowerRowsStart;
        }

        //==============================================================================
        static string GetSettingTooltipContent(string key) {
            if (string.IsNullOrEmpty(key))
                return null;

            if (key.StartsWith("Content", StringComparison.Ordinal))
                return GetSupporterDLCTooltipContent(key);

            if (key.StartsWith("Effect", StringComparison.Ordinal))
                return GetEffectTooltipContent(key);

            string layerName = GetPaintLayerName(key);
            if (layerName == null)
                return null;

            char channel = key[key.Length - 1];
            if (channel == 'R' || channel == 'G' || channel == 'B') {
                string colorName = channel == 'R' ? "red" : (channel == 'G' ? "green" : "blue");
                return "Sets the " + colorName + " amount for the " + layerName + " areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs.";
            }

            if (channel == 'A')
                return "Adjusts how strongly the " + layerName + " color is applied. Around 1 is the normal look. Lower or negative values tend to brighten or wash the color toward white; higher values darken it toward black, with a unique interaction with colors brighter than 1.\n\nThis behaves more like a brightness/contrast control than ordinary transparency, and extreme values can produce unusual results.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs.";

            if (channel == 'X')
                return "Controls the matte-to-shiny response for the duller regions of the " + layerName + " material. Lower values look flatter and more painted; higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs.";

            if (channel == 'Y')
                return "Controls the matte-to-shiny response for the middle regions of the " + layerName + " material (the areas that are not marked as \"dullest\" nor \"smoothest\"). Higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs.";

            if (channel == 'Z')
                return "Controls the matte-to-shiny response for the brightest or most polished regions of the " + layerName + " material. Higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs.";

            if (channel == 'W')
                return "Controls \"metalness\" for the " + layerName + " material. Around 0 behaves more like paint or plastic: the angle of the material doesn't affect how it looks. Around 1 behaves more like metal: the angle of the material does affect how that part of the material looks. Values outside 0 to 1 are experimental. Negative values seem to \"glow\", and larger positive values affect brightness of smooth-shininess and can cause exotic color effects, especially with mixed RGB values like (R,G,B)=(1.2, 0.5, 0.8) or (1.2, 0.4, 1.2), with a mix of above-1 and below-1 values.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs. When using large 'W' values, turn down X,Y,Z shininess to help avoid this.";

            return null;
        }

        //==============================================================================
        static string GetPaintLayerName(string key) {
            if (key.StartsWith("Primary", StringComparison.Ordinal))
                return "primary";
            if (key.StartsWith("Secondary", StringComparison.Ordinal))
                return "secondary";
            if (key.StartsWith("Tertiary", StringComparison.Ordinal))
                return "tertiary";
            return null;
        }

        //==============================================================================
        static string GetSupporterDLCTooltipContent(string key) {
            char channel = key[key.Length - 1];
            if (channel == 'W')
                return "Chooses the Supporter Upgrade DLC overlay pattern. This only has a visible effect if the Supporter Upgrade DLC is installed. The pattern is \"additive\", so the color must be brighter than black to be visible.";

            string colorName = channel == 'X' ? "red" : (channel == 'Y' ? "green" : "blue");
            return "Sets the " + colorName + " amount for the Supporter Upgrade DLC overlay pattern. This only has a visible effect if the Supporter Upgrade DLC is installed and a pattern other than \"none\" is selected (the Supporter DLC W setting).\n\nValues are additive and can create bright glow or bloom. Values that are too negative can disable the effect. Mixing positive and negative values can produce unusual glow, such as (R,G,B)=(+5, -5, -1.8)";
        }

        //==============================================================================
        static string GetEffectTooltipContent(string key) {
            char channel = key[key.Length - 1];
            if (channel == 'W')
                return "Reserved effect value. It currently does not appear to have a visible effect, but it is saved for completeness and experimentation.";

            string layerName = channel == 'X' ? "primary" : (channel == 'Y' ? "secondary" : "tertiary");
            return "Controls \"iridescence\" for the " + layerName + " paint areas. Positive values tend toward stronger rainbow iridescence. Negative values tend toward a pearlescent look. Values near 0 have little or no effect. Large values (positive or negative) will look \"unusual\". \"Metalness\" and the part's R,G,B color affect how this looks.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs.";
        }

        //==============================================================================
        static void UpdateContentWLevelWidget() {
            if (sliderHelpers == null || !sliderHelpers.TryGetValue("ContentW", out CIHelperSetting helper))
                return;

            UpdateDiscreteLevelWidget(helper, ContentW.idx, ContentW.names.Length, ContentW.GetLevelName());
        }

        //==============================================================================
        static void UpdateDiscreteLevelWidget(CIHelperSetting helper, int selectedIndex, int optionCount, string labelText) {
            if (helper == null)
                return;

            if (helper.levelLabel != null) {
                helper.levelLabel.text = labelText;
                helper.levelLabel.MarkAsChanged();
            }

            if (helper.levelSpriteBackground != null) {
                helper.levelSpriteBackground.width = 8 * Math.Min(21, Math.Max(0, optionCount));
                helper.levelSpriteBackground.MarkAsChanged();
            }

            if (helper.levelSpriteIndex == null)
                return;

            if (helper.levelSpriteBackground == null || optionCount <= 0) {
                helper.levelSpriteIndex.enabled = false;
                return;
            }

            int clampedIndex = Math.Min(Math.Max(0, selectedIndex), optionCount - 1);
            helper.levelSpriteIndex.enabled = true;
            helper.levelSpriteIndex.transform.localPosition = helper.levelSpriteBackground.transform.localPosition + new Vector3(8f * clampedIndex - helper.levelSpriteBackground.width * 0.5f, 0f, 0f);
            helper.levelSpriteIndex.MarkAsChanged();
        }

        //==============================================================================
        static void CreatePilotModeReadoutWidgets(UILabel labelTemplate) {
            if (labelTemplate != null) {
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

            UpdateLiverySlotDecorations(loadoutView.liveryHelperRoot, LiverySetsDB.PartKey(null, null), showMarkers && mechId >= 0, mechId, pilotId, showHierarchyDecorations && IsLiverySlotVisible(loadoutView.liveryHelperRoot), 0, rootHasChildren, false, loadoutView.liverySlotHeight);

            if (socketHelpers != null) {
                foreach (var kv in socketHelpers) {
                    CIHelperLoadoutLiverySlot slot = kv.Value;
                    bool slotVisible = IsLiverySlotVisible(slot);
                    bool hasSubparts = DoesSocketHaveVisibleLiverySubparts(unit, kv.Key);
                    bool hasAssignedSubpartLivery = hasSubparts && DoesSocketHaveVisibleAssignedLiverySubpart(unit, kv.Key, mechId);
                    UpdateLiverySlotDecorations(slot, LiverySetsDB.PartKey(kv.Key, null), showMarkers && slotVisible && mechId >= 0, mechId, pilotId, showHierarchyDecorations && slotVisible, 1, hasSubparts, hasAssignedSubpartLivery, loadoutView.liverySlotHeight);
                }
            }

            if (hardpointHelpers != null) {
                string socket = CIViewBaseLoadout.selectedUnitSocket;
                foreach (var kv in hardpointHelpers) {
                    CIHelperLoadoutLiverySlot slot = kv.Value;
                    bool slotVisible = !string.IsNullOrEmpty(socket) && IsLiverySlotVisible(slot);
                    UpdateLiverySlotDecorations(slot, LiverySetsDB.PartKey(socket, kv.Key), showMarkers && slotVisible && mechId >= 0, mechId, pilotId, showHierarchyDecorations && slotVisible, 2, false, false, loadoutView.liverySlotHeight);
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
        static bool DoesSocketHaveVisibleAssignedLiverySubpart(PersistentEntity unit, string socket, int mechId) {
            if (unit == null || mechId < 0 || string.IsNullOrEmpty(socket))
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
                if (hardpointData == null || hardpointData.isInternal || string.IsNullOrEmpty(hardpointData.visualGroup))
                    continue;

                string liveryKey = LiverySetsDB.GetCurrentLiveryKeyForSlot(mechId, socket, hardpoint);
                if (!string.IsNullOrEmpty(liveryKey) &&
                    !string.Equals(liveryKey, LiverySetsDB.PILOT_TRANSPARENT, StringComparison.Ordinal) &&
                    !string.Equals(liveryKey, LiverySetsDB.PILOT_BASE_PULSE_LIVERY, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        //==============================================================================
        static void UpdateLiverySlotDecorations(CIHelperLoadoutLiverySlot slot, string partKey, bool layerMarkerVisible, int mechId, string pilotId, bool hierarchyVisible, int hierarchyDepth, bool hasSubparts, bool hasAssignedSubpartLivery, int slotHeight) {
            UpdateLiverySlotLayerMarker(slot, partKey, layerMarkerVisible, mechId, pilotId);
            bool childMarkerPilotColored = hierarchyDepth > 0 && layerMarkerVisible && hasSubparts && LiverySetsDB.IsPilotLiveryAssignedToDescendantPart(mechId, partKey, pilotId);
            bool childMarkerAssignedColored = hierarchyDepth > 0 && hasSubparts && hasAssignedSubpartLivery;
            UpdateLiverySlotChildMarker(slot, hierarchyVisible && hasSubparts, childMarkerPilotColored, childMarkerAssignedColored);
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
        static void UpdateLiverySlotChildMarker(CIHelperLoadoutLiverySlot slot, bool visible, bool pilotColored, bool assignedColored) {
            UISprite marker = GetOrCreateLiverySlotSprite(slot, liverySlotChildMarkerName, visible, template: slot?.spriteIcon);
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
            marker.color = pilotColored ? pilotLiverySlotMarkerColor : (assignedColored ? liverySlotChildMarkerAssignedColor : liverySlotChildMarkerColor);
            marker.alpha = Mathf.Clamp01(slot.targetAlpha);
            marker.transform.localScale = Vector3.one;
            marker.transform.localPosition = new Vector3(bottomLeft.x + 6f, (bottomLeft.y + topRight.y) * 0.5f, 0f);
            marker.MarkAsChanged();
        }

        //==============================================================================
        static void UpdateLiverySlotHierarchyLine(CIHelperLoadoutLiverySlot slot, bool visible, int hierarchyDepth, int slotHeight) {
            HideLiverySlotSprite(slot, liverySlotHierarchyLineNamePrefix + "1");

            bool lineVisible = visible && hierarchyDepth >= 2;
            UISprite line = GetOrCreateLiverySlotSprite(slot, liverySlotHierarchyLineNamePrefix + "0", lineVisible, template: slot?.spriteIcon);
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
            if (pilotModePilotIds.Count == 0) {
                pilotModePilotId = null;
                pilotModePilotMechId = mechId;
                return;
            }

            bool mechChanged = mechId >= 0 && mechId != pilotModePilotMechId;
            if (!mechChanged && !string.IsNullOrEmpty(pilotModePilotId) && pilotModePilotIds.Contains(pilotModePilotId))
                return;

            string assignedPilotId = LiverySetsDB.ResolvePilotIdForMech(mechId);
            if (!string.IsNullOrEmpty(assignedPilotId) && pilotModePilotIds.Contains(assignedPilotId)) {
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

            foreach (PersistentEntity pilot in pilots) {
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
            if (pilotModePilotIds.Count == 0) {
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
            //CIViewOverworldLog.AddMessage($"Pilot livery target: {GetPilotDisplayName(pilotModePilotId)} [sp={pilotSymbolSpriteName}]");
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
        public static string GetPilotModeBasePulseLiveryKey() {
            if (!IsPilotModeActive() || pilotModeMechBaseVisible)
                return LiverySetsDB.PILOT_TRANSPARENT;

            DataContainerEquipmentLivery livery = EnsurePilotModeBasePulseLivery();
            if (livery == null)
                return LiverySetsDB.PILOT_TRANSPARENT;

            UpdatePilotModeBasePulseLiveryData(livery);
            return LiverySetsDB.PILOT_BASE_PULSE_LIVERY;
        }

        //==============================================================================
        public static void UpdatePilotModeBasePulseLivery() {
            if (!IsPilotModeActive() || pilotModeMechBaseVisible)
                return;

            DataContainerEquipmentLivery livery = EnsurePilotModeBasePulseLivery();
            if (livery == null)
                return;

            UpdatePilotModeBasePulseLiveryData(livery);

            int mechId = CIViewBaseLoadout.selectedUnitID;
            if (mechId >= 0)
                LiverySetsDB.ApplyLiverySetToMech(mechId, false, true, GetPilotModePilotId(mechId));
        }

        //==============================================================================
        static DataContainerEquipmentLivery EnsurePilotModeBasePulseLivery() {
            var liveryDict = DataMultiLinkerEquipmentLivery.data;
            if (liveryDict == null)
                return null;

            if (liveryDict.TryGetValue(LiverySetsDB.PILOT_BASE_PULSE_LIVERY, out DataContainerEquipmentLivery livery) && livery != null)
                return livery;

            livery = new DataContainerEquipmentLivery {
                key = LiverySetsDB.PILOT_BASE_PULSE_LIVERY,
                hidden = true,
                priority = int.MaxValue,
                textName = "LiveryGUI Pilot Base Pulse",
                source = "LiveryGUI",
                rating = 0,
                pattern = null,
                contentSource = null,
                materialPrimary = new Vector4(0f, 0.5f, 0.8f, 0f),
                materialSecondary = new Vector4(0f, 0.5f, 0.8f, 0f),
                materialTertiary = new Vector4(0f, 0.5f, 0.8f, 0f),
                effect = Vector4.zero,
                contentParameters = Vector4.zero
            };

            liveryDict[LiverySetsDB.PILOT_BASE_PULSE_LIVERY] = livery;
            DataMultiLinkerEquipmentLivery.OnAfterDeserialization();
            return livery;
        }

        //==============================================================================
        static void UpdatePilotModeBasePulseLiveryData(DataContainerEquipmentLivery livery) {
            float primary   = GetPilotModeBasePulseIntensity(0f);
            float secondary = GetPilotModeBasePulseIntensity(0.05f);
            float tertiary  = GetPilotModeBasePulseIntensity(0.08f);
            Color tertiary_color = GetPilotModeBasePulseHueColor(0f);

            livery.colorPrimary   = new Color(primary, primary, primary, 0f);
            livery.colorSecondary = new Color(secondary, secondary, secondary, 0f) * 0.8f;
            livery.colorTertiary  = tertiary_color * tertiary * 0.8f;
        }

        //==============================================================================
        static float GetPilotModeBasePulseIntensity(float lagInPeriods) {
            float phase = (Time.unscaledTime / pilotModeBasePulsePeriod - lagInPeriods) * Mathf.PI * 2f;
            float normalized = (Mathf.Sin(phase) + 1f) * 0.5f;
            return Mathf.Lerp(pilotModeBasePulseMin, pilotModeBasePulseMax, normalized);
        }

        //==============================================================================
        static Color GetPilotModeBasePulseHueColor(float lagInPeriods) {
            float hue = Mathf.Repeat((Time.unscaledTime / (pilotModeBasePulsePeriod * 1.87f)) - lagInPeriods, 1f);
            Color color = Color.HSVToRGB(hue, 0.4f, 1f);
            color.a = 0f;
            return color;
        }

        //==============================================================================
        static void UpdatePilotModeReadout() {
            bool pilotModeActive = IsPilotModeActive();
            string pilotName = string.IsNullOrEmpty(pilotModePilotId) ? "[none]" : GetPilotDisplayName(pilotModePilotId);
            if (pilotModeNameLabel != null) {
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
            if (!IsPBLiveryViewActive() || pilotModePortraitGO == null) {
                DeactivatePilotModeLivePortrait();
                return;
            }

            PersistentEntity pilot = IDUtility.GetPersistentEntity(pilotModePilotId);
            if (pilot == null || !pilot.isPilotTag || !pilot.hasPilotAppearance) {
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

            foreach (PilotViewBase.PilotModel model in pilotView.models) {
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
            foreach (KeyValuePair<Animator, AnimatorUpdateMode> item in pilotModeLivePortraitAnimatorUpdateModes) {
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
            if (pilotView != null) {
                pilotView.OutputToRenderTexture();
                pilotView.SetActive(false);
            }

            pilotModeLivePortraitActive = false;
            pilotModeLivePortraitPilotIdLast = null;
        }

        //==============================================================================
        public static void UpdatePilotModePortraitVisibility() {
            bool pilotModeActive = IsPilotModeActive();
            if (!pilotModeActive) {
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
            // Instead, we'll have the on-input field do nothing itself, and let the save button use this
            // field's value as the key/name for the saved livery or a newly created copy.

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
            foreach (KeyValuePair<string, SliderConfig> item in sliderConfigs) {
                string key = item.Key;
                SliderConfig cfg = item.Value;
                if (!sliderHelpers.TryGetValue(key, out CIHelperSetting helper) || helper == null)
                    continue;

                if (TryGetSettingPosition(cfg, x, y, out Vector3 position))
                    helper.gameObject.transform.localPosition = position;
            }
            UpdateSettingTooltipButtonPositioning(x, y);

            float pilotCyclerBottomY = -(Screen.height * pixelSizeAdj) + Positions.pxGapAbovePaneGO + 66f;
            Vector3 pilotCyclerPos = new Vector3(Positions.pilotPanelLeft, pilotCyclerBottomY, 0f);
            pilotModePortraitGO.transform.localPosition = pilotCyclerPos + new Vector3(Positions.pilotPortraitDim/2f, -12f);
            pilotModePrevButton.gameObject.transform.localPosition = pilotCyclerPos + Positions.pilotPrevButtonOffset;
            pilotModeNextButton.gameObject.transform.localPosition = pilotCyclerPos + Positions.pilotNextButtonOffset;
            pilotModeNameLabel.gameObject.transform.localPosition  = pilotCyclerPos + Positions.pilotNameOffset;

            liveryNameInput.gameObject.transform.localPosition = paneGO.activeSelf ? Positions.liveryName : Positions.liveryName_hiddenOffscreen;
        }

        //==============================================================================
        static void UpdateSettingTooltipButtonPositioning(float[] x, float[] y) {
            if (settingTooltipButtons == null)
                return;

            foreach (KeyValuePair<string, SliderConfig> item in sliderConfigs) {
                string key = item.Key;
                SliderConfig cfg = item.Value;
                if (!settingTooltipButtons.TryGetValue(key, out CIButton button) || button == null)
                    continue;

                if (TryGetSettingPosition(cfg, x, y, out Vector3 position))
                    button.gameObject.transform.localPosition = position + GetSettingTooltipButtonOffset(cfg);
            }
        }

        //==============================================================================
        static bool TryGetSettingPosition(SliderConfig cfg, float[] x, float[] y, out Vector3 position) {
            position = Vector3.zero;
            if (cfg == null || x == null || y == null || cfg.column < 0 || cfg.column >= x.Length || cfg.row < 0 || cfg.row >= y.Length)
                return false;

            position = new Vector3(x[cfg.column], y[cfg.row], 0f);
            return true;
        }

        //==============================================================================
        static Vector3 GetSettingTooltipButtonOffset(SliderConfig cfg) {
            return IsRightColumnSetting(cfg) ? new Vector3(-29f, -9f, 0f) : new Vector3(327f, -9f, 0f);
        }

        //==============================================================================
        static void OnClickSecondary(CIHelperSetting helper) {
            sliderRightClickHandler?.CaptureRightClickFor(helper.sliderBar);
        }

        //==============================================================================
        static void CreateCopyForEditedName() {
            if (!HasSelectedLivery())
                return;

            string newLiveryKey = CreateCopyOfSelectedLivery();
            if (newLiveryKey == liveryNameInput.value) {
                SelectLivery(newLiveryKey);
            }
            RefreshSphereAndMechPreviews();
        }

        //==============================================================================
        static void OnResetLiveryClicked() {
            DataContainerEquipmentLivery origLivery;
            string activeLiveryKeyForLookup;
            if (string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitLivery) ||
                !LiverySnapshotDB.originalLiveries.ContainsKey(CIViewBaseLoadout.selectedUnitLivery)) {
                Debug.Log($"[LiveryGUI] OnResetLiveryClicked(): livery has null/empty/unknown key={CIViewBaseLoadout.selectedUnitLivery ?? "<null>"}");
                return;
                //liveryNameInput.Set("null");
                //origLivery = LiverySnapshotDB.defaultLivery.onDiskDat;
                //activeLiveryKeyForLookup = "default";
            } else {
                liveryNameInput.Set(CIViewBaseLoadout.selectedUnitLivery);
                origLivery = LiverySnapshotDB.originalLiveries[CIViewBaseLoadout.selectedUnitLivery].onDiskDat;
                activeLiveryKeyForLookup = CIViewBaseLoadout.selectedUnitLivery;
            }

            if (!DataMultiLinkerEquipmentLivery.data.ContainsKey(activeLiveryKeyForLookup)) {
                Debug.LogWarning($"[LiveryGUI] BUG: Couldn't find DataMultiLinkerEquipmentLivery.data key={activeLiveryKeyForLookup}");
            }
            else {
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
                //CIViewOverworldLog.AddMessage($"Reset livery to last saved version. [sp=s_icon_l32_retreat]");
            }

            RefreshSphereAndMechPreviews();
        }

        //==============================================================================
        static string CreateCopyOfSelectedLivery() {
            if (CIViewBaseLoadout.ins == null) return null;

            var liveriesDict = DataMultiLinkerEquipmentLivery.data;
            string newKey = liveryNameInput.value; // (from the text-input field)
            if (string.IsNullOrEmpty(newKey))
                return null;

            if (liveriesDict.ContainsKey(newKey)) {
                Debug.LogWarning($"[LiveryGUI] USAGE: Refusing to create livery {newKey}: That key already exists.");
                CIViewOverworldLog.AddMessage($"No. A livery with that Name already exists. [sp=s_icon_l32_cancel]");
                return null;
            }

            DataContainerEquipmentLivery newCopy;

            string origKey = CIViewBaseLoadout.selectedUnitLivery;
            if (string.IsNullOrEmpty(origKey) || !liveriesDict.TryGetValue(origKey, out var original)) {
                newCopy = new DataContainerEquipmentLivery();
            }
            else {
                newCopy = LiverySnapshotDB.DeepCopyLiveryDat(original);
            }

            newCopy.key = newKey;
            newCopy.textName = newKey;
            newCopy.source = $"LiveryGUI";

            liveriesDict[newKey] = newCopy;
            DataMultiLinkerEquipmentLivery.OnAfterDeserialization(); // (triggers rebuilding its .dataSorted)

            DataContainerEquipmentLivery snapshotSource = newCopy;
            if (!string.IsNullOrEmpty(origKey) && LiverySnapshotDB.originalLiveries.TryGetValue(origKey, out var lastSavedLivery) && lastSavedLivery != null)
                snapshotSource = lastSavedLivery.onDiskDat;
            LiverySnapshotDB.AddLiveryDataSnapshot(newKey, snapshotSource, true);

            Debug.Log($"[LiveryGUI] INFO: copied livery {origKey} to {newKey}");
            //CIViewOverworldLog.AddMessage($"Created a new copy of that livery. [sp=s_icon_l32_lc_grid_plus]");
            return newKey;
        }

        //==============================================================================
        static void SelectLivery(string liveryKey) {
            if (string.IsNullOrEmpty(liveryKey)) {
                if(!string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitLivery))
                    liveryKey = CIViewBaseLoadout.selectedUnitLivery; // This "re-selects" it, which per the below call to OnLiveryAttachAttempt, toggles/de-selects it. (Passing a null key gets ignored.)
                // else: selected livery is already null
            }
            else {
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
        static bool IsInternalDisplayLiveryKey(string key) {
            return string.Equals(key, LiverySetsDB.PILOT_BASE_PULSE_LIVERY, StringComparison.Ordinal);
        }

        //==============================================================================
        static bool HasSelectedLivery() {
            string key = CIViewBaseLoadout.selectedUnitLivery;
            return !string.IsNullOrEmpty(key) &&
                DataMultiLinkerEquipmentLivery.data != null &&
                DataMultiLinkerEquipmentLivery.data.ContainsKey(key);
        }

        //==============================================================================
        static DataContainerEquipmentLivery GetSelectedLivery() {
            string key = CIViewBaseLoadout.selectedUnitLivery;
            if (key.IsNullOrEmpty() || IsInternalDisplayLiveryKey(key))
                return null;

            return DataMultiLinker<DataContainerEquipmentLivery>.GetEntry(key, false);
        }

        //==============================================================================
        static bool SelectedLiveryIsFavorited() {
            if (CIViewBaseLoadout.selectedUnitLivery.IsNullOrEmpty() || IsInternalDisplayLiveryKey(CIViewBaseLoadout.selectedUnitLivery))
                return false;
            return (CIViewBaseLoadout.liveryKeysFavorite.Contains(CIViewBaseLoadout.selectedUnitLivery));
        }

        //==============================================================================
        static void ResetLiveryGUIWidgetsToMatchLivery(DataContainerEquipmentLivery livery) {
            LiverySetsDB.OnMaybeMechSelectionChanged();

            //Dev.Log($"[LiveryGUI] DEBUG-SPAM: ResetLiveryGUIWidgetsToMatchLivery CIViewBaseLoadout.selectedUnitLivery={CIViewBaseLoadout.selectedUnitLivery}");
            if (string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitLivery) || IsInternalDisplayLiveryKey(CIViewBaseLoadout.selectedUnitLivery)) {
                liveryNameInput.Set("null");
            }
            else {
                liveryNameInput.Set(CIViewBaseLoadout.selectedUnitLivery);
            }
            liveryNameInput.UpdateLabel();

            if (livery != null) {
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
                UpdateContentWLevelWidget();
            }

            UpdateButtonColors();
        }

        //==============================================================================
        static void UpdateButtonColors() {
            bool hasSelectedLivery = HasSelectedLivery();
            bool liveryDataIsModified = hasSelectedLivery && LiverySnapshotDB.IsCurrentLiveryModified();
            bool liveryNameIsModified = hasSelectedLivery && (CIViewBaseLoadout.selectedUnitLivery != liveryNameInput.value);
            bool liveryIsModified = (liveryDataIsModified || liveryNameIsModified);
            bool liveryNameCanBeSaved = hasSelectedLivery && IsLiveryNameSaveable(liveryNameInput.value);
            bool liveryNameIsBlocked = hasSelectedLivery && IsLiveryNameReservedByNonOwnedLivery(liveryNameInput.value);

            var saveIcon = saveLiveryButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            var saveFillIdle = saveLiveryButton.gameObject.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
            if (saveIcon != null && saveFillIdle != null) {
                if (liveryIsModified && liveryNameCanBeSaved) {
                    saveIcon.color = activeButtonFGColor;
                    saveFillIdle.color = activeButtonBGColor;
                }
                else if (liveryIsModified && liveryNameIsBlocked) {
                    saveIcon.color = activeRedButtonFGColor;
                    saveFillIdle.color = activeRedButtonBGColor;
                }
                else {
                    saveIcon.color = grayedOutButtonFGColor;
                    saveFillIdle.color = grayedOutButtonBGColor;
                }
            }

            var resetIcon = resetLiveryButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            var resetFillIdle = resetLiveryButton.gameObject.transform.Find("Sprite_Fill_Idle")?.GetComponent<UISprite>();
            if (resetIcon != null && resetFillIdle != null) {
                if (liveryIsModified) {
                    resetIcon.color = activeRedButtonFGColor;
                    resetFillIdle.color = activeRedButtonBGColor;
                }
                else {
                    resetIcon.color = grayedOutButtonFGColor;
                    resetFillIdle.color = grayedOutButtonBGColor;
                }
            }

            var favoriteIcon = favoriteLiveryButton.gameObject.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
            if (favoriteIcon != null)
                favoriteIcon.spriteName = (SelectedLiveryIsFavorited()) ? spriteNameStarFilled : spriteNameStarOutline;
        }

        //==============================================================================
        static bool IsLiveryNameSaveable(string key) {
            if (string.IsNullOrEmpty(key))
                return false;

            if (DataMultiLinkerEquipmentLivery.data == null || !DataMultiLinkerEquipmentLivery.data.ContainsKey(key))
                return true;

            return LiverySnapshotDB.originalLiveries.ContainsKey(key) && LiverySnapshotDB.originalLiveries[key].ownedByLiveryGUI;
        }

        //==============================================================================
        static bool IsLiveryNameReservedByNonOwnedLivery(string key) {
            if (string.IsNullOrEmpty(key))
                return false;

            if (DataMultiLinkerEquipmentLivery.data == null || !DataMultiLinkerEquipmentLivery.data.ContainsKey(key))
                return false;

            return !LiverySnapshotDB.originalLiveries.ContainsKey(key) || !LiverySnapshotDB.originalLiveries[key].ownedByLiveryGUI;
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
            if (pilotModePrevButton != null) {
                pilotModePrevButton.gameObject.SetActive(pilotModeActive);
                pilotModePrevButton.available = canCyclePilots;
            }
            if (pilotModeNextButton != null) {
                pilotModeNextButton.gameObject.SetActive(pilotModeActive);
                pilotModeNextButton.available = canCyclePilots;
            }
            if (pilotModeBaseToggleButton != null) {
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
