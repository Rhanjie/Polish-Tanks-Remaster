using NUnit.Framework;
using PolishTanks.Tank;

namespace PolishTanks.Tests.EditMode
{
    public class HitStaggerTests
    {
        [Test]
        public void Duration_BigGunOnLightTank_IsLongAndCapped()
        {
            Assert.That(HitStagger.Duration(24f, 16f), Is.EqualTo(HitStagger.MaxSeconds));
        }

        [Test]
        public void Duration_LightGunOnHeavyTank_IsIgnored()
        {
            Assert.That(HitStagger.Duration(3f, 51f), Is.EqualTo(0f));
        }

        [Test]
        public void Duration_GrowsWithImpulse()
        {
            Assert.That(HitStagger.Duration(20f, 28f), Is.GreaterThan(HitStagger.Duration(10f, 28f)));
        }
    }
}
