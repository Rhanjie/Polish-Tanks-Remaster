using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PolishTanks.Tank.Modules;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Boundary cases for TankStatsCalculator; the main behaviour is covered in TankStatsCalculatorTests.</summary>
    public class TankStatsCalculatorEdgeCaseTests
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

        private void ZeroAllMasses()
        {
            foreach (var module in loadout.AllModules())
                ModuleFactory.Set(module, ("mass", 0f));
        }

        [Test]
        public void Calculate_ZeroTotalMass_GivesFiniteSpeed()
        {
            ZeroAllMasses();

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(float.IsFinite(stats.Drive.maxForwardSpeed), Is.True);
        }

        [Test]
        public void Calculate_ZeroTotalMass_GivesFinitePowerToWeight()
        {
            ZeroAllMasses();

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(float.IsFinite(stats.PowerToWeight), Is.True);
        }

        [Test]
        public void Calculate_ZeroTotalMass_IsNotOverloaded()
        {
            ZeroAllMasses();

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.IsOverloaded, Is.False);
        }

        [Test]
        public void Calculate_MassExactlyAtTrackLoad_IsNotOverloaded()
        {
            ModuleFactory.Set(loadout.Tracks, ("maxLoad", 20000f)); // baseline weighs exactly 20 t

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.IsOverloaded, Is.False);
        }

        [Test]
        public void Calculate_CalledTwice_GivesSameStats()
        {
            var first = TankStatsCalculator.Calculate(loadout);
            var second = TankStatsCalculator.Calculate(loadout);

            Assert.That(second.Drive.maxForwardSpeed, Is.EqualTo(first.Drive.maxForwardSpeed));
        }

        [Test]
        public void Calculate_Equipment_CallsModifyInListOrder()
        {
            var log = new List<string>();
            var first = factory.Create<RecordingEquipmentModule>();
            var second = factory.Create<RecordingEquipmentModule>();
            first.Log = log;
            second.Log = log;
            first.Label = "first";
            second.Label = "second";
            ModuleFactory.Set(loadout, ("equipment", new UnityEngine.Object[] { first, second }));

            TankStatsCalculator.Calculate(loadout);

            Assert.That(log, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void Calculate_Equipment_ListOrderDecidesResultOfNonCommutingModifiers()
        {
            var doubler = factory.Create<RecordingEquipmentModule>();
            doubler.HitPointsMultiplier = 2f;
            var bonus = factory.Create<RecordingEquipmentModule>();
            bonus.HitPointsBonus = 100f;
            var hullHp = loadout.Hull.HitPoints;

            ModuleFactory.Set(loadout, ("equipment", new UnityEngine.Object[] { bonus, doubler }));
            var bonusFirst = TankStatsCalculator.Calculate(loadout).HitPoints;

            Assert.That(bonusFirst, Is.EqualTo((hullHp + 100f) * 2f));
        }

        [Test]
        public void Calculate_NullEquipmentEntry_IsIgnored()
        {
            var armor = factory.Create<ArmorKitModule>(("extraHitPoints", 50f));
            ModuleFactory.Set(loadout, ("equipment", new UnityEngine.Object[] { null, armor }));
            var plain = TankStatsCalculator.Calculate(factory.CreateBaseline());

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.HitPoints, Is.EqualTo(plain.HitPoints + 50f));
        }

        [Test]
        public void Validate_NullEquipmentEntries_DoNotCountTowardHullSlots()
        {
            ModuleFactory.Set(loadout.Hull, ("equipmentSlots", 1));
            var armor = factory.Create<ArmorKitModule>();
            ModuleFactory.Set(loadout, ("equipment", new UnityEngine.Object[] { null, armor, null }));

            Assert.That(TankStatsCalculator.Validate(loadout), Is.Empty);
        }

        [Test]
        public void Validate_SeveralEmptySlots_ReportsEachOne()
        {
            ModuleFactory.Set(loadout, ("hull", null), ("engine", null), ("mainGun", null));

            Assert.That(TankStatsCalculator.Validate(loadout).Count, Is.EqualTo(3));
        }

        [Test]
        public void Validate_ZeroEquipmentSlotsAndNoEquipment_IsValid()
        {
            ModuleFactory.Set(loadout.Hull, ("equipmentSlots", 0));

            Assert.That(TankStatsCalculator.Validate(loadout).Any(), Is.False);
        }
    }
}
