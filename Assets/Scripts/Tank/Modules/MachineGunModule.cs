using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>
    /// What every tank machine gun shares, wherever it is mounted: rate of fire, belt feed, spread and the bullet.
    /// <see cref="CoaxialMachineGunModule"/> and <see cref="HullMachineGunModule"/> only pick the slot.
    /// </summary>
    public abstract class MachineGunModule : TankModule
    {
        [SerializeField, Min(1f)] private float caliber = 7.92f;
        [SerializeField, Min(1f)] private float roundsPerMinute = 900f;
        [Tooltip("Spread once the tank stands still and the gun has settled, degrees.")]
        [SerializeField, Min(0f)] private float dispersion = 3f;
        [Tooltip("Seconds for the spread to close most of the way back after moving or firing.")]
        [SerializeField, Min(0f)] private float aimTime = 0.8f;
        [Tooltip("Degrees each round adds to the spread, so long bursts walk wide.")]
        [SerializeField, Min(0f)] private float shotDispersion = 0.25f;
        [SerializeField, Min(0f)] private float damage = 8f;
        [Tooltip("Armor penetration in mm, rolled between min and max on every hit.")]
        [SerializeField, Min(0f)] private float penetrationMin = 6f;
        [SerializeField, Min(0f)] private float penetrationMax = 10f;

        [Tooltip("Rounds per belt before a belt change.")]
        [SerializeField, Min(1)] private int beltSize = 150;

        [Tooltip("Seconds to change a belt.")]
        [SerializeField, Min(0f)] private float beltChangeTime = 4f;

        [Header("Projectile")]
        [SerializeField] private ProjectileDefinition projectile;
        [Tooltip("Muzzle velocity in m/s. Projectiles scale it to world units.")]
        [SerializeField, Min(1f)] private float muzzleVelocity = 750f;
        [Tooltip("Push on whatever the bullet hits. Rifle-calibre rounds are too light to shove crates or vehicles, so keep it at 0.")]
        [SerializeField, Min(0f)] private float impactImpulse;
        [Tooltip("Camera shake per round. Keep it tiny, MGs fire fast.")]
        [SerializeField, Min(0f)] private float shakeForce = 0.03f;

        public float Caliber => caliber;
        public float RoundsPerMinute => roundsPerMinute;
        public float FireInterval => 60f / roundsPerMinute;
        public float Dispersion => dispersion;
        public float AimTime => aimTime;
        public float ShotDispersion => shotDispersion;
        public float Damage => damage;
        public float PenetrationMin => penetrationMin;
        public float PenetrationMax => penetrationMax;
        public int BeltSize => beltSize;
        public float BeltChangeTime => beltChangeTime;
        public ProjectileDefinition Projectile => projectile;
        public float MuzzleVelocity => muzzleVelocity;
        public float ImpactImpulse => impactImpulse;
        public float ShakeForce => shakeForce;
    }
}
