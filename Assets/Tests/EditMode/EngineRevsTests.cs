using NUnit.Framework;
using PolishTanks.Audio;

namespace PolishTanks.Tests.EditMode
{
    public class EngineRevsTests
    {
        [Test]
        public void Target_ThrottleAtStandstill_RevsBeforeTheHullMoves()
        {
            Assert.That(EngineRevs.Target(0f, 5f, 1f, 0f), Is.EqualTo(1f));
        }

        [Test]
        public void Target_CoastingInReverse_FollowsSpeedShare()
        {
            Assert.That(EngineRevs.Target(-2.5f, 5f, 0f, 0f), Is.EqualTo(0.5f).Within(1e-5f));
        }

        [Test]
        public void Approach_FallsSlowerThanItRises()
        {
            var up = EngineRevs.Approach(0f, 1f, 2f, 0.5f, 0.1f);
            var down = 1f - EngineRevs.Approach(1f, 0f, 2f, 0.5f, 0.1f);

            Assert.That(up, Is.GreaterThan(down));
        }
    }
}
