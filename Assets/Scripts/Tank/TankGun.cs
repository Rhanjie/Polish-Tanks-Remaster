using System;
using PolishTanks.Weapons;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolishTanks.Tank
{
    /// <summary>
    /// A tank weapon (main gun or coaxial MG): reload and belt feed, pooled projectiles,
    /// recoil on the hull, barrel kick, camera shake, muzzle flash and smoke.
    /// </summary>
    public class TankGun : MonoBehaviour
    {
        [SerializeField] private GunSettings settings = GunSettings.Default;
        [SerializeField] private Transform muzzle;
        [SerializeField] private SpriteRenderer muzzleFlash;

        [Header("Feedback (all optional)")]
        [Tooltip("Hull that takes the recoil and that projectiles ignore.")]
        [SerializeField] private TankMovement recoilTarget;
        [SerializeField] private BarrelRecoil barrelRecoil;
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private ParticleSystem muzzleSmoke;
        [SerializeField, Min(0)] private int smokeParticles = 8;

        private const int MaxShotsPerFrame = 8;

        private ReloadTimer reload;
        private bool started;
        private Magazine magazine;
        private Rigidbody2D ownerBody;
        private float flashTimeLeft;
        private bool triggerHeld;

        /// <summary>Raised on every shot with the gun that fired.</summary>
        public event Action<TankGun> Fired;

        public Transform Muzzle => muzzle;
        public bool IsReloaded => reload == null || reload.IsReady;
        public float ReloadProgress => reload?.Progress ?? 1f;
        public int RoundsInMagazine => magazine?.Rounds ?? 0;
        public bool IsChangingMagazine => magazine != null && magazine.IsReloading;

        /// <summary>Direction of the last shot, after spread.</summary>
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
                    PrewarmPools(); // a loadout swap may bring a new projectile
            }
        }

        /// <summary>While held, the gun fires every time it's ready.</summary>
        public void SetTrigger(bool held) => triggerHeld = held;

        public bool TryFire()
        {
            if (!isActiveAndEnabled || !magazine.CanFire || !reload.TryConsume())
                return false;
            magazine.Consume();

            Vector2 axis = muzzle != null ? muzzle.up : transform.up;
            var direction = BallisticsMath.ApplySpread(axis, settings.dispersion, Random.value, Random.value);
            LastShotDirection = direction;

            SpawnProjectile(direction);
            ApplyRecoil(direction);
            ShowMuzzleFlash();
            if (muzzleSmoke != null && smokeParticles > 0)
                muzzleSmoke.Emit(smokeParticles);

            Fired?.Invoke(this);
            return true;
        }

        private void Awake()
        {
            reload = new ReloadTimer(settings.reloadTime);
            magazine = new Magazine(settings.magazineSize, settings.magazineReloadTime);
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

        private void OnValidate()
        {
            if (reload != null)
                reload.ReloadTime = settings.reloadTime;
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

            // A fast gun at a low frame rate owes more than one shot per frame.
            var shots = 0;
            while (triggerHeld && shots < MaxShotsPerFrame && TryFire())
                shots++;

            UpdateMuzzleFlash(Time.deltaTime);
        }

        /// <summary>Creates the projectile and impact pools up front, so the first shot doesn't stall.</summary>
        private void PrewarmPools()
        {
            var definition = settings.projectile;
            if (definition == null || definition.Prefab == null)
                return;
            PoolRegistry.For(definition.Prefab, definition.PrewarmCount);
            if (definition.ImpactEffect != null)
                PoolRegistry.For(definition.ImpactEffect, Mathf.Max(2, definition.PrewarmCount / 3));
        }

        private void SpawnProjectile(Vector2 direction)
        {
            var definition = settings.projectile;
            if (definition == null || definition.Prefab == null)
                return;

            var origin = muzzle != null ? muzzle.position : transform.position;
            var shooterVelocity = ownerBody != null ? ownerBody.linearVelocity : Vector2.zero;
            var velocity = BallisticsMath.LaunchVelocity(direction, settings.projectileSpeed, shooterVelocity);

            var projectile = PoolRegistry.For(definition.Prefab, definition.PrewarmCount)
                .Get(origin, Quaternion.LookRotation(Vector3.forward, direction));
            projectile.Launch(velocity, definition.Lifetime, ownerBody, settings.impactImpulse, settings.damage, definition.ImpactEffect);
        }

        private void ApplyRecoil(Vector2 direction)
        {
            if (recoilTarget != null && settings.recoilImpulse > 0f)
            {
                // Recoil goes into the turret ring (the muzzle's parent), not the barrel tip,
                // so firing sideways nudges the hull instead of spinning it.
                var point = muzzle != null && muzzle.parent != null ? muzzle.parent.position : transform.position;
                recoilTarget.AddImpulse(-direction * settings.recoilImpulse, point);
            }

            if (barrelRecoil != null && settings.barrelKick > 0f)
                barrelRecoil.Kick(settings.barrelKick, settings.barrelReturnTime);

            if (impulseSource != null && settings.shakeForce > 0f)
                impulseSource.GenerateImpulseWithVelocity(-direction * settings.shakeForce);
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
