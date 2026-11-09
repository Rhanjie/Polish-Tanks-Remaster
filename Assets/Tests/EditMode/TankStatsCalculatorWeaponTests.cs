using NUnit.Framework;
using PolishTanks.Tank.Modules;
using PolishTanks.Weapons;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>How module weapon fields end up in <see cref="TankStats.Gun"/> and <see cref="TankStats.MachineGun"/>.</summary>
    public class TankStatsCalculatorWeaponTests
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

        private CoaxialMachineGunModule AddMachineGun(params (string field, object value)[] values)
        {
            var mg = factory.Create<CoaxialMachineGunModule>(values);
            ModuleFactory.Set(loadout, ("coaxialMachineGun", mg));
            return mg;
        }

        [Test]
        public void Gun_ProjectileSpeed_ScalesMuzzleVelocityToWorldUnits()
        {
            ModuleFactory.Set(loadout.MainGun, ("muzzleVelocity", 500f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.Gun.projectileSpeed,
                Is.EqualTo(500f * TankStatsCalculator.ProjectileSpeedPerMuzzleVelocity).Within(1e-4f));
        }

        [Test]
        public void Gun_RecoilImpulse_ComesFromModule()
        {
            ModuleFactory.Set(loadout.MainGun, ("recoilImpulse", 12f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.Gun.recoilImpulse, Is.EqualTo(12f));
        }

        [Test]
        public void Gun_ShakeForce_ComesFromModule()
        {
            ModuleFactory.Set(loadout.MainGun, ("shakeForce", 0.7f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.Gun.shakeForce, Is.EqualTo(0.7f));
        }

        [Test]
        public void Gun_Projectile_IsTheModulesDefinition()
        {
            var definition = factory.Create<ProjectileDefinition>();
            ModuleFactory.Set(loadout.MainGun, ("projectile", definition));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.Gun.projectile, Is.SameAs(definition));
        }

        [Test]
        public void Gun_Dispersion_IsScaledByOpticsAccuracy()
        {
            var optics = factory.Create<OpticsModule>(("accuracyMultiplier", 2f));
            ModuleFactory.Set(loadout, ("optics", optics));
            ModuleFactory.Set(loadout.MainGun, ("dispersion", 0.5f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.Gun.dispersion, Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void Gun_MagazineSize_IsZeroSoMainGunIsFedContinuously()
        {
            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.Gun.magazineSize, Is.EqualTo(0));
        }

        [Test]
        public void MachineGun_MagazineSize_EqualsBeltSize()
        {
            AddMachineGun(("beltSize", 75));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.MachineGun.magazineSize, Is.EqualTo(75));
        }

        [Test]
        public void MachineGun_MagazineReloadTime_EqualsBeltChangeTime()
        {
            AddMachineGun(("beltChangeTime", 6f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.MachineGun.magazineReloadTime, Is.EqualTo(6f));
        }

        [Test]
        public void MachineGun_ReloadTime_EqualsFireInterval()
        {
            AddMachineGun(("roundsPerMinute", 600f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.MachineGun.reloadTime, Is.EqualTo(0.1f).Within(1e-5f));
        }

        [Test]
        public void MachineGun_ProjectileSpeed_ScalesMuzzleVelocityToWorldUnits()
        {
            AddMachineGun(("muzzleVelocity", 800f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.MachineGun.projectileSpeed,
                Is.EqualTo(800f * TankStatsCalculator.ProjectileSpeedPerMuzzleVelocity).Within(1e-4f));
        }

        [Test]
        public void MachineGun_RecoilImpulse_IsZero()
        {
            AddMachineGun();

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.MachineGun.recoilImpulse, Is.EqualTo(0f));
        }

        [Test]
        public void MachineGun_ShakeForce_ComesFromModule()
        {
            AddMachineGun(("shakeForce", 0.05f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.MachineGun.shakeForce, Is.EqualTo(0.05f));
        }

        [Test]
        public void MachineGun_WithoutModule_LeavesSettingsDefault()
        {
            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.MachineGun.magazineSize, Is.EqualTo(0));
            Assert.That(stats.MachineGun.projectile == null, Is.True);
        }
    }
}
