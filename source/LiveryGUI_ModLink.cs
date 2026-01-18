using Entitas;
using HarmonyLib;
using PhantomBrigade;
using PhantomBrigade.Data;
using PhantomBrigade.Mods;
using System;
using System.Collections.Generic;
using System.IO;
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
//  - Minimum button readability: add tooltips (or convert to text-buttons).
//  - Display the selected livery's name -- working except initially when page first opened is "-".
//  - save-button... tie in with text-input field. maybe it just resets the field back to the thing's name. o/w need to clone-and-rename as part of save. (shrug)

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

namespace LiveryGUI_ModExtensions
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

    // marker for whether we've initialized our LiveryGUI sliders into a given root-GUI-thingy yet
    class LiveryAdvancedPaneMarker : MonoBehaviour {}

    public class SliderConfig {
        public string name;
        public string label;
        public Color fillColor;
        public float min;
        public float max;
        public SliderConfig(string nameIn, string labelIn, Color fillColorIn, float minIn = 0f, float maxIn = 1f) {
            name = nameIn;
            label = labelIn;
            fillColor = fillColorIn;
            min = minIn;
            max = maxIn;
        }
    }


    //+================================================================================================+
    //||                                                                                              ||
    //+================================================================================================+
    public class Patches
    {
        static GameObject paneGO = null;
        static Dictionary<string, CIHelperSetting> sliderHelpers = null;
        static Dictionary<string, SliderConfig> sliderConfigs = null;
        static CIButton toggleLiveryGUIButton;
        static CIButton cloneLiveryButton;
        static CIButton saveLiveryButton;
        static UIInput liveryNameInput;

        static readonly string saveDirMetadataContent = @"priority: 0
colorHue: 0.324
id: UserSaveData_Directory
ver: 0.0
url: 
includesConfigOverrides: false
includesConfigEdits: false
includesConfigTrees: false
includesLibraries: false
includesTextures: false
includesLocalizationEdits: false
includesLocalizations: false
includesAssetBundles: false
gameVersion2Compatible: true
gameVersionMin: 1.0
gameVersionMax: 
name: UserSaveData Directory
desc: >-
  This 'mod' changes nothing. It's just a stub to suppress a warning message about mods saving data to a 'local mods' directory that the game assumes is itself a mod. (Feel free to deactivate this mod. That won't defeat its purpose.)
";

        static string GetUserSaveDataDir()
        {
            //   !!! BYG states:
            //   !!!   "Do not modify, create or edit files outside of AppData/Local/PhantomBrigade/Mods"
            //
            // Note: That is the local-mods directory, so maybe be careful about dumping user-data into the mod's own directory; e.g., maybe create a userSavedData under MyMod dir?
            // Any dir created directly in Mods, PB might try to load as a mod. (Presumably it quickly gives up after it fails to find a metadata.yaml, but still.)
            // But I don't want to have the Unity-app-PB-SDK "Export to user (deploy locally)" delete my saved data.
            // So maybe modders could use a structure like AppData/Local/PhantomBrigade/Mods/UserSavedData/MyModAbc123? Hm.
            //
            // DO NOT USE: Application.persistentDataPath                          // DO NOT USE. under AppData/LocalLow   (eg /Brace Yourself Games/Phantom Brigade/Mods)
            // DO NOT USE: DataPathHelper.GetModsFolder(ModFolderType.Application) // DO NOT USE. under steamapps/common/Phantom Brigade/Mods
            // DO NOT USE: DataPathHelper.GetModsFolder(ModFolderType.Workshop)    // DO NOT USE. under steamapps/workshop/content/553540

            //Debug.Log($"DataPathHelper.GetModsFolder(ModFolderType.User): {DataPathHelper.GetModsFolder(ModFolderType.User)}"); // under AppData/Local/PhantomBrigade/Mods

            string localModsDir = DataPathHelper.GetModsFolder(ModFolderType.User); // .../AppData/Local/PhantomBrigade/Mods/
            return Path.Combine(localModsDir, "UserSavedData");
        }

        static string GetLiveryGUISaveDir()
        {
            return Path.Combine(GetUserSaveDataDir(), "LiveryGUI");
        }


        // Load all liveries the user previously saved to file.
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

                string liveryGUISaveDir = GetLiveryGUISaveDir();

                if (!Directory.Exists(liveryGUISaveDir))
                {
                    Debug.Log($"[LiveryGUI] User-saved liveries directory doesn't exist yet (i.e., user hasn't saved any liveries yet; {liveryGUISaveDir}).");
                    return;
                }

                var files = Directory.GetFiles(liveryGUISaveDir, "*.yaml");
                if (files.Length == 0)
                {
                    Debug.Log($"[LiveryGUI] No user-saved livery files found (i.e., user hasn't saved any liveries yet; no *.yaml files in {liveryGUISaveDir}).");
                    return;
                }

                var liveryDict = DataMultiLinkerEquipmentLivery.data;
                int loadedCount = 0;

                foreach (string path in files)
                {
                    try
                    {
                        var livery = UtilitiesYAML.LoadDataFromFile<DataContainerEquipmentLivery>(path, false, false);
                        if (livery == null)
                        {
                            Debug.LogWarning($"[LiveryGUI] Failed to load livery from {path}");
                            continue;
                        }

                        string key = Path.GetFileNameWithoutExtension(path);
                        livery.key = key;

                        if (string.IsNullOrEmpty(livery.textName))
                            livery.textName = key;

                        // Avoid collisions with base-game or other mods
                        if (liveryDict.ContainsKey(livery.key))
                        {
                            Debug.LogWarning($"[LiveryGUI] Livery key already exists, skipping: {livery.key}");
                            continue;
                        }

                        liveryDict[livery.key] = livery;
                        loadedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[LiveryGUI] Exception loading livery file '{path}':\n{ex}");
                    }
                }

                Debug.Log($"[LiveryGUI] Loaded {loadedCount} user-saved liveries.");
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
                    sliderHelpers = new Dictionary<string, CIHelperSetting>();
                    sliderConfigs = new Dictionary<string, SliderConfig>() {
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

                    foreach (var item in sliderConfigs) {
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
                        helper.sliderBar.spriteFill.color = cfg.fillColor;

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
                    if (toggleIcon      != null) toggleIcon.color      = new Color(0.9f, 0.9f, 0.9f, 0.8f);
                    if (toggleFrame     != null) toggleFrame.color     = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                    if (toggleFillIdle  != null) toggleFillIdle.color  = new Color(0.0f, 0.0f, 0.0f, 0.7f);
                    if (toggleFillHover != null) toggleFillHover.color = new Color(0.7f, 0.7f, 0.7f, 0.4f);

                    toggleLiveryGUIButton = toggleLiveryGUIButtonGO.GetComponent<CIButton>();
                    toggleLiveryGUIButton.callbackOnClick = new UICallback(() =>
                    {
                        paneGO.SetActive(!paneGO.activeSelf);
                    });

                    toggleLiveryGUIButtonGO.SetActive(true);

                    ////////////////////////////////////////////////////////////////////////////////
                    // Livery GUI buttons: 'clone livery', 'save livery to disk'
                    Vector3 posStep = new Vector3(80f, 0f, 0f);

                    GameObject cloneLiveryButtonGO = GameObject.Instantiate(toggleLiveryGUIButtonGO, paneGO.transform, false);
                    cloneLiveryButtonGO.name = "cloneLiveryButtonGO";
                    cloneLiveryButtonGO.transform.localPosition += posStep;
                    var cloneIcon = cloneLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                    if (cloneIcon != null) cloneIcon.color = new Color(0.5f, 0.8f, 0.6f, 0.8f);

                    GameObject saveLiveryButtonGO = GameObject.Instantiate(cloneLiveryButtonGO, paneGO.transform, false);
                    saveLiveryButtonGO.name = "saveLiveryButtonGO";
                    saveLiveryButtonGO.transform.localPosition += posStep;
                    var saveIcon = saveLiveryButtonGO.transform.Find("Sprite_Icon")?.GetComponent<UISprite>();
                    if (saveIcon != null) saveIcon.color = new Color(0.5f, 0.6f, 0.8f, 0.8f);

                    cloneLiveryButton = cloneLiveryButtonGO.GetComponent<CIButton>();
                    cloneLiveryButton.callbackOnClick = new UICallback(() =>
                    {
                        string newLiveryKey = CloneSelectedLivery();
                        SelectLivery(newLiveryKey);
                        RefreshSphereAndMechPreviews();
                    });

                    saveLiveryButton = saveLiveryButtonGO.GetComponent<CIButton>();
                    saveLiveryButton.callbackOnClick = new UICallback(() =>
                    {
                        SaveLiveryToFile(GetSelectedLivery());
                    });

                    ////////////////////////////////////////////////////////////////////////////////
                    // Livery GUI text-input field: for seeing & renaming the livery-key/file-name
                    GameObject liveryNameInputGO = GameObject.Instantiate(CIViewBaseLoadout.ins.headerInputUnitName.gameObject, paneGO.transform, false);
                    liveryNameInputGO.name = "liveryNameInputGO";
                    liveryNameInputGO.transform.localPosition = saveLiveryButtonGO.transform.localPosition + posStep;
                    liveryNameInputGO.transform.localScale = Vector3.one;

                    liveryNameInput = liveryNameInputGO.GetComponent<UIInput>();
                    liveryNameInput.onChange = new List<EventDelegate>() { new EventDelegate(new EventDelegate.Callback(OnLiveryNameInput)) };
                    liveryNameInput.onReturnKey = UIInput.OnReturnKey.Submit;
                    liveryNameInput.onSubmit = new List<EventDelegate> { new EventDelegate(new EventDelegate.Callback(OnLiveryNameInput)) };
                    liveryNameInput.label.text = "Livery Name";
                    liveryNameInput.label.ProcessText();
                    liveryNameInput.characterLimit += 10;

                    liveryNameInputGO.SetActive(true);

                    ////////////////////////////////////////////////////////////////////////////////
                    // Livery GUI initial visibility
                    paneGO.SetActive(false);
                }//if(doOnce)

                UpdateLiveryListTooltips();
                UpdateWidgetPositioning(__instance);
                ResetSlidersAndTextToMatchLivery(GetSelectedLivery());
            }//Postfix()

            static void OnLiveryNameInput()
            {
                // It would be possible to do something like (dangerous?) rename the livery, including changing its dictionary key.
                // This could leave dangling references.
                // Onstead, we'll have the on-input field do nothing itself, and let the 'create clone' button use this field's
                // value as the new key/name for the newly cloned livery.

                Contexts.sharedInstance.game.isInputBlocked = true; // (we've handled/consumed the input-event(s) this frame)
                return;
            }

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
                sliderHelpers["PrimaryR"].gameObject.transform.localPosition   = new Vector3(x[0], y[0]);
                sliderHelpers["PrimaryG"].gameObject.transform.localPosition   = new Vector3(x[0], y[1]);
                sliderHelpers["PrimaryB"].gameObject.transform.localPosition   = new Vector3(x[0], y[2]);
                sliderHelpers["PrimaryA"].gameObject.transform.localPosition   = new Vector3(x[0], y[3]);
                sliderHelpers["SecondaryR"].gameObject.transform.localPosition = new Vector3(x[0], y[4]);
                sliderHelpers["SecondaryG"].gameObject.transform.localPosition = new Vector3(x[0], y[5]);
                sliderHelpers["SecondaryB"].gameObject.transform.localPosition = new Vector3(x[0], y[6]);
                sliderHelpers["SecondaryA"].gameObject.transform.localPosition = new Vector3(x[0], y[7]);
                sliderHelpers["TertiaryR"].gameObject.transform.localPosition  = new Vector3(x[0], y[8]);
                sliderHelpers["TertiaryG"].gameObject.transform.localPosition  = new Vector3(x[0], y[9]);
                sliderHelpers["TertiaryB"].gameObject.transform.localPosition  = new Vector3(x[0], y[10]);
                sliderHelpers["TertiaryA"].gameObject.transform.localPosition  = new Vector3(x[0], y[11]);
                sliderHelpers["PrimaryX"].gameObject.transform.localPosition   = new Vector3(x[1], y[0]);
                sliderHelpers["PrimaryY"].gameObject.transform.localPosition   = new Vector3(x[1], y[1]);
                sliderHelpers["PrimaryZ"].gameObject.transform.localPosition   = new Vector3(x[1], y[2]);
                sliderHelpers["PrimaryW"].gameObject.transform.localPosition   = new Vector3(x[1], y[3]);
                sliderHelpers["SecondaryX"].gameObject.transform.localPosition = new Vector3(x[1], y[4]);
                sliderHelpers["SecondaryY"].gameObject.transform.localPosition = new Vector3(x[1], y[5]);
                sliderHelpers["SecondaryZ"].gameObject.transform.localPosition = new Vector3(x[1], y[6]);
                sliderHelpers["SecondaryW"].gameObject.transform.localPosition = new Vector3(x[1], y[7]);
                sliderHelpers["TertiaryX"].gameObject.transform.localPosition  = new Vector3(x[1], y[8]);
                sliderHelpers["TertiaryY"].gameObject.transform.localPosition  = new Vector3(x[1], y[9]);
                sliderHelpers["TertiaryZ"].gameObject.transform.localPosition  = new Vector3(x[1], y[10]);
                sliderHelpers["TertiaryW"].gameObject.transform.localPosition  = new Vector3(x[1], y[11]);
                sliderHelpers["EffectX"].gameObject.transform.localPosition    = new Vector3(x[1], y[12]);
                sliderHelpers["EffectY"].gameObject.transform.localPosition    = new Vector3(x[1], y[13]);
                sliderHelpers["EffectZ"].gameObject.transform.localPosition    = new Vector3(x[1], y[14]);
                sliderHelpers["EffectW"].gameObject.transform.localPosition    = new Vector3(x[1], y[15]);
            }

            static void UpdateLiveryListTooltips()
            {
#if false // disabled due to buggy, sometimes get stuck up, sometimes stops working until you click. and it's kind of in the way of navigation and not super valuable while browsing
                var liveryOptionInstances = (Dictionary<string, CIHelperLoadoutLivery>) AccessTools.Field(typeof(CIViewBaseLoadout), "liveryOptionInstances")?.GetValue(CIViewBaseLoadout.ins);
                if (liveryOptionInstances == null)
                {
                    Debug.Log($"[LiveryGUI] null liveryOptionInstances. isNull={AccessTools.Field(typeof(CIViewBaseLoadout), "liveryOptionInstances")==null},{liveryOptionInstances==null}");
                    return;
                }

                foreach (var item in liveryOptionInstances) {
                    string key = item.Key;
                    CIHelperLoadoutLivery liveryHelper = item.Value;
                    if (key == null || liveryHelper == null)
                    {
                        Debug.Log($"[LiveryGUI] null key/val encountered: {key}, valIsNull={liveryHelper==null}");
                        continue;
                    }
                    liveryHelper.button.tooltipUsed = true;
                    liveryHelper.button.tooltipKey = null;
                    liveryHelper.button.AddTooltip(key, liveryHelper.name); // the 'title' is REALLY BIG AND CAPITALIZED, so probably leave title null and only use the text (?)
                }
#endif
            }
            
            static string CloneSelectedLivery()
            {
                if (CIViewBaseLoadout.ins == null) return null;

                var liveriesDict = DataMultiLinkerEquipmentLivery.data;
                string newKey = liveryNameInput.value; // (from the text-input field)
                if (string.IsNullOrEmpty(newKey))
                    return null;

                if (liveriesDict.ContainsKey(newKey))
                {
                    Debug.Log($"[LiveryGUI] Refusing to clone livery {newKey}: That key already exists. Selecting the existing livery-object.");
                    return newKey;
                }

                DataContainerEquipmentLivery newCopy;

                string currentKey = CIViewBaseLoadout.selectedUnitLivery;
                if (string.IsNullOrEmpty(currentKey) || !liveriesDict.TryGetValue(currentKey, out var original))
                {
                    newCopy = new DataContainerEquipmentLivery();
                }
                else
                {
                    // Deep copy
                    newCopy = JsonUtility.FromJson<DataContainerEquipmentLivery>(JsonUtility.ToJson(original)); // (well. i guess it works.)
                }

                newCopy.key = newKey;
                newCopy.textName = newKey;
                newCopy.source = $"LiveryGUI";

                liveriesDict[newKey] = newCopy;
                DataMultiLinkerEquipmentLivery.OnAfterDeserialization(); // (triggers rebuilding its .dataSorted)

                return newKey;
            }

            static void SaveLiveryToFile(DataContainerEquipmentLivery liveryDat)
            {
                string userSaveDataDir = GetUserSaveDataDir();
                string liveryGUISaveDir = GetLiveryGUISaveDir();
                Directory.CreateDirectory(liveryGUISaveDir);

                string saveDirMetadataFilePath = Path.Combine(userSaveDataDir, "metadata.yaml");
                if (!File.Exists(saveDirMetadataFilePath)) {
                    File.WriteAllText(saveDirMetadataFilePath, saveDirMetadataContent);
                    Debug.Log($"[LiveryGUI] Wrote new {saveDirMetadataFilePath} (to suppress future warnings about that directory not itself being a mod)");
                }

                string liveryFileName = liveryDat.textName + ".yaml";

                try
                {
                    UtilitiesYAML.SaveDataToFile(liveryGUISaveDir, liveryFileName, liveryDat, false);
                    Debug.Log($"[LiveryGUI] Saved {liveryFileName} to {liveryGUISaveDir}");
                } catch(Exception ex)
                {
                    Debug.Log($"[LiveryGUI] Failed to save {liveryFileName} to {liveryGUISaveDir}:\n{ex}");
                }
            }

            static void SelectLivery(string liveryKey) {
                if (!string.IsNullOrEmpty(liveryKey)) {
                    //Debug.Log($"!!! trying to clone livery... trying to set selected key={liveryKey} and command loadoutView.Redraw()...");
                    CIViewBaseLoadout.selectedUnitLivery = liveryKey;
                    object[] args = { liveryKey };
                    AccessTools.Method(typeof(CIViewBaseLoadout), "OnLiveryAttachAttempt").Invoke(CIViewBaseLoadout.ins, args); // (the built-in on-click handler when player clicks on a livery in the list)
                    CIViewBaseLoadout.ins.Redraw(CIViewBaseLoadout.selectedUnitSocket, CIViewBaseLoadout.selectedUnitHardpoint, false);
                }
            }

            static DataContainerEquipmentLivery GetSelectedLivery()
            {
                string key = CIViewBaseLoadout.selectedUnitLivery;
                if (string.IsNullOrEmpty(key))
                    return null;

                return DataMultiLinker<DataContainerEquipmentLivery>.GetEntry(key, false);
            }

            static void ResetSlidersAndTextToMatchLivery(DataContainerEquipmentLivery livery)
            {
                if (livery == null) return;
                sliderHelpers["PrimaryR"].sliderBar.valueRaw   = livery.colorPrimary.r;
                sliderHelpers["PrimaryG"].sliderBar.valueRaw   = livery.colorPrimary.g;
                sliderHelpers["PrimaryB"].sliderBar.valueRaw   = livery.colorPrimary.b;
                sliderHelpers["PrimaryA"].sliderBar.valueRaw   = livery.colorPrimary.a;
                sliderHelpers["SecondaryR"].sliderBar.valueRaw = livery.colorSecondary.r;
                sliderHelpers["SecondaryG"].sliderBar.valueRaw = livery.colorSecondary.g;
                sliderHelpers["SecondaryB"].sliderBar.valueRaw = livery.colorSecondary.b;
                sliderHelpers["SecondaryA"].sliderBar.valueRaw = livery.colorSecondary.a;
                sliderHelpers["TertiaryR"].sliderBar.valueRaw  = livery.colorTertiary.r;
                sliderHelpers["TertiaryG"].sliderBar.valueRaw  = livery.colorTertiary.g;
                sliderHelpers["TertiaryB"].sliderBar.valueRaw  = livery.colorTertiary.b;
                sliderHelpers["TertiaryA"].sliderBar.valueRaw  = livery.colorTertiary.a;
                sliderHelpers["PrimaryX"].sliderBar.valueRaw   = livery.materialPrimary.x;
                sliderHelpers["PrimaryY"].sliderBar.valueRaw   = livery.materialPrimary.y;
                sliderHelpers["PrimaryZ"].sliderBar.valueRaw   = livery.materialPrimary.z;
                sliderHelpers["PrimaryW"].sliderBar.valueRaw   = livery.materialPrimary.w;
                sliderHelpers["SecondaryX"].sliderBar.valueRaw = livery.materialSecondary.x;
                sliderHelpers["SecondaryY"].sliderBar.valueRaw = livery.materialSecondary.y;
                sliderHelpers["SecondaryZ"].sliderBar.valueRaw = livery.materialSecondary.z;
                sliderHelpers["SecondaryW"].sliderBar.valueRaw = livery.materialSecondary.w;
                sliderHelpers["TertiaryX"].sliderBar.valueRaw  = livery.materialTertiary.x;
                sliderHelpers["TertiaryY"].sliderBar.valueRaw  = livery.materialTertiary.y;
                sliderHelpers["TertiaryZ"].sliderBar.valueRaw  = livery.materialTertiary.z;
                sliderHelpers["TertiaryW"].sliderBar.valueRaw  = livery.materialTertiary.w;
                sliderHelpers["EffectX"].sliderBar.valueRaw    = livery.effect.x;
                sliderHelpers["EffectY"].sliderBar.valueRaw    = livery.effect.y;
                sliderHelpers["EffectZ"].sliderBar.valueRaw    = livery.effect.z;
                sliderHelpers["EffectW"].sliderBar.valueRaw    = livery.effect.w;

                if(string.IsNullOrEmpty(CIViewBaseLoadout.selectedUnitLivery))
                {
                    liveryNameInput.value = "null";
                } else
                {
                    liveryNameInput.value = CIViewBaseLoadout.selectedUnitLivery;
                }
            }

            static void UpdateLiveryFromSliders() {
                var livery = GetSelectedLivery();
                if (livery == null)
                    return;
                livery.colorPrimary.r      = sliderHelpers["PrimaryR"].sliderBar.valueRaw;
                livery.colorPrimary.g      = sliderHelpers["PrimaryG"].sliderBar.valueRaw;
                livery.colorPrimary.b      = sliderHelpers["PrimaryB"].sliderBar.valueRaw;
                livery.colorPrimary.a      = sliderHelpers["PrimaryA"].sliderBar.valueRaw;
                livery.colorSecondary.r    = sliderHelpers["SecondaryR"].sliderBar.valueRaw;
                livery.colorSecondary.g    = sliderHelpers["SecondaryG"].sliderBar.valueRaw;
                livery.colorSecondary.b    = sliderHelpers["SecondaryB"].sliderBar.valueRaw;
                livery.colorSecondary.a    = sliderHelpers["SecondaryA"].sliderBar.valueRaw;
                livery.colorTertiary.r     = sliderHelpers["TertiaryR"].sliderBar.valueRaw;
                livery.colorTertiary.g     = sliderHelpers["TertiaryG"].sliderBar.valueRaw;
                livery.colorTertiary.b     = sliderHelpers["TertiaryB"].sliderBar.valueRaw;
                livery.colorTertiary.a     = sliderHelpers["TertiaryA"].sliderBar.valueRaw;
                livery.materialPrimary.x   = sliderHelpers["PrimaryX"].sliderBar.valueRaw;
                livery.materialPrimary.y   = sliderHelpers["PrimaryY"].sliderBar.valueRaw;
                livery.materialPrimary.z   = sliderHelpers["PrimaryZ"].sliderBar.valueRaw;
                livery.materialPrimary.w   = sliderHelpers["PrimaryW"].sliderBar.valueRaw;
                livery.materialSecondary.x = sliderHelpers["SecondaryX"].sliderBar.valueRaw;
                livery.materialSecondary.y = sliderHelpers["SecondaryY"].sliderBar.valueRaw;
                livery.materialSecondary.z = sliderHelpers["SecondaryZ"].sliderBar.valueRaw;
                livery.materialSecondary.w = sliderHelpers["SecondaryW"].sliderBar.valueRaw;
                livery.materialTertiary.x  = sliderHelpers["TertiaryX"].sliderBar.valueRaw;
                livery.materialTertiary.y  = sliderHelpers["TertiaryY"].sliderBar.valueRaw;
                livery.materialTertiary.z  = sliderHelpers["TertiaryZ"].sliderBar.valueRaw;
                livery.materialTertiary.w  = sliderHelpers["TertiaryW"].sliderBar.valueRaw;
                livery.effect.x            = sliderHelpers["EffectX"].sliderBar.valueRaw;
                livery.effect.y            = sliderHelpers["EffectY"].sliderBar.valueRaw;
                livery.effect.z            = sliderHelpers["EffectZ"].sliderBar.valueRaw;
                livery.effect.w            = sliderHelpers["EffectW"].sliderBar.valueRaw;
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

                // trigger refresh of the whole mech, all parts. (only seems to redo ALL parts if the top-level of the mech is selected, so we do that temporarily)
                var origSocket = CIViewBaseLoadout.selectedUnitSocket;
                var origHardpoint = CIViewBaseLoadout.selectedUnitHardpoint;
                CIViewBaseLoadout.selectedUnitSocket = null;
                CIViewBaseLoadout.selectedUnitHardpoint = null;
                AccessTools.Method(typeof(CIViewBaseLoadout), "OnLiveryHoverEnd").Invoke(CIViewBaseLoadout.ins, null);
                CIViewBaseLoadout.selectedUnitSocket = origSocket;
                CIViewBaseLoadout.selectedUnitHardpoint = origHardpoint;
            }
        }//class RedrawLiveryGUI
    }//class Patches
}//namespace
