using NUnit.Framework;
using PolishTanks.Tank;

namespace PolishTanks.Tests.EditMode
{
    public class ReloadTimerTests
    {
        [Test]
        public void NewTimer_IsReady()
        {
            var timer = new ReloadTimer(1f);

            Assert.That(timer.IsReady, Is.True);
            Assert.That(timer.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void TryConsume_StartsReload()
        {
            var timer = new ReloadTimer(1f);

            Assert.That(timer.TryConsume(), Is.True);
            Assert.That(timer.IsReady, Is.False);
            Assert.That(timer.TryConsume(), Is.False);
        }

        [Test]
        public void Tick_PastReloadTime_IsReadyAgain()
        {
            var timer = new ReloadTimer(1f);
            timer.TryConsume();

            timer.Tick(0.5f);
            Assert.That(timer.Progress, Is.EqualTo(0.5f).Within(1e-5f));

            timer.Tick(0.6f);
            Assert.That(timer.IsReady, Is.True);
        }

        [Test]
        public void HeldTrigger_KeepsFireRateIndependentOfFrameRate()
        {
            const float interval = 60f / 900f;
            const float dt = 1f / 50f;
            var timer = new ReloadTimer(interval);
            var shots = 0;

            for (var frame = 0; frame < 500; frame++)
            {
                timer.Tick(dt);
                while (timer.TryConsume())
                    shots++;
            }

            Assert.That(shots, Is.InRange(149, 151));
        }

        [Test]
        public void IdleTime_IsNotBanked()
        {
            var timer = new ReloadTimer(1f);
            timer.TryConsume();
            for (var i = 0; i < 100; i++)
                timer.Tick(0.1f);

            Assert.That(timer.TryConsume(), Is.True);
            Assert.That(timer.TryConsume(), Is.False);
        }
    }
}
