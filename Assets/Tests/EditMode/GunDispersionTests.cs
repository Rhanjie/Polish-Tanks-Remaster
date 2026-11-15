using NUnit.Framework;
using PolishTanks.Tank;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>The spread of a gun over time: floor, growth from motion and shots, cap, settling and crew scaling.</summary>
    public class GunDispersionTests
    {
        private const float Settled = 1f;
        private const float Max = 5f;
        private const float AimTime = 1f;

        private static GunSettings Settings(float dispersion = Settled) => new GunSettings
        {
            dispersion = dispersion,
            maxDispersion = Max,
            movingDispersion = 1f,
            turningDispersion = 0.1f,
            shotDispersion = 2f,
            aimTime = AimTime,
        };

        private static void Run(GunDispersion dispersion, float seconds, float speed, float rotationSpeed)
        {
            for (var t = 0f; t < seconds; t += 0.02f)
                dispersion.Tick(0.02f, speed, rotationSpeed);
        }

        [Test]
        public void Current_WithZeroDispersion_StaysAtTheFloor()
        {
            var dispersion = new GunDispersion(Settings(0f));

            Run(dispersion, 5f, 0f, 0f);

            Assert.That(dispersion.Current, Is.EqualTo(GunDispersion.MinimumSettled));
        }

        [Test]
        public void Tick_WhileMoving_OpensTheSpread()
        {
            var dispersion = new GunDispersion(Settings());

            Run(dispersion, 2f, 2f, 0f);

            Assert.That(dispersion.Current, Is.GreaterThan(Settled + 1f));
        }

        [Test]
        public void Tick_WhileSwingingTheBarrel_OpensTheSpread()
        {
            var dispersion = new GunDispersion(Settings());

            Run(dispersion, 2f, 0f, 20f);

            Assert.That(dispersion.Current, Is.GreaterThan(Settled + 1f));
        }

        [Test]
        public void Tick_AtExtremeSpeed_IsCappedAtMaximum()
        {
            var dispersion = new GunDispersion(Settings());

            Run(dispersion, 5f, 1000f, 1000f);

            Assert.That(dispersion.Current, Is.EqualTo(Max).Within(1e-3f));
        }

        [Test]
        public void Tick_StandingStillForAimTime_ClosesMostOfTheWayBack()
        {
            var dispersion = new GunDispersion(Settings());
            dispersion.OnShot();

            Run(dispersion, AimTime, 0f, 0f);

            Assert.That(dispersion.Current, Is.LessThan(Settled + 0.15f));
        }

        [Test]
        public void Tick_WithZeroDeltaTime_ChangesNothing()
        {
            var dispersion = new GunDispersion(Settings());
            dispersion.OnShot();
            var before = dispersion.Current;

            dispersion.Tick(0f, 10f, 10f);

            Assert.That(dispersion.Current, Is.EqualTo(before));
        }

        [Test]
        public void OnShot_AddsShotDispersion()
        {
            var dispersion = new GunDispersion(Settings());

            dispersion.OnShot();

            Assert.That(dispersion.Current, Is.EqualTo(Settled + 2f).Within(1e-5f));
        }

        [Test]
        public void OnShot_RepeatedlyWithoutSettling_IsCappedAtMaximum()
        {
            var dispersion = new GunDispersion(Settings());

            for (var i = 0; i < 10; i++)
                dispersion.OnShot();

            Assert.That(dispersion.Current, Is.EqualTo(Max));
        }

        [Test]
        public void Current_WithCrewFactor_ScalesTheSpread()
        {
            var dispersion = new GunDispersion(Settings()) { CrewFactor = 0.5f };

            Assert.That(dispersion.Current, Is.EqualTo(Settled * 0.5f).Within(1e-5f));
        }
    }
}
