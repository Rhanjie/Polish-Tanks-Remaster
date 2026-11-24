using System.Collections;
using NUnit.Framework;
using PolishTanks.Navigation;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Drives a rig tank over a grid whose wall exists only in the navigation data, so the tests check the route the
    /// follower takes and not collisions.
    /// </summary>
    public class TankPathFollowerTests
    {
        private static readonly Rect Area = new Rect(-15f, -10f, 30f, 20f);
        private static readonly Vector2 Start = new Vector2(-8f, 0f);
        private static readonly Vector2 Goal = new Vector2(8f, 0f);

        private TankRig rig;

        [SetUp]
        public void SetUp() => rig = new TankRig();

        [TearDown]
        public void TearDown() => rig.Dispose();

        private TankPathFollower BuildFollower(BoxObstacleSource source, out NavGrid grid)
        {
            grid = source.BuildGrid(Area);
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Start);
            var follower = tank.Root.AddComponent<TankPathFollower>();
            follower.Navigation = new GridNavigation(grid);
            return follower;
        }

        private static BoxObstacleSource WallWithGapAbove() => new BoxObstacleSource().Solid(new Rect(-0.1f, -10f, 0.2f, 12f));

        [UnityTest]
        public IEnumerator MoveTo_GoalBehindWall_ArrivesWithinTimeLimit()
        {
            var follower = BuildFollower(WallWithGapAbove(), out _);
            var arrivals = 0;
            follower.Arrived += _ => arrivals++;
            follower.MoveTo(Goal);

            var deadline = Time.time + 40f;
            while (!follower.HasArrived && Time.time < deadline)
                yield return null;

            Assert.That(follower.LastStatus, Is.EqualTo(PathStatus.Found));
            Assert.That(follower.HasArrived, Is.True);
            Assert.That(arrivals, Is.EqualTo(1));
        }

        [Test]
        public void MoveTo_GoalBehindWall_PlansWaypointsOutsideBlockedCells()
        {
            var follower = BuildFollower(WallWithGapAbove(), out var grid);

            follower.MoveTo(Goal);

            var blocked = false;
            foreach (var point in follower.Path)
                blocked |= grid.IsBlocked(grid.WorldToCell(point));
            Assert.That(follower.Path.Count, Is.GreaterThan(1));
            Assert.That(blocked, Is.False);
        }

        [Test]
        public void MoveTo_EverythingBlocked_ReturnsFalseAndRaisesPathFailed()
        {
            var follower = BuildFollower(new BoxObstacleSource().Solid(Area), out _);
            var failures = 0;
            follower.PathFailed += _ => failures++;

            var started = follower.MoveTo(Goal);

            Assert.That(started, Is.False);
            Assert.That(failures, Is.EqualTo(1));
        }
    }
}
