using System;
using System.Collections.Generic;
using PolishTanks.Combat;
using PolishTanks.Damage;
using PolishTanks.Weapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Knocks out tank parts when hits land, using the break chances from the fitted modules, and pushes the result
    /// into the drive and turret. A track snapping at speed drags the hull round toward the dead side; a destroyed
    /// engine stops the tank and starts a fire that eats hit points until it burns out. Rounds below the settings'
    /// minimum calibre never damage parts. A loadout with a different part layout keeps the damage of shared parts.
    /// </summary>
    [RequireComponent(typeof(TankController), typeof(Damageable), typeof(TankMovement))]
    public class ModuleDamage : MonoBehaviour
    {
        private const float YankMinSpeed = 0.2f;

        private static readonly Func<float> Dice = () => Random.value;

        [SerializeField] private ModuleDamageSettings settings;
        [SerializeField] private ParticleSystem fireEffect;

        private readonly List<VehiclePart> rolled = new List<VehiclePart>();
        private readonly FireState fire = new FireState();
        private TankController controller;
        private Damageable damageable;
        private TankMovement movement;
        private TurretAimer aimer;
        private Rigidbody2D body;
        private BoxCollider2D hullCollider;
        private PartDamageChances chances;
        private bool ownsSettings;

        public event Action<PartId, PartCondition> PartChanged;
        public event Action<bool> FireChanged;
        public event Action LayoutChanged;

        public VehicleDamageModel Model { get; private set; }
        public bool IsBurning => fire.IsBurning;
        public float FireRemaining => fire.Remaining;
        public ModuleDamageSettings Settings => settings;

        public PartId? Break(VehiclePart part)
        {
            if (Model == null || controller.IsDestroyed)
                return null;
            var speed = movement.CurrentSpeed;
            var broken = Model.Break(part, Dice);
            if (broken == null)
                return null;

            if (part == VehiclePart.LeftTrack || part == VehiclePart.RightTrack)
                Yank(part == VehiclePart.LeftTrack, speed);
            if (part == VehiclePart.Engine)
                Ignite();
            return broken;
        }

        public bool FieldRepair(PartId id)
        {
            if (Model == null || !Model.FieldRepair(id))
                return false;
            if (id.Part == VehiclePart.Engine)
                Extinguish();
            return true;
        }

        public void Extinguish()
        {
            if (!fire.IsBurning)
                return;
            fire.Extinguish();
            UpdateFireEffect();
        }

        private void Awake()
        {
            controller = GetComponent<TankController>();
            damageable = GetComponent<Damageable>();
            movement = GetComponent<TankMovement>();
            body = GetComponent<Rigidbody2D>();
            hullCollider = GetComponent<BoxCollider2D>();
            aimer = GetComponentInChildren<TurretAimer>(true);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ModuleDamageSettings>();
                ownsSettings = true;
            }

            controller.StatsApplied += OnStatsApplied;
            controller.Destroyed += OnTankDestroyed;
            damageable.Hit += OnHit;
            if (controller.Stats != null)
                OnStatsApplied(controller.Stats);
            UpdateFireEffect();
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.StatsApplied -= OnStatsApplied;
                controller.Destroyed -= OnTankDestroyed;
            }
            if (damageable != null)
                damageable.Hit -= OnHit;
            if (ownsSettings)
                Destroy(settings);
        }

        private void Update()
        {
            if (!fire.IsBurning)
                return;
            var ticks = fire.Tick(Time.deltaTime, settings.FireTickInterval);
            if (ticks > 0 && !damageable.IsDead)
                damageable.ApplyDamage(ticks * settings.FireTickDamage * damageable.MaxHitPoints);
            if (!fire.IsBurning)
                UpdateFireEffect();
        }

        private void OnStatsApplied(Modules.TankStats stats)
        {
            chances = stats.PartChances;
            if (Model != null && SameLayout(Model, chances))
                return;
            var previous = Model;
            Model = new VehicleDamageModel(chances);
            if (previous != null)
            {
                previous.Changed -= OnPartChanged;
                Model.CopyConditionsFrom(previous);
            }
            Model.Changed += OnPartChanged;
            PushEffects();
            if (previous != null)
                LayoutChanged?.Invoke();
        }

        private void OnHit(PenetrationResult result, HitOutcome outcome, float dealt)
        {
            if (Model == null || damageable.IsDead || (outcome != HitOutcome.Penetrated && outcome != HitOutcome.Blocked))
                return;
            if (damageable.LastCaliber < settings.MinCaliber)
                return;

            var leftSide = Vector2.Dot(damageable.LastFace, transform.right) < 0f;
            PartHitRules.Roll(result.Side, leftSide, outcome == HitOutcome.Penetrated, chances,
                settings.NonPenetratingTrackFactor, Dice, rolled);
            foreach (var part in rolled)
                Break(part);
        }

        private void OnPartChanged(PartId id, PartCondition condition)
        {
            PushEffects();
            PartChanged?.Invoke(id, condition);
        }

        private void OnTankDestroyed(TankController _)
        {
            if (!fire.IsBurning)
                return;
            fire.Extinguish();
            UpdateFireEffect();
        }

        private void PushEffects()
        {
            movement.Condition = new DriveCondition(Model.EnginePower, Model.LeftTrack, Model.RightTrack, settings.CrippledCrawl);
            if (aimer != null)
                aimer.TraverseFactor = Model.TurretTraverse;
        }

        private void Yank(bool leftTrack, float speed)
        {
            if (Mathf.Abs(speed) < YankMinSpeed || body == null)
                return;
            var halfWidth = hullCollider != null ? hullCollider.size.x * 0.5f : 1f;
            Vector2 deadSide = transform.TransformPoint(new Vector3(leftTrack ? -halfWidth : halfWidth, 0f, 0f));
            var drag = -(Vector2)transform.up * (Mathf.Sign(speed) * settings.TrackYank * Mathf.Abs(speed) * body.mass);
            movement.AddImpulse(drag, deadSide);
        }

        private void Ignite()
        {
            fire.Ignite(settings.FireDuration, settings.FireTickInterval);
            UpdateFireEffect();
        }

        private void UpdateFireEffect()
        {
            FireChanged?.Invoke(fire.IsBurning);
            if (fireEffect == null)
                return;
            if (fire.IsBurning)
                fireEffect.Play(true);
            else
                fireEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private static bool SameLayout(VehicleDamageModel model, in PartDamageChances layout) =>
            model.VisionBlocks == layout.VisionBlocks
            && model.Has(new PartId(VehiclePart.LeftTrack)) == layout.HasTracks
            && model.Has(new PartId(VehiclePart.TurretRing)) == layout.HasTurretRing;
    }
}
