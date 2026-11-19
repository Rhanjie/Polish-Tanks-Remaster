using NUnit.Framework;
using PolishTanks.AI;

namespace PolishTanks.Tests.EditMode
{
    public class SightBoostTests
    {
        [Test]
        public void Factor_RightAfterTrigger_IsTheTriggerFactor()
        {
            var boost = new SightBoost();

            boost.Trigger(10f, 2f, 4f);

            Assert.That(boost.Factor(10f), Is.EqualTo(2f).Within(1e-4f));
        }

        [Test]
        public void Factor_HalfwayThroughDuration_IsHalfwayBackToOne()
        {
            var boost = new SightBoost();
            boost.Trigger(10f, 2f, 4f);

            Assert.That(boost.Factor(12f), Is.EqualTo(1.5f).Within(1e-4f));
        }

        [Test]
        public void Factor_AfterDuration_IsOne()
        {
            var boost = new SightBoost();
            boost.Trigger(10f, 2f, 4f);

            Assert.That(boost.Factor(14f), Is.EqualTo(1f));
        }

        [Test]
        public void Trigger_WeakerThanCurrentBoost_DoesNotCutItShort()
        {
            var boost = new SightBoost();
            boost.Trigger(0f, 3f, 4f);

            boost.Trigger(1f, 1.5f, 4f);

            Assert.That(boost.Factor(1f), Is.EqualTo(2.5f).Within(1e-4f));
        }

        [Test]
        public void Trigger_StrongerThanCurrentBoost_TakesOver()
        {
            var boost = new SightBoost();
            boost.Trigger(0f, 1.5f, 4f);

            boost.Trigger(1f, 3f, 4f);

            Assert.That(boost.Factor(1f), Is.EqualTo(3f).Within(1e-4f));
        }
    }
}
