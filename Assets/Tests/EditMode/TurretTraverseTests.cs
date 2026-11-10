using NUnit.Framework;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class TurretTraverseTests
    {
        private const float Dt = 0.02f;

        [Test]
        public void Step_NeverExceedsMaxTraverseSpeed()
        {
            var settings = TurretSettings.Default;
            var traverse = new TurretTraverse(settings);

            for (var i = 0; i < 200; i++)
            {
                var before = traverse.Angle;
                traverse.Step(179f, Dt);
                var turned = Mathf.Abs(Mathf.DeltaAngle(before, traverse.Angle));
                Assert.That(turned, Is.LessThanOrEqualTo(settings.maxTraverseSpeed * Dt + 1e-4f));
            }
        }

        [Test]
        public void Step_LagsBehindTargetAtFirst()
        {
            var traverse = new TurretTraverse(TurretSettings.Default);

            traverse.Step(90f, Dt);

            Assert.That(traverse.Angle, Is.GreaterThan(0f).And.LessThan(10f));
        }

        [Test]
        public void Step_EventuallyReachesTarget()
        {
            var traverse = new TurretTraverse(TurretSettings.Default);

            for (var i = 0; i < 500; i++)
                traverse.Step(120f, Dt);

            Assert.That(traverse.Angle, Is.EqualTo(120f).Within(0.01f));
            Assert.That(traverse.Velocity, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Step_TakesShortestWayAcross180()
        {
            var traverse = new TurretTraverse(TurretSettings.Default, 170f);

            traverse.Step(-170f, Dt);

            Assert.That(traverse.Velocity, Is.GreaterThan(0f));
        }

        [Test]
        public void Angle_StaysNormalized()
        {
            var traverse = new TurretTraverse(TurretSettings.Default, 170f);

            for (var i = 0; i < 500; i++)
                traverse.Step(-170f, Dt);

            Assert.That(traverse.Angle, Is.InRange(-180f, 180f));
            Assert.That(Mathf.DeltaAngle(traverse.Angle, -170f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Step_ZeroDeltaTime_ChangesNothing()
        {
            var traverse = new TurretTraverse(TurretSettings.Default, 10f);

            traverse.Step(90f, 0f);

            Assert.That(traverse.Angle, Is.EqualTo(10f));
        }
    }
}
