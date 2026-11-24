using NUnit.Framework;
using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    public class FireStateTests
    {
        [Test]
        public void Tick_AfterDuration_FireGoesOut()
        {
            var fire = new FireState();
            fire.Ignite(2f, 0.5f);

            fire.Tick(2f, 0.5f);

            Assert.That(fire.IsBurning, Is.False);
        }

        [Test]
        public void Tick_PastDuration_OnlyCountsTicksWhileBurning()
        {
            var fire = new FireState();
            fire.Ignite(2f, 1f);

            Assert.That(fire.Tick(10f, 1f), Is.EqualTo(2));
        }

        [Test]
        public void Tick_OverSeveralIntervals_ReturnsOneTickPerInterval()
        {
            var fire = new FireState();
            fire.Ignite(10f, 1f);

            Assert.That(fire.Tick(3.5f, 1f), Is.EqualTo(3));
        }

        [Test]
        public void Tick_NotBurning_ReturnsZero()
        {
            Assert.That(new FireState().Tick(5f, 1f), Is.EqualTo(0));
        }

        [Test]
        public void Ignite_WhileBurning_KeepsTickRhythm()
        {
            var fire = new FireState();
            fire.Ignite(10f, 1f);
            fire.Tick(0.6f, 1f);

            fire.Ignite(10f, 1f);

            Assert.That(fire.Tick(0.5f, 1f), Is.EqualTo(1));
        }

        [Test]
        public void Ignite_WhileBurning_ExtendsBurnTime()
        {
            var fire = new FireState();
            fire.Ignite(2f, 1f);
            fire.Tick(1.5f, 1f);

            fire.Ignite(2f, 1f);

            Assert.That(fire.Remaining, Is.EqualTo(2f));
        }
    }
}
