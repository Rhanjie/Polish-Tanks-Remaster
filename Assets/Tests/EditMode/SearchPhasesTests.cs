using System;
using NUnit.Framework;
using PolishTanks.AI;
using PolishTanks.AI.Behaviours;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>The phases of <see cref="SearchBehaviour"/> (approach, look around, roam, done) on a fake <see cref="ISearchSpace"/>.</summary>
    public class SearchPhasesTests
    {
        private sealed class FakeSearchSpace : ISearchSpace
        {
            public Func<Vector2, bool> IsBlocked = _ => false;

            public bool IsWalkable(Vector2 position) => true;

            public float ClearDistance(Vector2 from, Vector2 direction, float maxDistance) => IsBlocked(direction) ? 0f : maxDistance;
        }

        private static readonly Vector2EqualityComparer Approx = new Vector2EqualityComparer(0.01f);

        private AiTestRig rig;
        private FakeSearchSpace space;

        [SetUp]
        public void SetUp()
        {
            rig = new AiTestRig().WithDoctrine("lookSeconds", 1f);
            space = new FakeSearchSpace();
            rig.Context.SearchSpace = space;
        }

        [TearDown]
        public void TearDown() => rig.Dispose();

        private static bool IsEast(Vector2 direction) => Vector2.Dot(direction, Vector2.right) > 0.99f;

        private Vector2 LastAim => rig.Driver.AimCalls[rig.Driver.AimCalls.Count - 1];

        [Test]
        public void Tick_DriverArrivesAtTheLostSpot_SwitchesToLookAround()
        {
            rig.WithDoctrine("searchLostTarget", true);
            rig.Target.Report(new Vector2(10f, 0f), 0f);
            var search = new SearchBehaviour();
            search.Enter(rig.Context);
            rig.Driver.Arrive();

            search.Tick(rig.Context);

            Assert.That(search.Phase, Is.EqualTo(SearchPhase.LookAround));
        }

        [Test]
        public void Enter_FarFromTheLostSpot_StartsWithTheApproach()
        {
            rig.WithDoctrine("searchLostTarget", true);
            rig.Target.Report(new Vector2(10f, 0f), 0f);
            var search = new SearchBehaviour();

            search.Enter(rig.Context);

            Assert.That(search.Phase, Is.EqualTo(SearchPhase.Approach));
        }

        [Test]
        public void Enter_BlockedDirections_AreNotLookedAt()
        {
            rig.WithDoctrine("searchLostTarget", false);
            rig.Target.Report(new Vector2(10f, 0f), 0f);
            space.IsBlocked = IsEast;
            var search = new SearchBehaviour();

            search.Enter(rig.Context);

            Assert.That(search.LookDirections.Count, Is.EqualTo(SearchBehaviour.DirectionCount - 1));
            Assert.That(search.LookDirections, Has.None.Matches<Vector2>(IsEast));
        }

        [Test]
        public void Tick_NoHeadingKnown_FirstLookIsTowardTheLostSpot()
        {
            rig.WithDoctrine("searchLostTarget", false);
            rig.Target.Report(new Vector2(0f, 10f), 0f);
            var search = new SearchBehaviour();
            search.Enter(rig.Context);

            search.Tick(rig.Context);

            Assert.That(LastAim, Is.EqualTo(new Vector2(0f, SearchBehaviour.LookDistance)).Using(Approx));
        }

        [Test]
        public void Tick_TargetWasHeadingWest_FirstLookIsWest()
        {
            rig.WithDoctrine("searchLostTarget", false);
            rig.Target.Observe(true, new Vector2(0f, 10f), 0f);
            rig.Target.Observe(true, new Vector2(-2f, 10f), 1f);
            rig.Target.Observe(false, Vector2.zero, 2f);
            var search = new SearchBehaviour();
            search.Enter(rig.Context);

            search.Tick(rig.Context);

            Assert.That(LastAim, Is.EqualTo(new Vector2(-SearchBehaviour.LookDistance, 0f)).Using(Approx));
        }

        [Test]
        public void Enter_EveryDirectionBlocked_GoesStraightToRoaming()
        {
            rig.WithDoctrine("searchLostTarget", false);
            rig.Target.Report(new Vector2(10f, 0f), 0f);
            space.IsBlocked = _ => true;
            var search = new SearchBehaviour();

            search.Enter(rig.Context);

            Assert.That(search.Phase, Is.EqualTo(SearchPhase.Roam));
        }

        [Test]
        public void Tick_RoamBeforeSearchSecondsPass_KeepsTheTarget()
        {
            rig.WithDoctrine("searchLostTarget", false).WithDoctrine("searchSeconds", 5f);
            rig.Target.Report(new Vector2(10f, 0f), 0f);
            space.IsBlocked = _ => true;
            var search = new SearchBehaviour();
            search.Enter(rig.Context);

            rig.Context.Time = 4.9f;
            search.Tick(rig.Context);

            Assert.That(rig.Target.IsKnown, Is.True);
        }

        [Test]
        public void Tick_RoamAfterSearchSecondsPass_EndsAndForgetsTheTarget()
        {
            rig.WithDoctrine("searchLostTarget", false).WithDoctrine("searchSeconds", 5f);
            rig.Target.Report(new Vector2(10f, 0f), 0f);
            space.IsBlocked = _ => true;
            var search = new SearchBehaviour();
            search.Enter(rig.Context);

            rig.Context.Time = 5f;
            search.Tick(rig.Context);

            Assert.That(search.Phase, Is.EqualTo(SearchPhase.Done));
            Assert.That(rig.Target.IsKnown, Is.False);
        }

        [Test]
        public void Tick_NewReportWhileLookingAround_RestartsTheApproach()
        {
            rig.WithDoctrine("searchLostTarget", true);
            rig.Driver.Position = new Vector2(9f, 0f);
            rig.Target.Report(new Vector2(10f, 0f), 0f);
            var search = new SearchBehaviour();
            search.Enter(rig.Context);
            rig.Target.Report(new Vector2(40f, 0f), 1f);

            search.Tick(rig.Context);

            Assert.That(search.Phase, Is.EqualTo(SearchPhase.Approach));
            Assert.That(rig.Driver.LastMoveTo, Is.EqualTo(new Vector2(40f, 0f)));
        }
    }
}
