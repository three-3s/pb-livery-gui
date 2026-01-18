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
    // database of snapshots of initial/last-saved-to-disk state for each livery
    public class LiverySnapshotDB
    {
        public class LiveryOriginalState
        {
            public bool ownedByLiveryGUI; // (as opposed to being built-in or provided by a basic liveries mod)
            public DataContainerEquipmentLivery onDiskDat; // (initial/unmodified, OR last-saved state. for 'revert' & 'is modified')
            public void UpdateLiveryDatSnapshot(DataContainerEquipmentLivery newDat)
            {
                onDiskDat = DeepCopyLiveryDat(newDat);
            }
            public LiveryOriginalState(bool liveryOwnedByLiveryGUI, DataContainerEquipmentLivery liveryDat)
            {
                this.ownedByLiveryGUI = liveryOwnedByLiveryGUI;
                UpdateLiveryDatSnapshot(liveryDat);
            }
        }

        public static Dictionary<string, LiveryOriginalState> originalLiveries; // key = livery's key

        static void DoInitIfNecessary()
        {
            if (originalLiveries == null)
            {
                originalLiveries = new Dictionary<string, LiveryOriginalState>();
            }
        }

        public static DataContainerEquipmentLivery DeepCopyLiveryDat(DataContainerEquipmentLivery original)
        {
            return JsonUtility.FromJson<DataContainerEquipmentLivery>(JsonUtility.ToJson(original)); // (well. i guess it works.)
        }

        public static void AddLiveryDataSnapshot(string key, DataContainerEquipmentLivery liveryDat, bool liveryOwnedByLiveryGUI)
        {
            DoInitIfNecessary();
            if (string.IsNullOrEmpty(key) || liveryDat == null)
            {
                Debug.Log($"[LiveryGUI] BUG: AddLiveryDataSnapshot(): key/dat is null? {key}, liverDat_null={liveryDat == null}");
                return;
            }
            if (originalLiveries.ContainsKey(key))
            {
                Debug.Log($"[LiveryGUI] BUG: AddLiveryDataSnapshot(): key already exists?: {key}");
                return;
            }
            LiveryOriginalState newRec = new LiveryOriginalState(liveryOwnedByLiveryGUI, liveryDat);
            originalLiveries[key] = newRec;
        }

        // Record info about the liveries that are initially present (the built-in ones, and ones provided by basic livery mods)
        public static void SnapshotInitialLiveries()
        {
            var liveryDict = DataMultiLinkerEquipmentLivery.data;
            if (liveryDict == null)
            {
                Debug.Log($"[LiveryGUI] ERROR: SnapshotInitialLiveries(): DataMultiLinkerEquipmentLivery.data is null");
                return;
            }

            if (originalLiveries == null)
            {
                originalLiveries = new Dictionary<string, LiveryOriginalState>();
            }

            foreach (KeyValuePair<string, DataContainerEquipmentLivery> item in liveryDict)
            {
                string key = item.Key;
                DataContainerEquipmentLivery liveryDat = item.Value;
                if (key == null || liveryDat == null)
                {
                    Debug.Log($"[LiveryGUI] BUG: SnapshotInitialLiveries(): null key/val encountered: key={key}, valIsNull={liveryDat == null}");
                    continue;
                }
                if (originalLiveries.ContainsKey(key))
                {
                    Debug.Log($"[LiveryGUI] BUG: SnapshotInitialLiveries(): key already exists?: {key}");
                    continue;
                }
                AddLiveryDataSnapshot(key, liveryDat, false);
            }
        }
    }
}
