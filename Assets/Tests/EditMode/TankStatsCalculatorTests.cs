using System;
using NUnit.Framework;
using PolishTanks.Tank.Modules;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class TankStatsCalculatorTests
    {
        private ModuleFactory factory;
        private TankLoadout loadout;

        [SetUp]
        public void SetUp()
        {
            factory = new ModuleFactory();
            loadout = factory.CreateBaseline();
        }

        [TearDown]
        public void TearDown() => factory.Dispose();

        [Test]
        public void Calculate_SumsMassAndCostOfAllModules()
        {
            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.TotalMass, Is.EqualTo(20000f));
            Assert.That(stats.PowerToWeight, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(stats.Cost, Is.EqualTo(600));
        }

        [Test]
        public void Calculate_BaselineSpeedFollowsPowerToWeightFormula()
        {
            var stats = TankStatsCalculator.Calculate(loadout);

            var expected = TankStatsCalculator.SpeedPerSqrtPowerToWeight * Mathf.Sqrt(15f);
            Assert.That(stats.Drive.maxForwardSpeed, Is.EqualTo(expected).Within(1e-4f));
            Assert.That(stats.Drive.maxReverseSpeed, Is.EqualTo(expected * 0.4f).Within(1e-4f));
        }

        [Test]
        public void HeavierHull_IsSlower()
        {
            var before = TankStatsCalculator.Calculate(loadout);

            ModuleFactory.Set(loadout.Hull, ("mass", 24000f));
            var after = TankStatsCalculator.Calculate(loadout);

            Assert.That(after.Drive.maxForwardSpeed, Is.LessThan(before.Drive.maxForwardSpeed));
            Assert.That(after.Drive.acceleration, Is.LessThan(before.Drive.acceleration));
        }

        [Test]
        public void StrongerEngine_IsFaster()
        {
            var before = TankStatsCalculator.Calculate(loadout);

            ModuleFactory.Set(loadout.Engine, ("horsePower", 600f));
            var after = TankStatsCalculator.Calculate(loadout);

            Assert.That(after.Drive.maxForwardSpeed, Is.GreaterThan(before.Drive.maxForwardSpeed));
            Assert.That(after.Drive.acceleration, Is.GreaterThan(before.Drive.acceleration));
        }

        [Test]
        public void OverloadedTracks_PenalizeSpeed()
        {
            var before = TankStatsCalculator.Calculate(loadout);

            ModuleFactory.Set(loadout.Tracks, ("maxLoad", 10000f));
            var after = TankStatsCalculator.Calculate(loadout);

            Assert.That(after.IsOverloaded, Is.True);
            Assert.That(after.Drive.maxForwardSpeed, Is.EqualTo(before.Drive.maxForwardSpeed * 0.5f).Within(1e-4f));
        }

        [Test]
        public void GunHeavierThanTurretRating_SlowsTraverse()
        {
            ModuleFactory.Set(loadout.MainGun, ("mass", 5000f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.Turret.maxTraverseSpeed, Is.EqualTo(90f * 2500f / 5000f).Within(1e-4f));
        }

        [Test]
        public void PoweredTraverseDrive_SpeedsUpTurret()
        {
            ModuleFactory.Set(loadout.Engine, ("traverseDriveMultiplier", 1.5f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.Turret.maxTraverseSpeed, Is.EqualTo(135f).Within(1e-4f));
        }

        [Test]
        public void Optics_SetViewRangeAndScaleDispersion()
        {
            var optics = factory.Create<OpticsModule>(("viewRange", 12f), ("accuracyMultiplier", 0.5f));
            ModuleFactory.Set(loadout, ("optics", optics));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.ViewRange, Is.EqualTo(12f));
            Assert.That(stats.GunDispersion, Is.EqualTo(0.25f).Within(1e-5f));
        }

        [Test]
        public void NoOptionalModules_UsesDefaults()
        {
            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.ViewRange, Is.EqualTo(TankStatsCalculator.DefaultViewRange));
            Assert.That(stats.HasMachineGun, Is.False);
        }

        [Test]
        public void MachineGun_FireIntervalFromRoundsPerMinute()
        {
            var mg = factory.Create<CoaxialMachineGunModule>(("roundsPerMinute", 600f));
            ModuleFactory.Set(loadout, ("coaxialMachineGun", mg));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.HasMachineGun, Is.True);
            Assert.That(stats.MachineGunFireInterval, Is.EqualTo(0.1f).Within(1e-5f));
        }

        [Test]
        public void Equipment_ModifiesStatsAndAddsMass()
        {
            var armor = factory.Create<ArmorKitModule>(("mass", 1000f), ("extraFrontArmor", 30f), ("extraHitPoints", 50f));
            var rack = factory.Create<AmmoRackModule>(("mass", 0f), ("extraShells", 10), ("reloadTimeMultiplier", 0.8f));
            ModuleFactory.Set(loadout, ("equipment", new UnityEngine.Object[] { armor, rack }));
            var baseline = factory.CreateBaseline();
            var plain = TankStatsCalculator.Calculate(baseline);

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.TotalMass, Is.EqualTo(plain.TotalMass + 1000f));
            Assert.That(stats.FrontArmor, Is.EqualTo(plain.FrontArmor + 30f));
            Assert.That(stats.HitPoints, Is.EqualTo(plain.HitPoints + 50f));
            Assert.That(stats.AmmoCapacity, Is.EqualTo(plain.AmmoCapacity + 10));
            Assert.That(stats.Gun.reloadTime, Is.EqualTo(1.6f).Within(1e-5f));
        }

        [Test]
        public void Validate_ValidLoadout_HasNoErrors()
        {
            Assert.That(TankStatsCalculator.Validate(loadout), Is.Empty);
        }

        [Test]
        public void Validate_EmptyRequiredSlot_NamesTheSlot()
        {
            ModuleFactory.Set(loadout, ("engine", null));

            var errors = TankStatsCalculator.Validate(loadout);

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0], Does.Contain("Engine"));
        }

        [Test]
        public void Validate_TooMuchEquipment_ReportsSlotCount()
        {
            var a = factory.Create<ArmorKitModule>();
            var b = factory.Create<ArmorKitModule>();
            var c = factory.Create<ArmorKitModule>();
            ModuleFactory.Set(loadout, ("equipment", new UnityEngine.Object[] { a, b, c }));

            var errors = TankStatsCalculator.Validate(loadout);

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0], Does.Contain("2 slots"));
        }

        [Test]
        public void Validate_NullLoadout_ReportsIt()
        {
            Assert.That(TankStatsCalculator.Validate(null), Is.Not.Empty);
        }

        [Test]
        public void Calculate_InvalidLoadout_Throws()
        {
            ModuleFactory.Set(loadout, ("mainGun", null));

            Assert.Throws<InvalidOperationException>(() => TankStatsCalculator.Calculate(loadout));
        }
    }
}
