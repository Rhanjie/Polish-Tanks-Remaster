using NUnit.Framework;
using PolishTanks.Tank.Modules;

namespace PolishTanks.Tests.EditMode
{
    public class ModuleCatalogTests
    {
        private ModuleFactory factory;
        private ModuleCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            factory = new ModuleFactory();
            catalog = factory.Create<ModuleCatalog>();
        }

        [TearDown]
        public void TearDown() => factory.Dispose();

        [Test]
        public void Find_DisplayNameInOtherCase_ReturnsModule()
        {
            var engine = factory.Create<EngineModule>(("displayName", "Diesel V12"));
            catalog.SetContents(new TankModule[] { engine }, new TankLoadout[0]);

            Assert.That(catalog.Find("diesel v12"), Is.SameAs(engine));
        }

        [Test]
        public void Find_AssetNameInOtherCase_ReturnsModule()
        {
            var engine = factory.Create<EngineModule>(("displayName", "Diesel V12"));
            catalog.SetContents(new TankModule[] { engine }, new TankLoadout[0]);

            Assert.That(catalog.Find("ENGINEMODULE"), Is.SameAs(engine));
        }

        [Test]
        public void ForSlot_MixedModules_ReturnsOnlyThatSlot()
        {
            var engine = factory.Create<EngineModule>();
            var hull = factory.Create<HullModule>();
            catalog.SetContents(new TankModule[] { engine, hull, null }, new TankLoadout[0]);

            Assert.That(catalog.ForSlot(ModuleSlot.Engine), Is.EqualTo(new[] { engine }));
        }
    }
}
