using System.Linq;
using NUnit.Framework;
using PolishTanks.Tank.Modules;

namespace PolishTanks.Tests.EditMode
{
    public class LoadoutBuilderTests
    {
        private ModuleFactory factory;
        private TankLoadout baseline;

        [SetUp]
        public void SetUp()
        {
            factory = new ModuleFactory();
            baseline = factory.CreateBaseline();
        }

        [TearDown]
        public void TearDown() => factory.Dispose();

        private HullModule Hull(bool mount, int equipmentSlots) =>
            factory.Create<HullModule>(("hasHullMachineGunMount", mount), ("equipmentSlots", equipmentSlots));

        [Test]
        public void Build_FromLoadout_KeepsSameModules()
        {
            var built = new LoadoutBuilder(baseline).Build();

            Assert.That(built.AllModules().ToList(), Is.EqualTo(baseline.AllModules().ToList()));
            LoadoutBuilder.Release(built);
        }

        [Test]
        public void Fit_HullWithoutMount_DropsHullMachineGun()
        {
            var builder = new LoadoutBuilder(baseline).Fit(Hull(true, 2)).Fit(factory.Create<HullMachineGunModule>());

            builder.Fit(Hull(false, 2));

            Assert.That(builder.Get(ModuleSlot.HullMachineGun), Is.Null);
        }

        [Test]
        public void Fit_HullWithFewerSlots_TrimsEquipmentToSlotCount()
        {
            var first = factory.Create<RecordingEquipmentModule>();
            var second = factory.Create<RecordingEquipmentModule>();
            var builder = new LoadoutBuilder(baseline).Fit(first).Fit(second);

            builder.Fit(Hull(false, 1));

            Assert.That(builder.Equipment, Is.EqualTo(new[] { first }));
        }

        [Test]
        public void CanFit_HullMachineGunOnHullWithoutMount_ReturnsFalse()
        {
            var builder = new LoadoutBuilder(baseline);

            Assert.That(builder.CanFit(factory.Create<HullMachineGunModule>()), Is.False);
        }

        [Test]
        public void CanFit_EquipmentWhenSlotsAreFull_ReturnsFalse()
        {
            var builder = new LoadoutBuilder(baseline)
                .Fit(factory.Create<RecordingEquipmentModule>())
                .Fit(factory.Create<RecordingEquipmentModule>());

            Assert.That(builder.CanFit(factory.Create<RecordingEquipmentModule>()), Is.False);
        }

        [Test]
        public void Validate_RequiredSlotRemoved_ReportsError()
        {
            var builder = new LoadoutBuilder(baseline).Remove(ModuleSlot.Engine);

            Assert.That(builder.Validate(), Is.Not.Empty);
        }

        [Test]
        public void Release_RuntimeLoadout_DestroysIt()
        {
            var built = new LoadoutBuilder(baseline).Build();

            LoadoutBuilder.Release(built);

            Assert.That(built == null, Is.True);
        }

        [Test]
        public void Release_AssetLoadout_LeavesItAlive()
        {
            LoadoutBuilder.Release(baseline);

            Assert.That(baseline == null, Is.False);
        }
    }
}
