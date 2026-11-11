using NUnit.Framework;
using PolishTanks.Tank.Modules;

namespace PolishTanks.Tests.EditMode
{
    public class TankStatsCalculatorPartChancesTests
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
        public void Calculate_EngineReliability_EngineBreakIsItsComplement()
        {
            ModuleFactory.Set(loadout.Engine, ("reliability", 0.8f));

            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.PartChances.EngineBreak, Is.EqualTo(0.2f).Within(1e-5f));
        }

        [Test]
        public void Calculate_NoOptics_UsesDefaultVisionBlockCount()
        {
            var stats = TankStatsCalculator.Calculate(loadout);

            Assert.That(stats.PartChances.VisionBlocks, Is.EqualTo(TankStatsCalculator.DefaultVisionBlocks));
        }
    }
}
