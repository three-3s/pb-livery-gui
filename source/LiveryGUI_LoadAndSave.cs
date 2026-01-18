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

            Debug.Log($"[LiveryGUI] INFO: Loaded {loadedCount} user-saved liveries. Total liveries: {liveryDict.Count} (though some may be hidden from player-selection list)");
        }

        //==============================================================================
        public static void SaveLiveryToFile(string key, DataContainerEquipmentLivery liveryDat)
        {
            if (LiverySnapshotDB.originalLiveries.ContainsKey(key) && !LiverySnapshotDB.originalLiveries[key].ownedByLiveryGUI)
            {
                Debug.Log($"[LiveryGUI] USAGE: Refusing to save to this livery key/name because LiveryGUI does not own that livery. You need to clone the livery to a new key/name. key={key}");
                //todo.status-msg-popup
                return;
            }

            string liveryGUISaveDir = GetLiveryGUISaveDir();
            Directory.CreateDirectory(liveryGUISaveDir);

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
        static string GetModSaveDataDir()
        {
            // BYG states: "do not modify user drive outside of the area already managed by PB [AppData/Local/PhantomBrigade/]"
            string appDataLocalPBDir = DataPathHelper.GetUserFolder(); // .../AppData/Local/PhantomBrigade/
            return Path.Combine(appDataLocalPBDir, "ModSavedData");
        }

        //==============================================================================
        static string GetLiveryGUISaveDir()
        {
            return Path.Combine(GetModSaveDataDir(), "LiveryGUI");
        }
    }
}
