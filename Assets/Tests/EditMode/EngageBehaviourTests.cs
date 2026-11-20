using NUnit.Framework;
using PolishTanks.AI.Behaviours;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class EngageBehaviourTests
    {
        private AiTestRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new AiTestRig().WithDoctrine("reactionTime", 0.5f);
            rig.Target.Observe(true, new Vector2(10f, 0f), 0f);
            rig.Context.Time = 1f;
        }

        [TearDown]
        public void TearDown() => rig.Dispose();

        [Test]
        public void Score_TargetSpotted_IsPositive()
        {
            Assert.That(new EngageBehaviour().Score(rig.Context), Is.GreaterThan(0f));
        }

        [Test]
        public void Score_TargetStillBeingNoticed_IsZero()
        {
            rig.Context.Time = 0.2f;

            Assert.That(new EngageBehaviour().Score(rig.Context), Is.EqualTo(0f));
        }

        [Test]
        public void Score_ScriptedCombat_IsZeroEvenWhenSpotted()
        {
            rig.Context.ScriptedCombat = true;

            Assert.That(new EngageBehaviour().Score(rig.Context), Is.EqualTo(0f));
        }

        [Test]
        public void TickThenExit_HoldsAnAngledHullHeadingAndReleasesIt()
        {
            var engage = new EngageBehaviour(() => 0.9f);

            engage.Tick(rig.Context);
            var held = rig.Driver.HullHeading;
            engage.Exit(rig.Context);

            Assert.That(held.HasValue && Vector2.Angle(Vector2.right, held.Value) > 10f, Is.True);
            Assert.That(rig.Driver.HullHeading, Is.Null);
        }
    }
}
