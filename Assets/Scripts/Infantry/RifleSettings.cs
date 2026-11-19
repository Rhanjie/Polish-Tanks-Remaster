using System;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>A soldier's rifle: the bullet it fires, its ballistics and how it is fired (short bursts with pauses).</summary>
    [Serializable]
    public struct RifleSettings
    {
        public ProjectileDefinition projectile;
        [Min(1f)] public float range;
        [Min(1f)] public float projectileSpeed;
        [Tooltip("Caliber in mm. Below 20 counts as small arms against tanks: it can only blind or crack vision blocks.")]
        [Min(0f)] public float caliber;
        [Min(0f)] public float damage;
        [Min(0f)] public float penetrationMin;
        [Min(0f)] public float penetrationMax;
        [Min(0f)] public float impactImpulse;
        [Tooltip("Random spread either side of the aim, degrees.")]
        [Range(0f, 20f)] public float spread;
        [Min(1)] public int burstShots;
        [Tooltip("Seconds between shots within a burst.")]
        [Min(0.05f)] public float shotInterval;
        [Tooltip("Average seconds between bursts; each pause varies by a third either way.")]
        [Min(0f)] public float burstPause;

        public static RifleSettings Default => new RifleSettings
        {
            range = 26f,
            projectileSpeed = 70f,
            caliber = 7.6f,
            damage = 6f,
            penetrationMin = 5f,
            penetrationMax = 8f,
            impactImpulse = 0.2f,
            spread = 5f,
            burstShots = 3,
            shotInterval = 0.3f,
            burstPause = 1.6f,
        };
    }
}
