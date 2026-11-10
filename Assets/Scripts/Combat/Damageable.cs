using System;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolishTanks.Combat
{
    /// <summary>
    /// Takes projectile hits through armor. The plate is chosen from the hit surface against this object's up axis,
    /// penetration is rolled per hit, and only penetrating hits cost hit points.
    /// </summary>
    public class Damageable : MonoBehaviour, IProjectileHitHandler
    {
        [SerializeField] private DamageableProfile profile;

        [Inject] private IPoolService pools;

        private HealthPool health;
        private ArmorProfile armor;

        public event Action<PenetrationResult, float> Hit;
        public event Action<Damageable> Died;

        public ArmorProfile Armor => armor;
        public float HitPoints => Health.Current;
        public float MaxHitPoints => Health.Max;
        public float HealthFraction => Health.Fraction;
        public bool IsDead => Health.IsDead;
        public PenetrationResult LastResult { get; private set; }

        private HealthPool Health
        {
            get
            {
                if (health == null)
                {
                    health = new HealthPool(profile != null ? profile.HitPoints : 100f);
                    armor = profile != null ? profile.Armor : default;
                    health.Died += OnDied;
                }
                return health;
            }
        }

        public void Configure(float maxHitPoints, ArmorProfile newArmor)
        {
            Health.SetMax(maxHitPoints);
            armor = newArmor;
        }

        public float ApplyDamage(float amount) => Health.TakeDamage(amount);

        public HitOutcome OnProjectileHit(in ProjectileHit hit)
        {
            if (IsDead)
                return HitOutcome.Ignored;

            LastResult = ArmorMath.Resolve(armor, transform.up, hit.Normal, hit.Direction,
                hit.PenetrationMin, hit.PenetrationMax, Random.value);

            var dealt = LastResult.Penetrated ? Health.TakeDamage(hit.Damage) : 0f;
            Hit?.Invoke(LastResult, dealt);
            return LastResult.Penetrated ? HitOutcome.Penetrated : HitOutcome.Blocked;
        }

        private void Awake() => _ = Health;

        private void OnDied()
        {
            if (profile != null && profile.DestroyedEffect != null && pools != null)
                pools.For(profile.DestroyedEffect, 1).Get(transform.position, Quaternion.identity);

            Died?.Invoke(this);

            if (profile != null && profile.RemoveOnDeath)
                Destroy(gameObject);
        }
    }
}
