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
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private BoxCollider2D hullCollider;
        [SerializeField] private SpriteRenderer hullSprite;
        [SerializeField] private SpriteRenderer turretSprite;
        [SerializeField] private Transform muzzleFlash;

        private bool initialized;
        private bool reapplyPending;
        private Vector3? baseMuzzleFlashScale;

        /// <summary>Raised after a loadout is applied.</summary>
        public event Action<TankStats> StatsApplied;

        /// <summary>The loadout currently applied, or null. May differ from the Inspector field if that one was invalid.</summary>
        public TankLoadout Loadout { get; private set; }

        /// <summary>Stats of the applied loadout, or null before one was applied.</summary>
        public TankStats Stats { get; private set; }

        /// <summary>Applies a new loadout. Returns false and keeps the current setup if the loadout is invalid.</summary>
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
            // Sprite and transform changes aren't allowed inside OnValidate, so apply on the next frame.
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
            if (body != null)
                body.mass = stats.TotalMass / 1000f; // tonnes keep the physics numbers small
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
                // Scale relative to the prefab's flash size, so 1 means "as authored".
                baseMuzzleFlashScale ??= muzzleFlash.localScale;
                muzzleFlash.localScale = baseMuzzleFlashScale.Value * source.MainGun.MuzzleFlashScale;
            }
        }
    }
}
