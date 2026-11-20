using NUnit.Framework;
using PolishTanks.Missions;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>
    /// <see cref="MissionProgress"/>: ordered primaries, start markers, early kills, survive timing,
    /// avoid objectives, optionals opened by a primary, win, loss and respect.
    /// </summary>
    public class MissionProgressTests
    {
        private static MissionObjective Reach(string id, bool optional = false, int respect = 50) =>
            new MissionObjective(id, MissionObjectiveKind.Reach, id, optional, respect);

        private static MissionObjective Destroy(string id, int count = 1) =>
            new MissionObjective(id, MissionObjectiveKind.Destroy, id, false, 50, 0f, count);

        private static MissionObjective Survive(string id, float seconds) =>
            new MissionObjective(id, MissionObjectiveKind.Survive, id, false, 50, seconds);

        private static MissionObjective Avoid(string id, string until) =>
            new MissionObjective(id, MissionObjectiveKind.Avoid, id, true, 40, untilObjectiveId: until);

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

        [Test]
        public void Begin_WaitingDestroyObjective_StartsOnlyAtItsMarkerAndCountsEarlyKills()
        {
            var markerFirst = new MissionObjective("t", MissionObjectiveKind.Destroy, "t", startMarker: true);
            var progress = Create(Reach("a"), markerFirst, Reach("b"));
            progress.Reached("a");
            progress.TargetDestroyed("t");
            var beforeMarker = progress.CurrentPrimary.State;

            progress.Begin("t");

            Assert.That(beforeMarker, Is.EqualTo(ObjectiveState.Waiting));
            Assert.That(progress.CurrentPrimary.Objective.Id, Is.EqualTo("b"));
        }

        [Test]
        public void Violated_AvoidObjective_FailsAndOnlyAnUntouchedOneCompletesWithItsObjective()
        {
            var progress = Create(Reach("a"), Reach("b"), Avoid("clean", "a"), Avoid("touched", "a"));
            progress.Violated("touched");

            progress.Reached("a");

            Assert.That(progress.Objectives[2].State, Is.EqualTo(ObjectiveState.Completed));
            Assert.That(progress.Objectives[3].State, Is.EqualTo(ObjectiveState.Failed));
        }

        [Test]
        public void Begin_ObjectiveWithTiedOptional_OpensItOnlyThenSoATouchBeforeDoesNotCount()
        {
            var marked = new MissionObjective("a", MissionObjectiveKind.Reach, "a", startMarker: true);
            var clean = new MissionObjective("clean", MissionObjectiveKind.Avoid, "clean", true, 40,
                untilObjectiveId: "a", openWithObjectiveId: "a");
            var progress = Create(marked, clean);
            progress.Violated("clean");
            var beforeMarker = progress.Objectives[1].State;

            progress.Begin("a");
            var afterMarker = progress.Objectives[1].State;
            progress.Reached("a");

            Assert.That(beforeMarker, Is.EqualTo(ObjectiveState.Pending));
            Assert.That(afterMarker, Is.EqualTo(ObjectiveState.Active));
            Assert.That(progress.Objectives[1].State, Is.EqualTo(ObjectiveState.Completed));
        }
    }
}
