using System;
using PolishTanks.Tank.Modules;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Builds the tank from a <see cref="TankLoadout"/>: computes its stats and pushes them
    /// into movement, turret and gun, and swaps sprites, collider and mass to match the modules.
    /// </summary>
    public class TankController : MonoBehaviour
    {
        [SerializeField] private TankLoadout loadout;

        [Header("Parts")]
        [SerializeField] private TankMovement movement;
        [SerializeField] private TurretAimer aimer;
        [SerializeField] private TankGun gun;
        [SerializeField] private TankGun coaxialGun;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private BoxCollider2D hullCollider;
        [SerializeField] private SpriteRenderer hullSprite;
        [SerializeField] private SpriteRenderer turretSprite;
        [SerializeField] private Transform muzzleFlash;

        private bool initialized;
        private bool reapplyPending;
        private Vector3? baseMuzzleFlashScale;

        public event Action<TankStats> StatsApplied;

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

            Loadout = newLoadout;
            loadout = newLoadout;
            Stats = TankStatsCalculator.Calculate(newLoadout);
            ApplyStats(Stats);
            ApplyVisuals(newLoadout);
            StatsApplied?.Invoke(Stats);
            return true;
        }

        private void Awake()
        {
            initialized = true;
            if (loadout != null)
                Apply(loadout);
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
                gun.Settings = stats.Gun;
            if (coaxialGun != null)
            {
                coaxialGun.enabled = stats.HasMachineGun;
                if (stats.HasMachineGun)
                    coaxialGun.Settings = stats.MachineGun;
            }
            if (body != null)
                body.mass = stats.TotalMass / 1000f;
        }

        private void ApplyVisuals(TankLoadout source)
        {
            var hull = source.Hull;
            var turret = source.Turret;

            if (hullSprite != null)
            {
                if (hull.Sprite != null)
                    hullSprite.sprite = hull.Sprite;
                hullSprite.transform.localScale = Vector3.one * hull.VisualScale;
            }

            if (hullCollider != null)
                hullCollider.size = hull.ColliderSize;

            if (aimer != null && aimer.Turret != null)
                aimer.Turret.localPosition = hull.TurretMount;

            if (turretSprite != null)
            {
                if (turret.Sprite != null)
                    turretSprite.sprite = turret.Sprite;
                turretSprite.transform.localScale = Vector3.one * turret.VisualScale;
            }

            if (gun != null && gun.Muzzle != null)
                gun.Muzzle.localPosition = new Vector3(0f, turret.MuzzleDistance, 0f);

            if (muzzleFlash != null)
            {
                baseMuzzleFlashScale ??= muzzleFlash.localScale;
                muzzleFlash.localScale = baseMuzzleFlashScale.Value * source.MainGun.MuzzleFlashScale;
            }
        }
    }
}
