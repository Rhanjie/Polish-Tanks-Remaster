using System;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolishTanks.Combat
{
    /// <summary>
    /// Takes projectile hits through armor. The plate is chosen from the hit surface against this object's up axis.
    /// A steep enough hit may glance off (once per shell); otherwise penetration is rolled and only penetrating hits
    /// cost hit points.
    /// </summary>
    public class Damageable : MonoBehaviour, IProjectileHitHandler
    {
        [SerializeField] private DamageableProfile profile;

        [Inject] private IPoolService pools;

        private HealthPool health;
        private ArmorProfile armor;

        public event Action<PenetrationResult, HitOutcome, float> Hit;
        public event Action<Damageable> Died;

        public ArmorProfile Armor => armor;
        public float HitPoints => Health.Current;
        public float MaxHitPoints => Health.Max;
        public float HealthFraction => Health.Fraction;
        public bool IsDead => Health.IsDead;
        public PenetrationResult LastResult { get; private set; }
        public HitOutcome LastOutcome { get; private set; }

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

        public HitResponse OnProjectileHit(in ProjectileHit hit)
        {
            if (IsDead)
            {
                LastOutcome = HitOutcome.Ignored;
                return HitResponse.Ignored;
            }

            var localFace = ArmorMath.SnapToFace(transform.InverseTransformDirection(hit.Normal),
                transform.InverseTransformDirection(hit.Direction));
            Vector2 face = transform.TransformDirection(localFace);
            var side = ArmorMath.GetSide(transform.up, face);
            var plate = armor.For(side);

            if (!hit.Ricocheted && GlancesOff(hit, face, plate))
            {
                LastResult = new PenetrationResult(side, ArmorMath.EffectiveArmor(plate, face, hit.Direction), 0f);
                return Respond(HitOutcome.Ricochet, face, 0f);
            }

            LastResult = ArmorMath.Resolve(armor, transform.up, face, hit.Direction,
                hit.PenetrationMin, hit.PenetrationMax, Random.value);
            var dealt = LastResult.Penetrated ? Health.TakeDamage(hit.Damage) : 0f;
            return Respond(LastResult.Penetrated ? HitOutcome.Penetrated : HitOutcome.Blocked, face, dealt);
        }

        private static bool GlancesOff(in ProjectileHit hit, Vector2 face, float plate)
        {
            var angle = ArmorMath.ImpactAngle(face, hit.Direction);
            return RicochetMath.Rolls(angle, RicochetMath.IsOvermatch(hit.Caliber, plate), Random.value);
        }

        private HitResponse Respond(HitOutcome outcome, Vector2 face, float dealt)
        {
            LastOutcome = outcome;
            Hit?.Invoke(LastResult, outcome, dealt);
            return new HitResponse(outcome, face);
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
