using System;
using UnityEngine;

namespace PolishTanks.Background
{
    [Serializable]
    public struct HillSettings
    {
        [Min(0.1f)] public float width;
        [Tooltip("Local y of the flat bottom edge of the silhouette.")]
        public float bottom;
        [Tooltip("Average local y of the ridge line.")]
        public float baseHeight;
        [Min(0f)] public float amplitude;
        [Min(0.001f)] public float frequency;
        [Range(8, 512)] public int segments;

        public static HillSettings Default => new HillSettings
        {
            width = 30f,
            bottom = -6f,
            baseHeight = -1f,
            amplitude = 1f,
            frequency = 0.4f,
            segments = 128,
        };
    }
}
