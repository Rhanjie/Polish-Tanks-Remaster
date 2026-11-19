using NUnit.Framework;
using PolishTanks.Weapons;

namespace PolishTanks.Tests.EditMode
{
    public class BlastPowerTests
    {
        [Test]
        public void Power_AtTheCentre_IsFull()
        {
            Assert.That(Blast.Power(0f, 3f), Is.EqualTo(1f));
        }

        [Test]
        public void Power_AtTheRadius_IsTheEdgePower()
        {
            Assert.That(Blast.Power(3f, 3f), Is.EqualTo(Blast.EdgePower).Within(1e-4f));
        }

        [Test]
        public void Power_OutsideTheRadius_IsZero()
        {
            Assert.That(Blast.Power(3.1f, 3f), Is.EqualTo(0f));
        }

        [Test]
        public void Power_ZeroRadius_IsZero()
        {
            Assert.That(Blast.Power(0f, 0f), Is.EqualTo(0f));
        }
    }
}
