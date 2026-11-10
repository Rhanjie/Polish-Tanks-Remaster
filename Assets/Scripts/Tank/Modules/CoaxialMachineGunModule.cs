using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Coaxial Machine Gun", fileName = "Mg_")]
    public class CoaxialMachineGunModule : TankModule
    {
        [SerializeField, Min(1f)] private float caliber = 7.92f;
        [SerializeField, Min(1f)] private float roundsPerMinute = 900f;
        [SerializeField, Min(0f)] private float dispersion = 2f;
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
        [SerializeField, Min(0f)] private float impactImpulse = 0.15f;
        [Tooltip("Camera shake per round. Keep it tiny, MGs fire fast.")]
        [SerializeField, Min(0f)] private float shakeForce = 0.03f;

        public override ModuleSlot Slot => ModuleSlot.CoaxialMachineGun;

        public float Caliber => caliber;
        public float RoundsPerMinute => roundsPerMinute;
        public float FireInterval => 60f / roundsPerMinute;
        public float Dispersion => dispersion;
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
