using PolishTanks.Weapons;
using UnityEngine;
using UnityEngine.Serialization;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Main Gun", fileName = "Gun_")]
    public class MainGunModule : TankModule
    {
        [SerializeField, Min(1f)] private float caliber = 75f;
        [SerializeField, Min(0.05f)] private float reloadTime = 2.5f;

        [Tooltip("Muzzle velocity in m/s. Projectiles scale it to world units.")]
        [SerializeField, Min(1f)] private float muzzleVelocity = 600f;

        [SerializeField, Min(0f)] private float damage = 120f;

        [Tooltip("Armor penetration in mm, rolled between min and max on every hit.")]
        [SerializeField, Min(0f)] private float penetrationMin = 70f;
        [FormerlySerializedAs("penetration")]
        [SerializeField, Min(0f)] private float penetrationMax = 90f;

        [Tooltip("Random spread of a shot, degrees.")]
        [SerializeField, Min(0f)] private float dispersion = 0.4f;

        [Tooltip("Shells carried before resupply.")]
        [SerializeField, Min(1)] private int ammoCapacity = 60;

        [Tooltip("Muzzle flash size multiplier.")]
        [SerializeField, Min(0.1f)] private float muzzleFlashScale = 1f;

        [Tooltip("Smoke particles puffed from the muzzle per shot.")]
        [SerializeField, Min(0)] private int muzzleSmoke = 10;

        [Header("Projectile")]
        [SerializeField] private ProjectileDefinition projectile;
        [Tooltip("Shove given to what the shell hits (mass × velocity, tank mass is in tonnes).")]
        [SerializeField, Min(0f)] private float impactImpulse = 5f;

        [Header("Kick")]
        [Tooltip("Shove on the tank per shot (mass × velocity, tank mass is in tonnes).")]
        [SerializeField, Min(0f)] private float recoilImpulse = 20f;
        [Tooltip("Camera shake strength per shot.")]
        [SerializeField, Min(0f)] private float shakeForce = 0.6f;
        [Tooltip("How far the turret slides back on a shot, world units.")]
        [SerializeField, Min(0f)] private float barrelKick = 0.35f;
        [SerializeField, Min(0.01f)] private float barrelReturnTime = 0.3f;

        public override ModuleSlot Slot => ModuleSlot.MainGun;

        public float Caliber => caliber;
        public float ReloadTime => reloadTime;
        public float MuzzleVelocity => muzzleVelocity;
        public float Damage => damage;
        public float PenetrationMin => penetrationMin;
        public float PenetrationMax => penetrationMax;
        public float Dispersion => dispersion;
        public int AmmoCapacity => ammoCapacity;
        public float MuzzleFlashScale => muzzleFlashScale;
        public int MuzzleSmoke => muzzleSmoke;
        public ProjectileDefinition Projectile => projectile;
        public float ImpactImpulse => impactImpulse;
        public float RecoilImpulse => recoilImpulse;
        public float ShakeForce => shakeForce;
        public float BarrelKick => barrelKick;
        public float BarrelReturnTime => barrelReturnTime;
    }
}
