using NUnit.Framework;
using PolishTanks.AI;

namespace PolishTanks.Tests.EditMode
{
    public class PerceptionTests
    {
        [Test]
        public void InReach_WithinSightRange_IsTrueEvenWhenNotHunting()
        {
            Assert.That(Perception.InReach(20f, 20f, 2.5f, false), Is.True);
        }

        [Test]
        public void InReach_BeyondSightRangeAndNotHunting_IsFalse()
        {
            Assert.That(Perception.InReach(21f, 20f, 2.5f, false), Is.False);
        }

        [Test]
        public void InReach_BeyondSightRangeButHuntingWithinPursuit_IsTrue()
        {
            Assert.That(Perception.InReach(49f, 20f, 2.5f, true), Is.True);
        }

        [Test]
        public void InReach_HuntingBeyondPursuitRange_IsFalse()
        {
            Assert.That(Perception.InReach(51f, 20f, 2.5f, true), Is.False);
        }
    }
}
