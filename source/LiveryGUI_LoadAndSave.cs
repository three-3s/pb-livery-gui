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
    public class LoadAndSave
    {
        //==============================================================================
        public static void LoadUserSavedLiveries()
        {
            string liveryGUISaveDir = GetLiveryGUISaveDir();

            if (!Directory.Exists(liveryGUISaveDir))
            {
                Debug.Log($"[LiveryGUI] INFO: User-saved liveries directory doesn't exist yet (i.e., user hasn't saved any liveries yet; {liveryGUISaveDir}).");
                return;
            }

            var files = Directory.GetFiles(liveryGUISaveDir, "*.yaml");
            if (files.Length == 0)
            {
                Debug.Log($"[LiveryGUI] INFO: No user-saved livery files found (i.e., user hasn't saved any liveries yet; no *.yaml files in {liveryGUISaveDir}).");
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
                        Debug.LogWarning($"[LiveryGUI] WARNING: Failed to load livery from {path}");
                        continue;
                    }

                    string key = Path.GetFileNameWithoutExtension(path);
                    livery.key = key;

                    if (string.IsNullOrEmpty(livery.textName))
                        livery.textName = key;

                    // Avoid collisions with base-game or other mods
                    if (liveryDict.ContainsKey(livery.key))
                    {
                        Debug.LogWarning($"[LiveryGUI] WARNING: Livery key already exists, skipping: {livery.key}");
                        continue;
                    }

                    liveryDict[livery.key] = livery;
                    loadedCount++;

                    LiverySnapshotDB.AddLiveryDataSnapshot(key, livery, true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LiveryGUI] WARNING: Exception loading livery file '{path}':\n{ex}");
                }
            }

            Debug.Log($"[LiveryGUI] INFO: Loaded {loadedCount} user-saved liveries. Total liveries: {liveryDict.Count}");
        }

        //==============================================================================
        public static void SaveLiveryToFile(string key, DataContainerEquipmentLivery liveryDat)
        {
            if(LiverySnapshotDB.originalLiveries.ContainsKey(key) && !LiverySnapshotDB.originalLiveries[key].ownedByLiveryGUI)
            {
                Debug.Log($"[LiveryGUI] USAGE: Refusing to save to this livery key/name because LiveryGUI does not own that livery. You need to clone the livery to a new key/name. key={key}");
                //todo.status-msg-popup
                return;
            }

            string userSaveDataDir = GetUserSaveDataDir();
            string liveryGUISaveDir = GetLiveryGUISaveDir();
            Directory.CreateDirectory(liveryGUISaveDir);

            string saveDirMetadataFilePath = Path.Combine(userSaveDataDir, "metadata.yaml");
            if (!File.Exists(saveDirMetadataFilePath))
            {
                File.WriteAllText(saveDirMetadataFilePath, saveDirMetadataContent);
                Debug.Log($"[LiveryGUI] INFO: Wrote new {saveDirMetadataFilePath} (to suppress future warnings about that directory not itself being a mod)");
            }

            string liveryFileName = key + ".yaml";

            try
            {
                UtilitiesYAML.SaveDataToFile(liveryGUISaveDir, liveryFileName, liveryDat, false);
                Debug.Log($"[LiveryGUI] INFO: Saved {liveryFileName} to {liveryGUISaveDir}");
                LiverySnapshotDB.AddOrUpdateSnapshot(key, liveryDat);
            }
            catch (Exception ex)
            {
                Debug.Log($"[LiveryGUI] WARNING: Failed to save {liveryFileName} to {liveryGUISaveDir}:\n{ex}");
            }
        }

        //==============================================================================
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

        //==============================================================================
        static string GetLiveryGUISaveDir()
        {
            return Path.Combine(GetUserSaveDataDir(), "LiveryGUI");
        }

        //==============================================================================
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
    }
}
