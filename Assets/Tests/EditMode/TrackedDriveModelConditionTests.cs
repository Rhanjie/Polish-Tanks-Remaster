using NUnit.Framework;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Drive behaviour under a damaged <see cref="DriveCondition"/>: one track out, no drive at all and a field-patched track.</summary>
    public class TrackedDriveModelConditionTests
    {
        private const float Dt = 0.02f;
        private const float Crawl = 0.05f;

        private static TrackedDriveModel CreateModel(DriveCondition condition) =>
            new TrackedDriveModel(TankDriveSettings.Default) { Condition = condition };

        private static void Run(TrackedDriveModel model, float throttle, float steer, float seconds)
        {
            for (var t = 0f; t < seconds; t += Dt)
                model.Step(throttle, steer, Dt);
        }

        [Test]
        public void Step_LeftTrackOutUnderThrottle_TurnsTowardLeft()
        {
            var model = CreateModel(new DriveCondition(1f, 0f, 1f, Crawl));

            Run(model, 1f, 0f, 1f);

            Assert.That(model.AngularSpeed, Is.GreaterThan(0f));
        }

        [Test]
        public void Step_LeftTrackOutUnderThrottle_SpeedStaysFarBelowNormal()
        {
            var model = CreateModel(new DriveCondition(1f, 0f, 1f, Crawl));

            Run(model, 1f, 0f, 5f);

            Assert.That(model.Speed, Is.LessThan(TankDriveSettings.Default.maxForwardSpeed * 0.1f));
        }

        [TestCase(0f, 1f, 1f)]
        [TestCase(1f, 0f, 0f)]
        public void Step_NoEngineOrNoTracks_SpeedAndTurnDecayToZero(float power, float left, float right)
        {
            var model = CreateModel(DriveCondition.Healthy);
            Run(model, 1f, 1f, 3f);
            model.Condition = new DriveCondition(power, left, right, Crawl);

            Run(model, 1f, 1f, 3f);

            Assert.That(Mathf.Abs(model.Speed) + Mathf.Abs(model.AngularSpeed), Is.EqualTo(0f));
        }

        [Test]
        public void Step_FieldRepairedTracks_CapSpeedAtHalf()
        {
            var model = CreateModel(new DriveCondition(1f, 0.5f, 0.5f, Crawl));

            Run(model, 1f, 0f, 10f);

            Assert.That(model.Speed, Is.EqualTo(TankDriveSettings.Default.maxForwardSpeed * 0.5f).Within(1e-4f));
        }
    }
}
