using PhantomBrigade.Data;
using System;
using System.Collections.Generic;
using System.IO;
using Debug = UnityEngine.Debug;

namespace LiveryGUIMod {

    public class LoadAndSave {
        const string SaveGameFolderName = "LiveryGUI";
        const string SaveGameUnitsFolderName = "units";
        const string SaveGamePilotsFolderName = "pilots";

        //==============================================================================
        public static void LoadUserSavedLiveries() {
            string liveryGUISaveDir = GetLiveryGUISaveDir();

            if (!Directory.Exists(liveryGUISaveDir)) {
                Debug.Log($"[LiveryGUI] INFO: User-saved liveries directory doesn't exist yet (i.e., user hasn't saved any liveries yet; {liveryGUISaveDir}).");
                return;
            }

            var files = Directory.GetFiles(liveryGUISaveDir, "*.yaml");
            if (files.Length == 0) {
                Debug.Log($"[LiveryGUI] INFO: No user-saved livery files found (i.e., user hasn't saved any liveries yet; no *.yaml files in {liveryGUISaveDir}).");
                return;
            }

            var liveryDict = DataMultiLinkerEquipmentLivery.data;
            int loadedCount = 0;
            int failCount = 0;

            foreach (string path in files) {
                try {
                    var livery = UtilitiesYAML.LoadDataFromFile<DataContainerEquipmentLivery>(path, false, false);
                    if (livery == null) {
                        Debug.LogWarning($"[LiveryGUI] WARNING: Failed to load livery from {path}");
                        failCount++;
                        continue;
                    }

                    string key = Path.GetFileNameWithoutExtension(path);
                    livery.key = key;

                    if (string.IsNullOrEmpty(livery.textName))
                        livery.textName = key;

                    // Avoid collisions with base-game or other mods
                    if (liveryDict.ContainsKey(livery.key)) {
                        Debug.LogWarning($"[LiveryGUI] WARNING: Livery key already exists, skipping: {livery.key}");
                        failCount++;
                        continue;
                    }

                    liveryDict[livery.key] = livery;
                    loadedCount++;

                    LiverySnapshotDB.AddLiveryDataSnapshot(key, livery, true);
                }
                catch (Exception ex) {
                    Debug.LogError($"[LiveryGUI] WARNING: Exception loading livery file '{path}':\n{ex}");
                    failCount++;
                }
            }

            Debug.Log($"[LiveryGUI] INFO: Loaded {loadedCount} user-saved liveries. Total liveries: {liveryDict.Count} (though some may be hidden from player-selection list)");
            if (failCount > 0) {
                Localization.AddOverworldMessage("livery_gui_message_load_failed", failCount);
            }
        }

        //==============================================================================
        public static void SaveLiveryToFile(string key, DataContainerEquipmentLivery liveryDat) {
            if (LiverySnapshotDB.originalLiveries.ContainsKey(key) && !LiverySnapshotDB.originalLiveries[key].ownedByLiveryGUI) {
                Debug.LogWarning($"[LiveryGUI] USAGE: Refusing to save to this livery key/name because LiveryGUI does not own that livery. Save it with a new key/name first. key={key}");
                Localization.AddOverworldMessage("livery_gui_message_save_blocked_name");
                return;
            }

            string liveryGUISaveDir = GetLiveryGUISaveDir();
            Directory.CreateDirectory(liveryGUISaveDir);

            string liveryFileName = key + ".yaml";

            try {
                UtilitiesYAML.SaveDataToFile(liveryGUISaveDir, liveryFileName, liveryDat, false);
                Debug.Log($"[LiveryGUI] INFO: Saved {liveryFileName} to {liveryGUISaveDir}");
                LiverySnapshotDB.AddOrUpdateSnapshot(key, liveryDat);
            }
            catch (Exception ex) {
                Debug.LogError($"[LiveryGUI] WARNING: Failed to save {liveryFileName} to {liveryGUISaveDir}:\n{ex}");
                Localization.AddOverworldMessage("livery_gui_message_save_failed");
            }
        }

        //==============================================================================
        static string GetModSaveDataDir() {
            // BYG states: "do not modify user drive outside of the area already managed by PB [AppData/Local/PhantomBrigade/]"
            string appDataLocalPBDir = DataPathHelper.GetUserFolder(); // .../AppData/Local/PhantomBrigade/
            return Path.Combine(appDataLocalPBDir, "ModSavedData");
        }

        //==============================================================================
        static string GetLiveryGUISaveDir() {
            return Path.Combine(GetModSaveDataDir(), "LiveryGUI");
        }

        //==============================================================================
        public static void SaveLiverySetsToSaveGameFolder(string savePath) {
            if (string.IsNullOrEmpty(savePath))
                return;

            try {
                string rootDir = Path.Combine(savePath, SaveGameFolderName);
                string unitsDir = Path.Combine(rootDir, SaveGameUnitsFolderName);
                string pilotsDir = Path.Combine(rootDir, SaveGamePilotsFolderName);

                Directory.CreateDirectory(unitsDir);
                Directory.CreateDirectory(pilotsDir);

                int unitCount = SaveAssignmentSets(unitsDir, LiverySetsDB.ExportMechOriginalSetsByUnitName());
                int pilotCount = SaveAssignmentSets(pilotsDir, LiverySetsDB.ExportPilotSetsByPilotName());
                Debug.Log($"[LiveryGUI] Saved livery sets to save folder: units={unitCount}, pilots={pilotCount}, path={rootDir}");
            }
            catch (Exception ex) {
                Debug.LogError($"[LiveryGUI] Failed to save livery sets to save folder '{savePath}':\n{ex}");
            }
        }

        //==============================================================================
        public static void LoadLiverySetsFromSaveGameFolder(string savePath) {
            var unitSets = new Dictionary<string, LiveryAssignmentSet>(StringComparer.Ordinal);
            var pilotSets = new Dictionary<string, LiveryAssignmentSet>(StringComparer.Ordinal);

            if (string.IsNullOrEmpty(savePath)) {
                LiverySetsDB.ImportSaveGameLiverySets(unitSets, pilotSets);
                return;
            }

            try {
                string rootDir = Path.Combine(savePath, SaveGameFolderName);
                if (!Directory.Exists(rootDir)) {
                    Debug.Log($"[LiveryGUI] No save-game livery sets found at {rootDir}; livery sets will be initialized from the loaded entities.");
                    LiverySetsDB.ImportSaveGameLiverySets(unitSets, pilotSets);
                    return;
                }

                LoadAssignmentSets(Path.Combine(rootDir, SaveGameUnitsFolderName), unitSets);
                LoadAssignmentSets(Path.Combine(rootDir, SaveGamePilotsFolderName), pilotSets);
                LiverySetsDB.ImportSaveGameLiverySets(unitSets, pilotSets);
                Debug.Log($"[LiveryGUI] Loaded save-game livery sets: units={unitSets.Count}, pilots={pilotSets.Count}, path={rootDir}");
            }
            catch (Exception ex) {
                Debug.LogError($"[LiveryGUI] Failed to load livery sets from save folder '{savePath}':\n{ex}");
                LiverySetsDB.ImportSaveGameLiverySets(unitSets, pilotSets);
            }
        }

        //==============================================================================
        static int SaveAssignmentSets(string folderPath, Dictionary<string, LiveryAssignmentSet> setsByName) {
            if (setsByName == null)
                return 0;

            int savedCount = 0;
            foreach (var kv in setsByName) {
                if (string.IsNullOrEmpty(kv.Key) || kv.Value == null)
                    continue;

                UtilitiesYAML.SaveDataToFile(folderPath, kv.Key + ".yaml", kv.Value, false);
                savedCount++;
            }
            return savedCount;
        }

        //==============================================================================
        static void LoadAssignmentSets(string folderPath, Dictionary<string, LiveryAssignmentSet> setsByName) {
            if (setsByName == null || string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return;

            foreach (string path in Directory.GetFiles(folderPath, "*.yaml")) {
                try {
                    LiveryAssignmentSet set = UtilitiesYAML.LoadDataFromFile<LiveryAssignmentSet>(path, false, false);
                    if (set == null) {
                        Debug.LogWarning($"[LiveryGUI] Failed to load livery assignment set from {path}");
                        continue;
                    }

                    set.NormalizeAfterDeserialization();
                    setsByName[Path.GetFileNameWithoutExtension(path)] = set;
                }
                catch (Exception ex) {
                    Debug.LogError($"[LiveryGUI] Exception loading livery assignment set '{path}':\n{ex}");
                }
            }
        }
    }
}
