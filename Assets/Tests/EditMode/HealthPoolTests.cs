using NUnit.Framework;
using PolishTanks.Combat;

namespace PolishTanks.Tests.EditMode
{
    public class HealthPoolTests
    {
        [Test]
        public void TakeDamage_PastZero_DiesOnceAndStopsAtZero()
        {
            var health = new HealthPool(100f);
            var deaths = 0;
            health.Died += () => deaths++;

            health.TakeDamage(80f);
            var dealt = health.TakeDamage(80f);
            health.TakeDamage(10f);

            Assert.That(dealt, Is.EqualTo(20f));
            Assert.That(health.Current, Is.EqualTo(0f));
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void SetMax_KeepsDamageFraction()
        {
            var health = new HealthPool(100f);
            health.TakeDamage(50f);

            health.SetMax(300f);

            Assert.That(health.Current, Is.EqualTo(150f));
        }
    }
}
