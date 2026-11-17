using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PolishTanks.Tank.Modules;
using PolishTanks.UI;

namespace PolishTanks.Tests.EditMode
{
    public class LoadoutStatSheetTests
    {
        private static StatLine Row(List<StatLine> lines, string label) => lines.Single(line => line.Label == label);

        [Test]
        public void Build_HigherMass_MarksMassWorse()
        {
            var lines = LoadoutStatSheet.Build(new TankStats { TotalMass = 30000f }, new TankStats { TotalMass = 25000f }, 1f);

            Assert.That(Row(lines, ModulePanelText.Mass).Change, Is.EqualTo(StatChange.Worse));
        }

        [Test]
        public void Build_HigherFrontArmor_MarksArmorBetter()
        {
            var lines = LoadoutStatSheet.Build(new TankStats { FrontArmor = 80f }, new TankStats { FrontArmor = 50f }, 1f);

            Assert.That(Row(lines, ModulePanelText.FrontArmor).Change, Is.EqualTo(StatChange.Better));
        }

        [Test]
        public void Build_NoPreviousStats_HasNoDeltas()
        {
            var lines = LoadoutStatSheet.Build(new TankStats { FrontArmor = 80f }, null, 1f);

            Assert.That(lines.All(line => line.Change == StatChange.Same && line.Delta == string.Empty), Is.True);
        }

        [Test]
        public void Build_NullStats_ReturnsNoRows()
        {
            Assert.That(LoadoutStatSheet.Build(null, null, 1f), Is.Empty);
        }
    }
}
