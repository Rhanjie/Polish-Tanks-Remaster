using System;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// A soldier's anti-tank grenades: the thrown prefab, how often one comes ready, the throwing distances, how far off
    /// it can land, how long it flies and burns, and the blast it makes. No prefab means the soldier carries none.
    /// </summary>
    [Serializable]
    public struct GrenadeSettings
    {
        public Grenade prefab;
        [Tooltip("Average seconds between throws; each wait varies by a fifth either way.")]
        [Min(0f)] public float cooldown;
        [Min(0f)] public float minRange;
        [Min(1f)] public float maxRange;
        [Tooltip("Largest distance in metres the grenade lands from where it was aimed.")]
        [Min(0f)] public float scatter;
        [Tooltip("Flight seconds per metre thrown.")]
        [Min(0.01f)] public float secondsPerMetre;
        [Min(0f)] public float fuse;
        [Tooltip("Seconds the soldier stands still to throw.")]
        [Min(0f)] public float windup;
        public BlastSettings blast;

        public bool IsCarried => prefab != null;

        public static GrenadeSettings Default => new GrenadeSettings
        {
            cooldown = 20f,
            minRange = 4f,
            maxRange = 14f,
            scatter = 1.2f,
            secondsPerMetre = 0.07f,
            fuse = 1.2f,
            windup = 0.6f,
            blast = new BlastSettings
            {
                radius = 2.5f,
                penetrationMin = 30f,
                penetrationMax = 45f,
                damage = 120f,
                caliber = 40f,
                impulse = 4f,
            },
        };
    }
}
