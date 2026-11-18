using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.AI;

namespace PolishTanks.Tests.EditMode
{
    public class UtilityBrainTests
    {
        private sealed class StubBehaviour : AiBehaviour
        {
            private readonly string name;
            private readonly List<string> log;

            public StubBehaviour(string name, float score, List<string> log)
            {
                this.name = name;
                this.log = log;
                ScoreValue = score;
            }

            public float ScoreValue;

            public override AiBehaviourKind Kind => AiBehaviourKind.FollowOrders;

            public override float Score(AiContext context) => ScoreValue;

            public override void Enter(AiContext context) => log.Add(name + ".Enter");

            public override void Tick(AiContext context) => log.Add(name + ".Tick");

            public override void Exit(AiContext context) => log.Add(name + ".Exit");
        }

        private AiTestRig rig;
        private List<string> log;

        [SetUp]
        public void SetUp()
        {
            rig = new AiTestRig();
            log = new List<string>();
        }

        [TearDown]
        public void TearDown() => rig.Dispose();

        [Test]
        public void Tick_SeveralScoring_RunsTheHighest()
        {
            var low = new StubBehaviour("Low", 0.2f, log);
            var high = new StubBehaviour("High", 0.8f, log);
            var brain = new UtilityBrain(new AiBehaviour[] { low, high });

            brain.Tick(rig.Context);

            Assert.That(brain.Current, Is.SameAs(high));
        }

        [Test]
        public void Tick_RivalWithinStickiness_KeepsCurrent()
        {
            var current = new StubBehaviour("A", 0.5f, log);
            var rival = new StubBehaviour("B", 0.2f, log);
            var brain = new UtilityBrain(new AiBehaviour[] { current, rival });
            brain.Tick(rig.Context);

            rival.ScoreValue = 0.5f + UtilityBrain.Stickiness * 0.5f;
            brain.Tick(rig.Context);

            Assert.That(brain.Current, Is.SameAs(current));
        }

        [Test]
        public void Tick_RivalClearlyBetter_Switches()
        {
            var current = new StubBehaviour("A", 0.5f, log);
            var rival = new StubBehaviour("B", 0.2f, log);
            var brain = new UtilityBrain(new AiBehaviour[] { current, rival });
            brain.Tick(rig.Context);

            rival.ScoreValue = 0.5f + UtilityBrain.Stickiness * 2f;
            brain.Tick(rig.Context);

            Assert.That(brain.Current, Is.SameAs(rival));
        }

        [Test]
        public void Tick_Switch_ExitsOldBeforeEnteringNew()
        {
            var first = new StubBehaviour("A", 0.5f, log);
            var second = new StubBehaviour("B", 0f, log);
            var brain = new UtilityBrain(new AiBehaviour[] { first, second });
            brain.Tick(rig.Context);
            log.Clear();

            second.ScoreValue = 0.9f;
            brain.Tick(rig.Context);

            Assert.That(log, Is.EqualTo(new[] { "A.Exit", "B.Enter", "B.Tick" }));
        }

        [Test]
        public void Tick_NothingScoresAboveZero_IdlesAndExitsCurrent()
        {
            var only = new StubBehaviour("A", 0.5f, log);
            var brain = new UtilityBrain(new AiBehaviour[] { only });
            brain.Tick(rig.Context);

            only.ScoreValue = 0f;
            brain.Tick(rig.Context);

            Assert.That(brain.Current, Is.Null);
            Assert.That(log, Does.Contain("A.Exit"));
        }

        [Test]
        public void Tick_NegativeScore_NeverRuns()
        {
            var brain = new UtilityBrain(new AiBehaviour[] { new StubBehaviour("A", -1f, log) });

            brain.Tick(rig.Context);

            Assert.That(brain.Current, Is.Null);
        }
    }
}
