using System;
using PolishTanks.Weapons;
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

        [Tooltip("Smoke particles emitted from the muzzle per shot.")]
        [Min(0)] public int smokeParticles;

        [Header("Projectile")]
        [Tooltip("What the gun fires. Without one the gun only flashes.")]
        public ProjectileDefinition projectile;
        [Min(0f)] public float projectileSpeed;
        [Tooltip("Max deviation from the barrel axis, degrees.")]
        [Min(0f)] public float dispersion;
        [Min(0f)] public float damage;
        [Tooltip("Impulse given to what the projectile hits.")]
        [Min(0f)] public float impactImpulse;

        [Header("Kick")]
        [Tooltip("Impulse pushing the tank back on each shot.")]
        [Min(0f)] public float recoilImpulse;
        [Tooltip("Camera shake strength.")]
        [Min(0f)] public float shakeForce;
        [Tooltip("How far the turret sprite slides back, world units.")]
        [Min(0f)] public float barrelKick;
        [Min(0.01f)] public float barrelReturnTime;

        [Header("Feed")]
        [Tooltip("Rounds per belt/magazine. 0 = fed continuously.")]
        [Min(0)] public int magazineSize;
        [Min(0f)] public float magazineReloadTime;

        public static GunSettings Default => new GunSettings
        {
            reloadTime = 1.5f,
            muzzleFlashDuration = 0.06f,
            projectileSpeed = 40f,
            barrelReturnTime = 0.3f,
        };
    }
}
