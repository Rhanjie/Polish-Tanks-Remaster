using System;
using UnityEngine;

namespace PolishTanks.Background
{
    [Serializable]
    public struct TreeSettings
    {
        [Min(0)] public int count;
        [Min(0.01f)] public float minHeight;
        [Min(0.01f)] public float maxHeight;
        [Tooltip("Crown width as a fraction of tree height.")]
        [Range(0.1f, 1.5f)] public float widthRatio;
        [Tooltip("Share of trees drawn as round deciduous crowns instead of pines.")]
        [Range(0f, 1f)] public float roundTreeShare;

        public static TreeSettings Default => new TreeSettings
        {
            count = 30,
            minHeight = 0.6f,
            maxHeight = 1.2f,
            widthRatio = 0.45f,
            roundTreeShare = 0.3f,
        };
    }
}
