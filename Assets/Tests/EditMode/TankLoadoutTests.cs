using System.Linq;
using NUnit.Framework;
using PolishTanks.Tank.Modules;

namespace PolishTanks.Tests.EditMode
{
    public class TankLoadoutTests
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
        public void AllModules_BaselineWithoutOptionalParts_ReturnsSixRequiredModules()
        {
            Assert.That(loadout.AllModules().Count(), Is.EqualTo(6));
        }

        [Test]
        public void AllModules_EmptyRequiredSlot_IsSkipped()
        {
            ModuleFactory.Set(loadout, ("engine", null));

            Assert.That(loadout.AllModules().Count(), Is.EqualTo(5));
        }

        [Test]
        public void AllModules_NullEntryInEquipment_IsSkipped()
        {
            var armor = factory.Create<ArmorKitModule>();
            ModuleFactory.Set(loadout, ("equipment", new UnityEngine.Object[] { null, armor }));

            Assert.That(loadout.AllModules().Count(), Is.EqualTo(7));
        }

        [Test]
        public void AllModules_WithEquipment_ListsEquipmentAfterFixedSlots()
        {
            var armor = factory.Create<ArmorKitModule>();
            ModuleFactory.Set(loadout, ("equipment", new UnityEngine.Object[] { armor }));

            Assert.That(loadout.AllModules().Last(), Is.SameAs(armor));
        }
    }
}
