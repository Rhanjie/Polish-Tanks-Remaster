using System;
using UnityEngine;

namespace PolishTanks.Tank
{
    [Serializable]
    public struct GunSettings
    {
        [Tooltip("Seconds between shots.")]
        [Min(0.01f)] public float reloadTime;

        [Tooltip("How long the muzzle flash stays visible, seconds.")]
        [Min(0f)] public float muzzleFlashDuration;

        public static GunSettings Default => new GunSettings
        {
            reloadTime = 1.5f,
            muzzleFlashDuration = 0.06f,
        };
    }
}
