using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiveryGUIMod
{
    // Small in-memory store for pilot/mech livery assignment sets.
    // Does not perform persistence. Designed to be lightweight and easily testable.

    [Serializable]
    public class LiveryAssignmentSet {
        // partKey -> liveryKey
        readonly Dictionary<string, string> _assignments = new Dictionary<string, string>(StringComparer.Ordinal);

        public IReadOnlyDictionary<string, string> Assignments => _assignments;

        // Get the livery key for a given part. If the part isn't present, return the sentinel for "transparent".
        public string GetForPart(string partKey) {
            if (string.IsNullOrEmpty(partKey))
                return LiverySetsDB.PILOT_TRANSPARENT;

            if (_assignments.TryGetValue(partKey, out var val))
                return val;

            return LiverySetsDB.PILOT_TRANSPARENT;
        }

        // Set (or add) a livery assignment for a part. Passing null/empty as liveryKey will set the sentinel (transparent).
        public void SetForPart(string partKey, string liveryKey) {
            if (string.IsNullOrEmpty(partKey))
            {
                Debug.LogWarning("[PilotLiveryStore] Ignoring SetForPart call with null/empty partKey");
                return;
            }

            if (string.IsNullOrEmpty(liveryKey))
                liveryKey = LiverySetsDB.PILOT_TRANSPARENT;

            _assignments[partKey] = liveryKey;
        }

        // Remove an assignment for a part. Returns true if removed.
        public bool RemovePart(string partKey) {
            if (string.IsNullOrEmpty(partKey))
                return false;
            return _assignments.Remove(partKey);
        }

        // Clear all assignments in this set.
        public void Clear() {
            _assignments.Clear();
        }

        // Shallow clone of the set.
        public LiveryAssignmentSet Clone() {
            var copy = new LiveryAssignmentSet();
            foreach (var kv in _assignments)
                copy._assignments[kv.Key] = kv.Value;
            return copy;
        }
    }
}
