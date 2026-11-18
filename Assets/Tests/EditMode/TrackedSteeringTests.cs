using NUnit.Framework;
using PolishTanks.Navigation;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class TrackedSteeringTests
    {
        private static readonly SteeringSettings Settings = SteeringSettings.Default;

        [Test]
        public void Toward_TargetAhead_DrivesStraightAtFullThrottle()
        {
            var drive = TrackedSteering.Toward(Vector2.zero, Vector2.up, new Vector2(0f, 10f), false, Settings);

            Assert.That(drive.Throttle, Is.EqualTo(1f).Within(0.01f));
            Assert.That(drive.Steer, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Toward_TargetOnTheRight_TurnsRightOnTheSpot()
        {
            var drive = TrackedSteering.Toward(Vector2.zero, Vector2.up, new Vector2(5f, 0f), false, Settings);

            Assert.That(drive.Steer, Is.GreaterThan(0f));
            Assert.That(drive.Throttle, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Toward_TargetCloseBehind_Reverses()
        {
            var drive = TrackedSteering.Toward(Vector2.zero, Vector2.up, new Vector2(0f, -3f), false, Settings);

            Assert.That(drive.Throttle, Is.LessThan(0f));
        }

        [Test]
        public void Toward_FinalPointVeryNear_KeepsMinimumApproachThrottle()
        {
            var drive = TrackedSteering.Toward(Vector2.zero, Vector2.up, new Vector2(0f, 1f), true, Settings);

            Assert.That(drive.Throttle, Is.EqualTo(Settings.minApproachThrottle).Within(0.01f));
        }

        [Test]
        public void Toward_FinalPointWithinSlowDownRange_ThrottleBelowFull()
        {
            var drive = TrackedSteering.Toward(Vector2.zero, Vector2.up, new Vector2(0f, 4f), true, Settings);

            Assert.That(drive.Throttle, Is.LessThan(1f));
        }
    }
}
