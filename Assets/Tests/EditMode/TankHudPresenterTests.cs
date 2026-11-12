using NUnit.Framework;
using PolishTanks.UI;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Speedometer scale, belt readout and repair key caption of the tank HUD.</summary>
    public class TankHudPresenterTests
    {
        [Test]
        public void GaugeMaxKmh_RoundsTopSpeedUpToWholeTick()
        {
            Assert.That(TankHudPresenter.GaugeMaxKmh(6f, 6.3f), Is.EqualTo(40));
        }

        [Test]
        public void GaugeFill_Reversing_UsesSpeedMagnitude()
        {
            Assert.That(TankHudPresenter.GaugeFill(-2f, 5f, 40), Is.EqualTo(0.25f).Within(1e-5f));
        }

        [Test]
        public void GaugeFill_AboveGaugeMax_IsClampedToFull()
        {
            Assert.That(TankHudPresenter.GaugeFill(20f, 5f, 40), Is.EqualTo(1f));
        }

        [Test]
        public void BeltText_ContinuousFeed_ShowsInfinity()
        {
            Assert.That(TankHudPresenter.BeltText(0, 0), Is.EqualTo("∞"));
        }

        [Test]
        public void RepairCaption_Repairing_ReturnsInterrupt()
        {
            Assert.That(TankHudPresenter.RepairCaption(true, true), Is.EqualTo("Przerwij"));
        }

        [Test]
        public void RepairCaption_WorkWaitingAndNotRepairing_ReturnsRepair()
        {
            Assert.That(TankHudPresenter.RepairCaption(false, true), Is.EqualTo("Naprawa"));
        }

        [Test]
        public void RepairCaption_NoWork_ReturnsNull()
        {
            Assert.That(TankHudPresenter.RepairCaption(false, false), Is.Null);
        }
    }
}
