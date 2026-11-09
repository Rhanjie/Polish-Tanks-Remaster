using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>Turns a <see cref="TankLoadout"/> into the stats the tank components run on.</summary>
    public static class TankStatsCalculator
    {
        /// <summary>Top speed (units/s) per square root of power-to-weight (hp/t).</summary>
        public const float SpeedPerSqrtPowerToWeight = 1.9f;

        /// <summary>Acceleration (units/s²) per hp/t.</summary>
        public const float AccelerationPerPowerToWeight = 0.35f;

        /// <summary>Braking (units/s²) on tracks with grip 1.</summary>
        public const float BaseBraking = 12f;

        /// <summary>Power-to-weight (hp/t) where the turn rate equals the tracks' rating.</summary>
        public const float ReferencePowerToWeight = 12f;

        public const float DefaultViewRange = 9f;
        public const float MuzzleFlashDuration = 0.06f;

        /// <summary>Lists what's wrong with a loadout. Empty means it can be calculated.</summary>
        public static IReadOnlyList<string> Validate(TankLoadout loadout)
        {
            var errors = new List<string>();
            if (loadout == null)
            {
                errors.Add("Loadout is missing.");
                return errors;
            }

            if (loadout.Hull == null) errors.Add("Hull slot is empty.");
            if (loadout.Engine == null) errors.Add("Engine slot is empty.");
            if (loadout.Transmission == null) errors.Add("Transmission slot is empty.");
            if (loadout.Tracks == null) errors.Add("Tracks slot is empty.");
            if (loadout.Turret == null) errors.Add("Turret slot is empty.");
            if (loadout.MainGun == null) errors.Add("Main gun slot is empty.");

            var equipmentCount = loadout.Equipment.Count(e => e != null);
            if (loadout.Hull != null && equipmentCount > loadout.Hull.EquipmentSlots)
                errors.Add($"{equipmentCount} equipment modules fitted, hull has {loadout.Hull.EquipmentSlots} slots.");

            return errors;
        }

        /// <exception cref="InvalidOperationException">The loadout fails <see cref="Validate"/>.</exception>
        public static TankStats Calculate(TankLoadout loadout)
        {
            var errors = Validate(loadout);
            if (errors.Count > 0)
                throw new InvalidOperationException($"Invalid loadout '{(loadout != null ? loadout.name : "null")}': {string.Join(" ", errors)}");

            var hull = loadout.Hull;
            var engine = loadout.Engine;
            var transmission = loadout.Transmission;
            var tracks = loadout.Tracks;
            var turret = loadout.Turret;
            var gun = loadout.MainGun;
            var mg = loadout.CoaxialMachineGun;
            var optics = loadout.Optics;

            var stats = new TankStats
            {
                TotalMass = loadout.AllModules().Sum(m => m.Mass),
                HorsePower = engine.HorsePower,
                Cost = loadout.AllModules().Sum(m => m.Cost),
            };

            CalculateMobility(stats, transmission, tracks);
            CalculateFirepower(stats, engine, turret, gun, mg, optics);

            stats.FrontArmor = hull.FrontArmor;
            stats.SideArmor = hull.SideArmor;
            stats.RearArmor = hull.RearArmor;
            stats.TurretFrontArmor = turret.FrontArmor;
            stats.HitPoints = hull.HitPoints;
            stats.ViewRange = optics != null ? optics.ViewRange : DefaultViewRange;

            foreach (var item in loadout.Equipment)
                if (item != null)
                    item.Modify(stats);

            return stats;
        }

        private static void CalculateMobility(TankStats stats, TransmissionModule transmission, TracksModule tracks)
        {
            var tonnes = Mathf.Max(0.1f, stats.TotalMass / 1000f);
            stats.PowerToWeight = stats.HorsePower / tonnes;

            // Above the tracks' rated load the tank loses speed and pickup in proportion.
            stats.IsOverloaded = stats.TotalMass > tracks.MaxLoad;
            var overloadFactor = stats.IsOverloaded ? tracks.MaxLoad / stats.TotalMass : 1f;

            var maxSpeed = SpeedPerSqrtPowerToWeight * Mathf.Sqrt(stats.PowerToWeight)
                           * transmission.Efficiency * tracks.MaxSpeedMultiplier * overloadFactor;
            var acceleration = AccelerationPerPowerToWeight * stats.PowerToWeight
                               * transmission.AccelerationMultiplier * tracks.Grip * overloadFactor;
            var braking = BaseBraking * tracks.Grip;

            var powerTurnFactor = Mathf.Clamp(Mathf.Sqrt(stats.PowerToWeight / ReferencePowerToWeight), 0.6f, 1.4f);
            var turnRate = tracks.TurnRate * transmission.TurnRateMultiplier * powerTurnFactor;

            stats.Drive = new TankDriveSettings
            {
                maxForwardSpeed = maxSpeed,
                maxReverseSpeed = maxSpeed * transmission.ReverseSpeedRatio,
                acceleration = acceleration,
                braking = braking,
                coastDeceleration = braking * 0.5f,
                turnRate = turnRate,
                turnRateAtMaxSpeed = turnRate * tracks.TurnRateAtSpeedRatio,
                turnAcceleration = turnRate * 5f,
            };
        }

        private static void CalculateFirepower(TankStats stats, EngineModule engine, TurretModule turret,
            MainGunModule gun, CoaxialMachineGunModule mg, OpticsModule optics)
        {
            // A gun heavier than the turret is built for slows the traverse.
            var gunLoadFactor = gun.Mass > turret.MaxGunMass ? turret.MaxGunMass / gun.Mass : 1f;
            var traverseFactor = engine.TraverseDriveMultiplier * gunLoadFactor;
            stats.Turret = new TurretSettings
            {
                maxTraverseSpeed = turret.MaxTraverseSpeed * traverseFactor,
                traverseAcceleration = turret.TraverseAcceleration * traverseFactor,
                responseTime = turret.ResponseTime,
            };

            var accuracy = optics != null ? optics.AccuracyMultiplier : 1f;
            stats.Gun = new GunSettings
            {
                reloadTime = gun.ReloadTime,
                muzzleFlashDuration = MuzzleFlashDuration,
            };
            stats.GunCaliber = gun.Caliber;
            stats.MuzzleVelocity = gun.MuzzleVelocity;
            stats.GunDamage = gun.Damage;
            stats.Penetration = gun.Penetration;
            stats.GunDispersion = gun.Dispersion * accuracy;
            stats.AmmoCapacity = gun.AmmoCapacity;

            stats.HasMachineGun = mg != null;
            if (mg == null)
                return;
            stats.MachineGunFireInterval = mg.FireInterval;
            stats.MachineGunDispersion = mg.Dispersion * accuracy;
            stats.MachineGunDamage = mg.Damage;
            stats.MachineGunBeltSize = mg.BeltSize;
            stats.MachineGunBeltChangeTime = mg.BeltChangeTime;
        }
    }
}
