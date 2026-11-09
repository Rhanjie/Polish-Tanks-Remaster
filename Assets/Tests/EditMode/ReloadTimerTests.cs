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
    }
}
