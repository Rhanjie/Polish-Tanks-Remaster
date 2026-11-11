using System;
using System.Collections.Generic;
using System.Linq;
using PolishTanks.Damage;
using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>Turns a <see cref="TankLoadout"/> into the stats the tank components run on.</summary>
    public static class TankStatsCalculator
    {
        public const float SpeedPerSqrtPowerToWeight = 1.9f;

        public const float AccelerationPerPowerToWeight = 0.35f;

        public const float BaseBraking = 12f;

        public const float ReferencePowerToWeight = 16f;

        public const float BaseImpulseLinearDecay = 4f;
        public const float BaseImpulseAngularDecay = 6f;

        public const float DefaultViewRange = 9f;
        public const int DefaultVisionBlocks = 4;
        public const float DefaultVisionBlockBreakChance = 0.3f;
        public const float MuzzleFlashDuration = 0.06f;
        public const float MachineGunFlashDuration = 0.03f;

        public const float ProjectileSpeedPerMuzzleVelocity = 0.06f;

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
            stats.PartChances = new PartDamageChances
            {
                HasTracks = true,
                HasTurretRing = true,
                VisionBlocks = optics != null ? optics.VisionBlocks : DefaultVisionBlocks,
                TrackBreak = tracks.BreakChance,
                EngineBreak = 1f - engine.Reliability,
                TurretRingJam = turret.RingJamChance,
                VisionBlockBreak = optics != null ? optics.BlockBreakChance : DefaultVisionBlockBreakChance,
            };

            foreach (var item in loadout.Equipment)
                if (item != null)
                    item.Modify(stats);

            return stats;
        }

        private static void CalculateMobility(TankStats stats, TransmissionModule transmission, TracksModule tracks)
        {
            var tonnes = Mathf.Max(0.1f, stats.TotalMass / 1000f);
            stats.PowerToWeight = stats.HorsePower / tonnes;

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
                impulseLinearDecay = BaseImpulseLinearDecay * tracks.Grip,
                impulseAngularDecay = BaseImpulseAngularDecay * tracks.Grip,
            };
        }

        private static void CalculateFirepower(TankStats stats, EngineModule engine, TurretModule turret,
            MainGunModule gun, CoaxialMachineGunModule mg, OpticsModule optics)
        {
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
                smokeParticles = gun.MuzzleSmoke,
                projectile = gun.Projectile,
                projectileSpeed = gun.MuzzleVelocity * ProjectileSpeedPerMuzzleVelocity,
                dispersion = gun.Dispersion * accuracy,
                damage = gun.Damage,
                caliber = gun.Caliber,
                penetrationMin = gun.PenetrationMin,
                penetrationMax = gun.PenetrationMax,
                impactImpulse = gun.ImpactImpulse,
                recoilImpulse = gun.RecoilImpulse,
                shakeForce = gun.ShakeForce,
                barrelKick = gun.BarrelKick,
                barrelReturnTime = gun.BarrelReturnTime,
            };
            stats.GunCaliber = gun.Caliber;
            stats.MuzzleVelocity = gun.MuzzleVelocity;
            stats.GunDamage = gun.Damage;
            stats.PenetrationMin = gun.PenetrationMin;
            stats.PenetrationMax = gun.PenetrationMax;
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
            stats.MachineGun = new GunSettings
            {
                reloadTime = mg.FireInterval,
                muzzleFlashDuration = MachineGunFlashDuration,
                projectile = mg.Projectile,
                projectileSpeed = mg.MuzzleVelocity * ProjectileSpeedPerMuzzleVelocity,
                dispersion = stats.MachineGunDispersion,
                damage = mg.Damage,
                caliber = mg.Caliber,
                penetrationMin = mg.PenetrationMin,
                penetrationMax = mg.PenetrationMax,
                impactImpulse = mg.ImpactImpulse,
                shakeForce = mg.ShakeForce,
                barrelReturnTime = 0.1f,
                magazineSize = mg.BeltSize,
                magazineReloadTime = mg.BeltChangeTime,
            };
        }
    }
}
