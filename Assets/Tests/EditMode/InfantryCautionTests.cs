using NUnit.Framework;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>A tank at the origin faces +Y; the agents are placed around it.</summary>
    public class InfantryCautionTests
    {
        private static FakeAiAgent Soldier(float x, float y, bool onFoot = true) =>
            new FakeAiAgent { Position = new Vector2(x, y), IsOnFoot = onFoot };

        private static void Yield(FakeAiAgent agent, out float speedLimit, out float steerBias) =>
            InfantryCaution.Yield(Vector2.zero, Vector2.up, new IAiAgent[] { agent }, out speedLimit, out steerBias);

        [Test]
        public void Yield_SoldierAheadInTheLane_SlowsTheTank()
        {
            Yield(Soldier(0f, 5f), out var speedLimit, out _);

            Assert.That(speedLimit, Is.LessThan(1f));
        }

        [Test]
        public void Yield_SoldierAheadOnTheLeft_SteersRight()
        {
            Yield(Soldier(-1f, 5f), out _, out var steerBias);

            Assert.That(steerBias, Is.GreaterThan(0f));
        }

        [Test]
        public void Yield_SoldierAheadOnTheRight_SteersLeft()
        {
            Yield(Soldier(1f, 5f), out _, out var steerBias);

            Assert.That(steerBias, Is.LessThan(0f));
        }

        [Test]
        public void Yield_CloserSoldier_SlowsMoreThanAFarOne()
        {
            Yield(Soldier(0f, 4f), out var near, out _);
            Yield(Soldier(0f, 8f), out var far, out _);

            Assert.That(near, Is.LessThan(far));
        }

        [Test]
        public void Yield_SoldierBehindTheTank_HasNoEffect()
        {
            Yield(Soldier(0f, -3f), out var speedLimit, out var steerBias);

            Assert.That(speedLimit, Is.EqualTo(1f));
            Assert.That(steerBias, Is.EqualTo(0f));
        }

        [Test]
        public void Yield_SoldierOutsideTheLane_HasNoEffect()
        {
            Yield(Soldier(InfantryCaution.LaneHalfWidth + 1f, 5f), out var speedLimit, out var steerBias);

            Assert.That(speedLimit, Is.EqualTo(1f));
            Assert.That(steerBias, Is.EqualTo(0f));
        }

        [Test]
        public void Yield_SoldierBeyondLookAhead_HasNoEffect()
        {
            Yield(Soldier(0f, InfantryCaution.LookAhead + 1f), out var speedLimit, out _);

            Assert.That(speedLimit, Is.EqualTo(1f));
        }

        [Test]
        public void Yield_TankAheadInTheLane_HasNoEffect()
        {
            Yield(Soldier(0f, 5f, onFoot: false), out var speedLimit, out var steerBias);

            Assert.That(speedLimit, Is.EqualTo(1f));
            Assert.That(steerBias, Is.EqualTo(0f));
        }

        [Test]
        public void Yield_InactiveSoldierAhead_HasNoEffect()
        {
            var dead = Soldier(0f, 5f);
            dead.IsActive = false;

            Yield(dead, out var speedLimit, out _);

            Assert.That(speedLimit, Is.EqualTo(1f));
        }
    }
}
