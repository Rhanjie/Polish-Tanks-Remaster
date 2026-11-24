using System;
using NUnit.Framework;
using PolishTanks.Infantry;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>A tank at the origin drives along +X at 5 m/s; soldiers stand around it. The random source is scripted.</summary>
    public class GiveWayPlannerTests
    {
        private static readonly TankThreat Tank = new TankThreat(Vector2.zero, new Vector2(5f, 0f));

        private static Func<float> Script(params float[] values)
        {
            var index = 0;
            return () => values[Math.Min(index++, values.Length - 1)];
        }

        private static GiveWayPlanner CalmPlanner() => new GiveWayPlanner(Script(0f, 0.9f, 0.9f));

        private static bool Plan(GiveWayPlanner planner, Vector2 soldier, float time, out Vector2 step, Func<Vector2, bool> walkable = null) =>
            planner.TryPlan(soldier, new[] { Tank }, time, walkable ?? (_ => true), out step);

        [Test]
        public void IsInLane_TankDrivingAtTheSoldier_IsTrue()
        {
            Assert.That(GiveWayPlanner.IsInLane(new Vector2(10f, 0f), Tank), Is.True);
        }

        [Test]
        public void IsInLane_SoldierWellToTheSide_IsFalse()
        {
            Assert.That(GiveWayPlanner.IsInLane(new Vector2(0f, 6f), Tank), Is.False);
        }

        [Test]
        public void IsInLane_TankDrivingAwayFromTheSoldier_IsFalse()
        {
            Assert.That(GiveWayPlanner.IsInLane(new Vector2(-10f, 0f), Tank), Is.False);
        }

        [Test]
        public void IsInLane_TankTooFarAheadToReachInTime_IsFalse()
        {
            Assert.That(GiveWayPlanner.IsInLane(new Vector2(40f, 0f), Tank), Is.False);
        }

        [Test]
        public void IsInLane_TankBarelyMoving_IsFalse()
        {
            var crawling = new TankThreat(Vector2.zero, new Vector2(GiveWayPlanner.MinThreatSpeed * 0.5f, 0f));

            Assert.That(GiveWayPlanner.IsInLane(new Vector2(3f, 0f), crawling), Is.False);
        }

        [Test]
        public void SideStep_SoldierSlightlyLeftOfCentre_LandsOutsideTheLaneOnTheLeft()
        {
            var soldier = new Vector2(10f, 1f);

            var step = GiveWayPlanner.SideStep(soldier, Tank, GiveWayPlanner.NearerSide(soldier, Tank), 1f);

            Assert.That(step.y, Is.GreaterThan(Tank.HalfWidth + GiveWayPlanner.LaneMargin));
            Assert.That(GiveWayPlanner.IsInLane(step, Tank), Is.False);
        }

        [Test]
        public void SideStep_SoldierSlightlyRightOfCentre_LandsOnTheRight()
        {
            var soldier = new Vector2(10f, -1f);

            var step = GiveWayPlanner.SideStep(soldier, Tank, GiveWayPlanner.NearerSide(soldier, Tank), 1f);

            Assert.That(step.y, Is.LessThan(-(Tank.HalfWidth + GiveWayPlanner.LaneMargin)));
        }

        [Test]
        public void TryPlan_BeforeTheReactionTime_DoesNotStepYet()
        {
            var planner = CalmPlanner();

            Assert.That(Plan(planner, new Vector2(10f, 1f), 0f, out _), Is.False);
        }

        [Test]
        public void TryPlan_BeforeTheReactionTime_AlreadyCountsAsThreatened()
        {
            var planner = CalmPlanner();

            Plan(planner, new Vector2(10f, 1f), 0f, out _);

            Assert.That(planner.IsThreatened, Is.True);
        }

        [Test]
        public void TryPlan_AfterTheReactionTime_StepsOutOfTheLane()
        {
            var planner = CalmPlanner();
            Plan(planner, new Vector2(10f, 1f), 0f, out _);

            var stepped = Plan(planner, new Vector2(10f, 1f), GiveWayPlanner.MaxReaction, out var step);

            Assert.That(stepped, Is.True);
            Assert.That(GiveWayPlanner.IsInLane(step, Tank), Is.False);
        }

        [Test]
        public void TryPlan_NearerSideBlocked_StepsToTheOtherSide()
        {
            var planner = CalmPlanner();
            Func<Vector2, bool> noLeft = point => point.y <= 0f;
            Plan(planner, new Vector2(10f, 1f), 0f, out _, noLeft);

            Plan(planner, new Vector2(10f, 1f), GiveWayPlanner.MaxReaction, out var step, noLeft);

            Assert.That(step.y, Is.LessThan(0f));
        }

        [Test]
        public void TryPlan_SoldierJustOutsideTheLaneWhileWaiting_StaysThreatened()
        {
            var planner = CalmPlanner();
            Plan(planner, new Vector2(10f, 1f), 0f, out _);
            var justOutside = new Vector2(10f, Tank.HalfWidth + GiveWayPlanner.LaneMargin + 0.8f);

            Plan(planner, justOutside, 1f, out _);

            Assert.That(planner.IsThreatened, Is.True);
        }

        [Test]
        public void TryPlan_FreshPlannerWithSoldierJustOutsideTheLane_IsNotThreatened()
        {
            var planner = CalmPlanner();
            var justOutside = new Vector2(10f, Tank.HalfWidth + GiveWayPlanner.LaneMargin + 0.8f);

            Plan(planner, justOutside, 1f, out _);

            Assert.That(planner.IsThreatened, Is.False);
        }

        [Test]
        public void TryPlan_TankHasGoneBy_ClearsTheThreat()
        {
            var planner = CalmPlanner();
            Plan(planner, new Vector2(10f, 1f), 0f, out _);

            Plan(planner, new Vector2(-30f, 1f), 1f, out _);

            Assert.That(planner.IsThreatened, Is.False);
        }
    }
}
