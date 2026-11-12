using System;
using PolishTanks.CameraRig;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolishTanks.Tank
{
    /// <summary>
    /// A tank weapon (main gun or coaxial MG): reload and belt feed, pooled projectiles,
    /// recoil on the hull, barrel kick, camera shake, muzzle flash and smoke. Sits on the muzzle object:
    /// its up axis is the barrel, the flash and smoke are children, and the hull and barrel recoil are found in its parents.
    /// </summary>
    public class TankGun : MonoBehaviour
    {
        [SerializeField] private GunMount mount = GunMount.Main;

        private const int MaxShotsPerFrame = 8;

        [Inject] private IPoolService pools;
        [Inject] private ICameraShake cameraShake;

        private GunSettings settings = GunSettings.Default;
        private SpriteRenderer muzzleFlash;
        private ParticleSystem muzzleSmoke;
        private TankMovement recoilTarget;
        private BarrelRecoil barrelRecoil;
        private ReloadTimer reload;
        private bool started;
        private Magazine magazine;
        private Rigidbody2D ownerBody;
        private float flashTimeLeft;
        private bool triggerHeld;

        public event Action<TankGun> Fired;

        public GunMount Mount => mount;
        public Transform Muzzle => transform;
        public Transform MuzzleFlash => muzzleFlash != null ? muzzleFlash.transform : null;
        public bool IsReloaded => reload == null || reload.IsReady;
        public float ReloadProgress => reload?.Progress ?? 1f;
        public int RoundsInMagazine => magazine?.Rounds ?? 0;
        public bool IsChangingMagazine => magazine != null && magazine.IsReloading;
        public float MagazineReloadProgress => magazine?.ReloadProgress ?? 1f;

        public Vector2 LastShotDirection { get; private set; }

        public GunSettings Settings
        {
            get => settings;
            set
            {
                var feedChanged = value.magazineSize != settings.magazineSize || !Mathf.Approximately(value.magazineReloadTime, settings.magazineReloadTime);
                settings = value;
                if (reload != null)
                    reload.ReloadTime = value.reloadTime;
                if (magazine != null && feedChanged)
                    magazine.Configure(value.magazineSize, value.magazineReloadTime);
                if (started)
                    PrewarmPools();
            }
        }

        public static TankGun Find(GameObject tank, GunMount mount)
        {
            foreach (var gun in tank.GetComponentsInChildren<TankGun>(true))
                if (gun.mount == mount)
                    return gun;
            return null;
        }

        public void SetTrigger(bool held) => triggerHeld = held;

        public bool TryFire()
        {
            if (!isActiveAndEnabled || !magazine.CanFire || !reload.TryConsume())
                return false;
            magazine.Consume();

            Vector2 axis = transform.up;
            var direction = BallisticsMath.ApplySpread(axis, settings.dispersion, Random.value, Random.value);
            LastShotDirection = direction;

            SpawnProjectile(direction);
            ApplyRecoil(direction);
            ShowMuzzleFlash();
            if (muzzleSmoke != null && settings.smokeParticles > 0)
                muzzleSmoke.Emit(settings.smokeParticles);

            Fired?.Invoke(this);
            return true;
        }

        private void Awake()
        {
            reload = new ReloadTimer(settings.reloadTime);
            magazine = new Magazine(settings.magazineSize, settings.magazineReloadTime);
            muzzleFlash = GetComponentInChildren<SpriteRenderer>(true);
            muzzleSmoke = GetComponentInChildren<ParticleSystem>(true);
            recoilTarget = GetComponentInParent<TankMovement>();
            barrelRecoil = GetComponentInParent<BarrelRecoil>();
            if (recoilTarget != null)
                ownerBody = recoilTarget.GetComponent<Rigidbody2D>();
            if (muzzleFlash != null)
                muzzleFlash.enabled = false;
        }

        private void OnDisable()
        {
            triggerHeld = false;
            flashTimeLeft = 0f;
            if (muzzleFlash != null)
                muzzleFlash.enabled = false;
        }

        private void Start()
        {
            started = true;
            PrewarmPools();
        }

        private void Update()
        {
            reload.Tick(Time.deltaTime);
            magazine.Tick(Time.deltaTime);

            var shots = 0;
            while (triggerHeld && shots < MaxShotsPerFrame && TryFire())
                shots++;

            UpdateMuzzleFlash(Time.deltaTime);
        }

        private void PrewarmPools()
        {
            var definition = settings.projectile;
            if (pools == null || definition == null || definition.Prefab == null)
                return;
            pools.For(definition.Prefab, definition.PrewarmCount);
            EffectPool(definition.ImpactEffect, definition);
            EffectPool(definition.BlockedEffect, definition);
            EffectPool(definition.RicochetEffect, definition);
            EffectPool(definition.OtherRicochetEffect, definition);
        }

        private void SpawnProjectile(Vector2 direction)
        {
            var definition = settings.projectile;
            if (definition == null || definition.Prefab == null)
                return;
            if (pools == null)
            {
                Debug.LogWarning($"{name}: no {nameof(IPoolService)} injected, the shot has no projectile.", this);
                return;
            }

            var origin = transform.position;
            var shooterVelocity = ownerBody != null ? ownerBody.linearVelocity : Vector2.zero;
            var velocity = BallisticsMath.LaunchVelocity(direction, settings.projectileSpeed, shooterVelocity);

            var projectile = pools.For(definition.Prefab, definition.PrewarmCount)
                .Get(origin, Quaternion.LookRotation(Vector3.forward, direction));
            projectile.Launch(new ProjectileShot
            {
                Velocity = velocity,
                Lifetime = definition.Lifetime,
                Shooter = ownerBody,
                Impulse = settings.impactImpulse,
                Damage = settings.damage,
                PenetrationMin = settings.penetrationMin,
                PenetrationMax = settings.penetrationMax,
                Caliber = settings.caliber,
                ImpactPool = EffectPool(definition.ImpactEffect, definition),
                BlockedPool = EffectPool(definition.BlockedEffect, definition),
                RicochetPool = EffectPool(definition.RicochetEffect, definition),
                OtherRicochetPool = EffectPool(definition.OtherRicochetEffect, definition),
            });
        }

        private PrefabPool<PooledEffect> EffectPool(PooledEffect effect, ProjectileDefinition definition) =>
            effect != null ? pools.For(effect, Mathf.Max(2, definition.PrewarmCount / 3)) : null;

        private void ApplyRecoil(Vector2 direction)
        {
            if (recoilTarget != null && settings.recoilImpulse > 0f)
            {
                var point = transform.parent != null ? transform.parent.position : transform.position;
                recoilTarget.AddImpulse(-direction * settings.recoilImpulse, point);
            }

            if (barrelRecoil != null && settings.barrelKick > 0f)
                barrelRecoil.Kick(settings.barrelKick, settings.barrelReturnTime);

            var origin = transform.position;
            cameraShake?.Shake(origin, -direction, settings.shakeForce);
        }

        private void ShowMuzzleFlash()
        {
            if (muzzleFlash == null || settings.muzzleFlashDuration <= 0f)
                return;
            flashTimeLeft = settings.muzzleFlashDuration;
            muzzleFlash.enabled = true;
        }

        private void UpdateMuzzleFlash(float deltaTime)
        {
            if (flashTimeLeft <= 0f)
                return;
            flashTimeLeft -= deltaTime;
            if (flashTimeLeft <= 0f && muzzleFlash != null)
                muzzleFlash.enabled = false;
        }
    }
}
