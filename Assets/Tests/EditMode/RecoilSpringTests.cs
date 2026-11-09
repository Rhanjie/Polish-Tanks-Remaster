using NUnit.Framework;
using PolishTanks.Weapons;

namespace PolishTanks.Tests.EditMode
{
    public class RecoilSpringTests
    {
        private const float Dt = 1f / 60f;

        [Test]
        public void Kick_SetsOffset()
        {
            var spring = new RecoilSpring(0.3f);

            spring.Kick(0.4f);

            Assert.That(spring.Offset, Is.EqualTo(0.4f));
        }

        [Test]
        public void Step_ReturnsToRestWithoutGoingNegative()
        {
            var spring = new RecoilSpring(0.3f);
            spring.Kick(0.5f);

            for (var i = 0; i < 120; i++)
            {
                spring.Step(Dt);
                Assert.That(spring.Offset, Is.GreaterThanOrEqualTo(0f));
            }

            Assert.That(spring.Offset, Is.EqualTo(0f));
        }

        [Test]
        public void Step_MostlyBackWithinReturnTime()
        {
            var spring = new RecoilSpring(0.3f);
            spring.Kick(1f);

            for (var t = 0f; t < 0.3f; t += Dt)
                spring.Step(Dt);

            Assert.That(spring.Offset, Is.LessThan(0.25f));
        }

        [Test]
        public void Kick_WhileKicked_KeepsTheLargerOffset()
        {
            var spring = new RecoilSpring(0.3f);
            spring.Kick(0.5f);

            spring.Kick(0.2f);

            Assert.That(spring.Offset, Is.EqualTo(0.5f));
        }

        [Test]
        public void Kick_NonPositive_IsIgnored()
        {
            var spring = new RecoilSpring(0.3f);

            spring.Kick(-1f);

            Assert.That(spring.Offset, Is.EqualTo(0f));
        }
    }
}
