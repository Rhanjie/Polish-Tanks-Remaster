using System;
using PolishTanks.Combat;
using PolishTanks.Tank.Modules;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Builds the tank from a <see cref="TankLoadout"/>: computes its stats and pushes them
    /// into movement, turret, guns and armor, and swaps sprites, collider and mass to match the modules.
    /// <see cref="Fit"/>, <see cref="Remove(ModuleSlot)"/> and <see cref="Modify"/> swap single modules while the game runs.
    /// When its <see cref="Damageable"/> dies the tank becomes a wreck: controls and guns stop and the sprites darken.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(TankMovement))]
    [RequireComponent(typeof(Damageable))]
    public class TankController : MonoBehaviour
    {
        private const float WreckDamping = 4f;
        private static readonly Color WreckTint = new Color(0.35f, 0.33f, 0.3f, 1f);

        [SerializeField] private TankLoadout loadout;

        private TankMovement movement;
        private Rigidbody2D body;
        private BoxCollider2D hullCollider;
        private TurretAimer aimer;
        private TankGun gun;
        private TankGun coaxialGun;
        private TankGun hullGun;
        private BallMount ballMount;
        private HullView hullView;
        private TurretView turretView;
        private Damageable damageable;
        private bool partsFound;
        private bool initialized;
        private bool reapplyPending;
        private Vector3? baseMuzzleFlashScale;

        public event Action<TankStats> StatsApplied;
        public event Action<TankController> Destroyed;

        public bool IsDestroyed { get; private set; }

        public TankLoadout Loadout { get; private set; }

        public TankStats Stats { get; private set; }

        public bool Apply(TankLoadout newLoadout)
        {
            var errors = TankStatsCalculator.Validate(newLoadout);
            if (errors.Count > 0)
            {
                Debug.LogError($"{name}: can't apply loadout '{(newLoadout != null ? newLoadout.name : "null")}'. {string.Join(" ", errors)}", this);
                return false;
            }

            FindParts();
            var previous = Loadout;
            Loadout = newLoadout;
            loadout = newLoadout;
            Stats = TankStatsCalculator.Calculate(newLoadout);
            ApplyStats(Stats);
            ApplyVisuals(newLoadout);
            StatsApplied?.Invoke(Stats);
            if (previous != newLoadout)
                LoadoutBuilder.Release(previous);
            return true;
        }

        public bool Fit(TankModule module) => Modify(builder => builder.Fit(module));

        public bool Remove(ModuleSlot slot) => Modify(builder => builder.Remove(slot));

        public bool Remove(TankModule module) => Modify(builder => builder.Remove(module));

        public bool Modify(Action<LoadoutBuilder> change)
        {
            var builder = new LoadoutBuilder(Loadout);
            change(builder);
            var next = builder.Build();
            if (Apply(next))
                return true;
            LoadoutBuilder.Release(next);
            return false;
        }

        private void FindParts()
        {
            if (partsFound)
                return;
            partsFound = true;
            movement = GetComponent<TankMovement>();
            body = GetComponent<Rigidbody2D>();
            hullCollider = GetComponent<BoxCollider2D>();
            aimer = GetComponentInChildren<TurretAimer>(true);
            gun = TankGun.Find(gameObject, GunMount.Main);
            coaxialGun = TankGun.Find(gameObject, GunMount.Coaxial);
            hullGun = TankGun.Find(gameObject, GunMount.Hull);
            ballMount = GetComponentInChildren<BallMount>(true);
            hullView = GetComponentInChildren<HullView>(true);
            damageable = GetComponent<Damageable>();
            if (damageable != null)
                damageable.Died += OnDied;
            turretView = GetComponentInChildren<TurretView>(true);
        }

        private void Awake()
        {
            FindParts();
            initialized = true;
            if (loadout != null)
                Apply(loadout);
        }

        private void OnDestroy()
        {
            LoadoutBuilder.Release(Loadout);
        }

        private void OnValidate()
        {
            if (Application.isPlaying && initialized && loadout != null)
                reapplyPending = true;
        }

        private void Update()
        {
            if (!reapplyPending)
                return;
            reapplyPending = false;
            Apply(loadout);
        }

        private void ApplyStats(TankStats stats)
        {
            if (movement != null)
                movement.Settings = stats.Drive;
            if (aimer != null)
                aimer.Settings = stats.Turret;
            if (gun != null)
            {
                gun.Settings = stats.Gun;
                gun.CrewFactor = stats.CrewDispersionFactor;
            }
            ApplyMachineGun(coaxialGun, stats.HasMachineGun, stats.MachineGun, stats.CrewDispersionFactor);
            ApplyMachineGun(hullGun, stats.HasHullMachineGun, stats.HullMachineGun, stats.CrewDispersionFactor);
            if (ballMount != null)
            {
                ballMount.gameObject.SetActive(stats.HasHullMachineGun);
                ballMount.Arc = stats.HullMachineGunArc;
                ballMount.TraverseSpeed = stats.HullMachineGunTraverseSpeed;
            }
            if (body != null)
                body.mass = stats.TotalMass / 1000f;
            if (damageable != null)
                damageable.Configure(stats.HitPoints, new ArmorProfile(stats.FrontArmor, stats.SideArmor, stats.RearArmor));
        }

        private static void ApplyMachineGun(TankGun machineGun, bool fitted, GunSettings settings, float crewFactor)
        {
            if (machineGun == null)
                return;
            machineGun.enabled = fitted;
            if (!fitted)
                return;
            machineGun.Settings = settings;
            machineGun.CrewFactor = crewFactor;
        }

        private void OnDied(Damageable _)
        {
            if (IsDestroyed)
                return;
            IsDestroyed = true;

            var velocity = body != null ? body.linearVelocity : Vector2.zero;
            var spin = body != null ? body.angularVelocity : 0f;

            foreach (var behaviour in GetComponentsInChildren<MonoBehaviour>())
                if (behaviour is TankInput || behaviour is TankGun || behaviour is TurretAimer || behaviour is BallMount || behaviour is TankMovement)
                    behaviour.enabled = false;

            if (body != null)
            {
                body.linearVelocity = velocity;
                body.angularVelocity = spin;
                body.linearDamping = WreckDamping;
                body.angularDamping = WreckDamping;
            }

            foreach (var sprite in GetComponentsInChildren<SpriteRenderer>())
                sprite.color *= WreckTint;

            Destroyed?.Invoke(this);
        }

        private void ApplyVisuals(TankLoadout source)
        {
            var hull = source.Hull;
            var turret = source.Turret;

            if (hullView != null)
                hullView.Apply(hull);
            if (turretView != null)
                turretView.Apply(turret);
            if (hullCollider != null)
                hullCollider.size = hull.ColliderSize;
            if (aimer != null)
                aimer.transform.localPosition = hull.TurretMount;
            if (ballMount != null)
                ballMount.transform.localPosition = hull.HullMachineGunMount;
            if (gun == null)
                return;

            gun.Muzzle.localPosition = new Vector3(0f, turret.MuzzleDistance, 0f);
            var flash = gun.MuzzleFlash;
            if (flash != null)
            {
                baseMuzzleFlashScale ??= flash.localScale;
                flash.localScale = baseMuzzleFlashScale.Value * source.MainGun.MuzzleFlashScale;
            }
        }
    }
}
