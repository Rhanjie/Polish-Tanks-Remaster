using NUnit.Framework;
using PolishTanks.Missions;

namespace PolishTanks.Tests.EditMode
{
    /// <summary><see cref="MissionProgress"/>: ordered primaries, early kills, survive timing, win, loss and respect.</summary>
    public class MissionProgressTests
    {
        private static MissionObjective Reach(string id, bool optional = false, int respect = 50) =>
            new MissionObjective(id, MissionObjectiveKind.Reach, id, optional, respect);

        private static MissionObjective Destroy(string id, int count = 1) =>
            new MissionObjective(id, MissionObjectiveKind.Destroy, id, false, 50, 0f, count);

        private static MissionObjective Survive(string id, float seconds) =>
            new MissionObjective(id, MissionObjectiveKind.Survive, id, false, 50, seconds);

        private static MissionProgress Create(params MissionObjective[] objectives) =>
            new MissionProgress(objectives, default);

        [Test]
        public void Reached_LaterObjectiveBeforeItOpens_DoesNothing()
        {
            var progress = Create(Reach("a"), Reach("b"));

            progress.Reached("b");

            Assert.That(progress.Objectives[1].State, Is.EqualTo(ObjectiveState.Pending));
        }

        [Test]
        public void Reached_PreviousObjective_CompletesLaterDestroyKilledEarly()
        {
            var progress = Create(Reach("a"), Destroy("t"), Reach("b"));
            progress.TargetDestroyed("t");

            progress.Reached("a");

            Assert.That(progress.CurrentPrimary.Objective.Id, Is.EqualTo("b"));
        }

        [Test]
        public void Tick_SurviveObjective_CountsOnlyOnceOpenAndWinsAfterItsTime()
        {
            var progress = Create(Reach("a"), Survive("s", 10f));
            progress.Tick(20f);
            progress.Reached("a");

            progress.Tick(5f);
            var halfway = progress.Outcome;
            progress.Tick(5f);

            Assert.That(halfway, Is.EqualTo(MissionOutcome.Running));
            Assert.That(progress.Outcome, Is.EqualTo(MissionOutcome.Won));
        }

        [Test]
        public void Reached_LastPrimary_WinsAndFailsUnfinishedOptional()
        {
            var progress = Create(Reach("a"), Reach("bonus", optional: true));

            progress.Reached("a");

            Assert.That(progress.Objectives[1].State, Is.EqualTo(ObjectiveState.Failed));
            Assert.That(progress.Outcome, Is.EqualTo(MissionOutcome.Won));
        }

        [Test]
        public void Fail_WithOpenObjectives_LosesAndFailsThem()
        {
            var progress = Create(Reach("a"), Reach("b"));

            progress.Fail();

            Assert.That(progress.Outcome, Is.EqualTo(MissionOutcome.Lost));
            Assert.That(progress.Objectives[0].State, Is.EqualTo(ObjectiveState.Failed));
            Assert.That(progress.Objectives[1].State, Is.EqualTo(ObjectiveState.Failed));
        }

        [Test]
        public void TotalRespect_AfterObjectivesAndLosses_SumsBoth()
        {
            var respect = new MissionRespect { tankDestroyed = 100, infantryKilled = 10 };
            var progress = new MissionProgress(new[] { Reach("a", respect: 30), Reach("b", respect: 70) }, respect);
            progress.Reached("a");
            progress.EnemyLost(EnemyLoss.Tank);
            progress.EnemyLost(EnemyLoss.Infantry);
            progress.EnemyLost(EnemyLoss.Infantry);

            Assert.That(progress.TotalRespect, Is.EqualTo(30 + 100 + 20));
        }
    }
}
