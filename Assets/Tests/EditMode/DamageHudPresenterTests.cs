using NUnit.Framework;
using PolishTanks.Damage;
using PolishTanks.UI;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>When the damage HUD panel is shown and what the repair key caption says.</summary>
    public class DamageHudPresenterTests
    {
        private static PartDamageChances Full => new PartDamageChances { HasTracks = true, HasTurretRing = true, VisionBlocks = 4 };

        private static float First() => 0f;

        [Test]
        public void IsVisible_NoDamageAndNoFire_ReturnsFalse()
        {
            var model = new VehicleDamageModel(Full);

            Assert.That(DamageHudPresenter.IsVisible(model, false), Is.False);
        }

        [Test]
        public void IsVisible_PartDestroyed_ReturnsTrue()
        {
            var model = new VehicleDamageModel(Full);
            model.Break(VehiclePart.LeftTrack, First);

            Assert.That(DamageHudPresenter.IsVisible(model, false), Is.True);
        }

        [Test]
        public void IsVisible_PartPatched_ReturnsTrue()
        {
            var model = new VehicleDamageModel(Full);
            var broken = model.Break(VehiclePart.LeftTrack, First);
            model.FieldRepair(broken.Value);

            Assert.That(DamageHudPresenter.IsVisible(model, false), Is.True);
        }

        [Test]
        public void IsVisible_BurningWithoutDamage_ReturnsTrue()
        {
            var model = new VehicleDamageModel(Full);

            Assert.That(DamageHudPresenter.IsVisible(model, true), Is.True);
        }

        [Test]
        public void IsVisible_NullModelAndNoFire_ReturnsFalse()
        {
            Assert.That(DamageHudPresenter.IsVisible(null, false), Is.False);
        }

        [Test]
        public void RepairCaption_Repairing_ReturnsInterrupt()
        {
            Assert.That(DamageHudPresenter.RepairCaption(true, true), Is.EqualTo("Przerwij"));
        }

        [Test]
        public void RepairCaption_WorkWaitingAndNotRepairing_ReturnsRepair()
        {
            Assert.That(DamageHudPresenter.RepairCaption(false, true), Is.EqualTo("Naprawa"));
        }

        [Test]
        public void RepairCaption_NoWork_ReturnsNull()
        {
            Assert.That(DamageHudPresenter.RepairCaption(false, false), Is.Null);
        }
    }
}
