using NUnit.Framework;
using PolishTanks.Tank.Modules;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Hull machine gun validation and stats, and how optics accuracy scales the spread fields.</summary>
    public class TankStatsCalculatorHullMachineGunTests
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

        private HullMachineGunModule AddHullMachineGun(params (string field, object value)[] values)
        {
            var mg = factory.Create<HullMachineGunModule>(values);
            ModuleFactory.Set(loadout, ("hullMachineGun", mg));
            return mg;
        }

        [Test]
        public void Validate_HullMachineGunOnHullWithoutMount_Fails()
        {
            AddHullMachineGun();

            var errors = TankStatsCalculator.Validate(loadout);

            Assert.That(errors, Has.Some.Contain("no mount"));
        }

        [Test]
        public void Validate_HullMachineGunOnHullWithMount_Passes()
        {
            ModuleFactory.Set(loadout.Hull, ("hasHullMachineGunMount", true));
            AddHullMachineGun();

            var errors = TankStatsCalculator.Validate(loadout);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void HullMachineGun_ArcAndTraverseSpeed_ComeFromModule()
        {
            ModuleFactory.Set(loadout.Hull, ("hasHullMachineGunMount", true));
            AddHullMachineGun(("arc", 20f), ("traverseSpeed", 60f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That((stats.HullMachineGunArc, stats.HullMachineGunTraverseSpeed), Is.EqualTo((20f, 60f)));
        }

        [Test]
        public void HullMachineGun_Settings_FollowTheModuleBelt()
        {
            ModuleFactory.Set(loadout.Hull, ("hasHullMachineGunMount", true));
            AddHullMachineGun(("beltSize", 90));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.HullMachineGun.magazineSize, Is.EqualTo(90));
        }

        [Test]
        public void HullMachineGun_WithoutModule_IsNotFlagged()
        {
            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.HasHullMachineGun, Is.False);
        }

        [Test]
        public void Gun_SpreadFields_AreScaledByOpticsAccuracy()
        {
            var optics = factory.Create<OpticsModule>(("accuracyMultiplier", 2f));
            ModuleFactory.Set(loadout, ("optics", optics));
            ModuleFactory.Set(loadout.MainGun, ("dispersion", 0.5f), ("shotDispersion", 1.5f), ("aimTime", 3f));

            var gun = TankStatsCalculator.Calculate(loadout).Gun;

            Assert.That(gun.maxDispersion, Is.EqualTo((0.5f + TankStatsCalculator.MaxDispersionBloom) * 2f).Within(1e-4f));
            Assert.That(gun.movingDispersion, Is.EqualTo(TankStatsCalculator.MovingDispersion * 2f).Within(1e-5f));
            Assert.That(gun.turningDispersion, Is.EqualTo(TankStatsCalculator.TurningDispersion * 2f).Within(1e-5f));
            Assert.That(gun.shotDispersion, Is.EqualTo(3f).Within(1e-5f));
            Assert.That(gun.aimTime, Is.EqualTo(3f));
        }
    }
}
