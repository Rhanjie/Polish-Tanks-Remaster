using NUnit.Framework;
using PolishTanks.Tank;

namespace PolishTanks.Tests.EditMode
{
    public class TrackedDriveModelTests
    {
        private const float Dt = 0.02f;

        private static TrackedDriveModel CreateModel() => new TrackedDriveModel(TankDriveSettings.Default);

        private static void Run(TrackedDriveModel model, float throttle, float steer, float seconds)
        {
            for (var t = 0f; t < seconds; t += Dt)
                model.Step(throttle, steer, Dt);
        }

        [Test]
        public void Step_FullThrottle_AcceleratesGradually()
        {
            var model = CreateModel();

            model.Step(1f, 0f, Dt);

            Assert.That(model.Speed, Is.EqualTo(TankDriveSettings.Default.acceleration * Dt).Within(1e-5f));
        }

        [Test]
        public void Step_FullThrottleForLong_StopsAtMaxForwardSpeed()
        {
            var model = CreateModel();

            Run(model, 1f, 0f, 10f);

            Assert.That(model.Speed, Is.EqualTo(TankDriveSettings.Default.maxForwardSpeed).Within(1e-4f));
        }

        [Test]
        public void Step_FullReverseForLong_StopsAtMaxReverseSpeed()
        {
            var model = CreateModel();

            Run(model, -1f, 0f, 10f);

            Assert.That(model.Speed, Is.EqualTo(-TankDriveSettings.Default.maxReverseSpeed).Within(1e-4f));
        }

        [Test]
        public void Step_ReverseWhileMovingForward_UsesBrakingRate()
        {
            var model = CreateModel();
            Run(model, 1f, 0f, 10f);
            var before = model.Speed;

            model.Step(-1f, 0f, Dt);

            Assert.That(before - model.Speed, Is.EqualTo(TankDriveSettings.Default.braking * Dt).Within(1e-4f));
        }

        [Test]
        public void Step_NoThrottle_CoastsToStop()
        {
            var model = CreateModel();
            Run(model, 1f, 0f, 10f);

            Run(model, 0f, 0f, 5f);

            Assert.That(model.Speed, Is.EqualTo(0f));
        }

        [Test]
        public void Step_SteerRight_TurnsClockwise()
        {
            var model = CreateModel();

            Run(model, 0f, 1f, 1f);

            Assert.That(model.AngularSpeed, Is.EqualTo(-TankDriveSettings.Default.turnRate).Within(1e-4f));
        }

        [Test]
        public void Step_AtTopSpeed_TurnsSlowerThanStandingStill()
        {
            var model = CreateModel();

            Run(model, 1f, -1f, 10f);

            Assert.That(model.AngularSpeed, Is.EqualTo(TankDriveSettings.Default.turnRateAtMaxSpeed).Within(1e-3f));
        }

        [Test]
        public void Step_ZeroDeltaTime_ChangesNothing()
        {
            var model = CreateModel();

            model.Step(1f, 1f, 0f);

            Assert.That(model.Speed, Is.EqualTo(0f));
            Assert.That(model.AngularSpeed, Is.EqualTo(0f));
        }

        [Test]
        public void Step_ThrottleAboveOne_IsClamped()
        {
            var model = CreateModel();

            Run(model, 5f, 0f, 10f);

            Assert.That(model.Speed, Is.EqualTo(TankDriveSettings.Default.maxForwardSpeed).Within(1e-4f));
        }

        [Test]
        public void SyncSpeed_BlockedHull_DropsSpeed()
        {
            var model = CreateModel();
            Run(model, 1f, 0f, 10f);

            model.SyncSpeed(0f);

            Assert.That(model.Speed, Is.EqualTo(0f));
        }

        [Test]
        public void SyncSpeed_AboveLimit_IsClamped()
        {
            var model = CreateModel();

            model.SyncSpeed(100f);

            Assert.That(model.Speed, Is.EqualTo(TankDriveSettings.Default.maxForwardSpeed));
        }
    }
}
