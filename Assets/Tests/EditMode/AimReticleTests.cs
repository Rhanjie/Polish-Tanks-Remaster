using NUnit.Framework;
using PolishTanks.UI;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>The world radius of the aim ring for a given distance and spread.</summary>
    public class AimReticleTests
    {
        [Test]
        public void Radius_At45Degrees_EqualsDistance()
        {
            Assert.That(AimReticle.Radius(10f, 45f), Is.EqualTo(10f).Within(1e-4f));
        }

        [Test]
        public void Radius_WithZeroSpread_IsZero()
        {
            Assert.That(AimReticle.Radius(10f, 0f), Is.EqualTo(0f));
        }

        [Test]
        public void Radius_WithNegativeDistance_IsZero()
        {
            Assert.That(AimReticle.Radius(-5f, 10f), Is.EqualTo(0f));
        }

        [Test]
        public void Radius_WithSpreadPastRightAngle_StaysFinite()
        {
            Assert.That(float.IsInfinity(AimReticle.Radius(10f, 120f)), Is.False);
        }
    }
}
