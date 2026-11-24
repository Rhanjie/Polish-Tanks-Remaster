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
        [Min(0f)] public float damage;
        [Tooltip("Shell diameter, mm. A shell three times thicker than a plate cannot glance off it.")]
        [Min(0f)] public float caliber;
        [Tooltip("Armor the projectile can defeat, rolled between min and max on every hit, mm.")]
        [Min(0f)] public float penetrationMin;
        [Min(0f)] public float penetrationMax;
        [Tooltip("Impulse given to what the projectile hits.")]
        [Min(0f)] public float impactImpulse;

        [Header("Dispersion")]
        [Tooltip("Max deviation from the barrel axis once the gun has settled, degrees. Never drops below a small floor.")]
        [Min(0f)] public float dispersion;
        [Tooltip("Upper limit of the spread, degrees.")]
        [Min(0f)] public float maxDispersion;
        [Tooltip("Degrees added per world unit per second of tank speed.")]
        [Min(0f)] public float movingDispersion;
        [Tooltip("Degrees added per degree per second the barrel swings, from the hull turning or the turret traversing.")]
        [Min(0f)] public float turningDispersion;
        [Tooltip("Degrees added by each shot.")]
        [Min(0f)] public float shotDispersion;
        [Tooltip("Seconds for the spread to close most of the way back once the gun is held still.")]
        [Min(0f)] public float aimTime;

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
