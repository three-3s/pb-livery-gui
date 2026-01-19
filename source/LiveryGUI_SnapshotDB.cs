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
        public static Dictionary<string, LiveryOriginalState> originalLiveries = new Dictionary<string, LiveryOriginalState>(); // key = livery's key
        public readonly static LiveryOriginalState defaultLivery = new LiveryOriginalState(false, new DataContainerEquipmentLivery()); // for null livery

        //==============================================================================
        public class LiveryOriginalState
        {
            public bool ownedByLiveryGUI; // (as opposed to being built-in or provided by a basic liveries mod)
            public DataContainerEquipmentLivery onDiskDat; // (initial/unmodified, OR last-saved state. for 'revert' & 'is modified')

            //--------------------------------------------------------------
            public LiveryOriginalState(bool liveryOwnedByLiveryGUI, DataContainerEquipmentLivery liveryDat)
            {
                this.ownedByLiveryGUI = liveryOwnedByLiveryGUI;
                UpdateLiveryDatSnapshot(liveryDat);
            }

            //--------------------------------------------------------------
            public void UpdateLiveryDatSnapshot(DataContainerEquipmentLivery newDat)
            {
                onDiskDat = DeepCopyLiveryDat(newDat);
            }
        }

        //==============================================================================
        static void DoInitIfNecessary()
        {
            // no-op
        }

        //==============================================================================
        public static DataContainerEquipmentLivery DeepCopyLiveryDat(DataContainerEquipmentLivery original)
        {
            return JsonUtility.FromJson<DataContainerEquipmentLivery>(JsonUtility.ToJson(original)); // (well. i guess it works.)
        }

        //==============================================================================
        public static bool LiveryDatsMatch(DataContainerEquipmentLivery a, DataContainerEquipmentLivery b)
        {
            return (JsonUtility.ToJson(a) == JsonUtility.ToJson(b)); // (may as well commit to the bit.)
        }

        //==============================================================================
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

        //==============================================================================
        // Record info about the liveries that are initially present (the built-in ones, and ones provided by basic livery mods)
        public static void SnapshotInitialLiveries()
        {
            var liveryDict = DataMultiLinkerEquipmentLivery.data;
            if (liveryDict == null)
            {
                Debug.Log($"[LiveryGUI] ERROR: SnapshotInitialLiveries(): DataMultiLinkerEquipmentLivery.data is null");
                return;
            }

            DoInitIfNecessary();

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

        //==============================================================================
        public static void AddOrUpdateSnapshot(string key, DataContainerEquipmentLivery liveryDat)
        {
            if (!originalLiveries.ContainsKey(key))
            {
                AddLiveryDataSnapshot(key, liveryDat, true);
            }
            else
            {
                if(!originalLiveries[key].ownedByLiveryGUI)
                {
                    Debug.Log($"[LiveryGUI] BUG: AddOrUpdateSnapshot(): key already exists, but LiverGUI mod does not own it?: {key}");
                    return;
                }
                originalLiveries[key].UpdateLiveryDatSnapshot(liveryDat);
            }
        }

        //==============================================================================
        public static bool IsCurrentLiveryModified()
        {
            string key = CIViewBaseLoadout.selectedUnitLivery;

            if (string.IsNullOrEmpty(key))
                return false;

            var liveryDict = DataMultiLinkerEquipmentLivery.data;
            if (liveryDict == null || !liveryDict.ContainsKey(key) || liveryDict[key] == null)
            {
                Debug.Log($"[LiveryGUI] ERROR: IsCurrentLiveryModified(): DataMultiLinkerEquipmentLivery.data is null ({liveryDict == null}) or doesn't contain selected livery key (key={key}, present={liveryDict.ContainsKey(key)}) or the livery is null ({liveryDict[key] == null}).");
                return false;
            }

            if (!originalLiveries.ContainsKey(key))
            {
                Debug.Log($"[LiveryGUI] BUG: IsCurrentLiveryModified(): originalLiveries DB doesn't contain key={key}");
                return false;
            }

            return !LiveryDatsMatch(LiverySnapshotDB.originalLiveries[key].onDiskDat, DataMultiLinkerEquipmentLivery.data[key]);
        }
    }
}
