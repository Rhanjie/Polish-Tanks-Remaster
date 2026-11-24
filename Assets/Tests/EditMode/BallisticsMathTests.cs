using NUnit.Framework;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class BallisticsMathTests
    {
        [Test]
        public void Rotate_90Degrees_TurnsUpToLeft()
        {
            var v = BallisticsMath.Rotate(Vector2.up, 90f);

            Assert.That(v.x, Is.EqualTo(-1f).Within(1e-5f));
            Assert.That(v.y, Is.EqualTo(0f).Within(1e-5f));
        }

        [TestCase(0f, 0f)]
        [TestCase(1f, 1f)]
        [TestCase(0.5f, 0.5f)]
        [TestCase(0f, 1f)]
        [TestCase(0.9f, 0.95f)]
        public void ApplySpread_StaysWithinSpread(float r1, float r2)
        {
            var dir = BallisticsMath.ApplySpread(Vector2.up, 5f, r1, r2);

            Assert.That(Vector2.Angle(Vector2.up, dir), Is.LessThanOrEqualTo(5f + 1e-3f));
            Assert.That(dir.magnitude, Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void ApplySpread_MidRandoms_ShootStraight()
        {
            var dir = BallisticsMath.ApplySpread(Vector2.right, 10f, 0.5f, 0.5f);

            Assert.That(Vector2.Angle(Vector2.right, dir), Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void ApplySpread_ExtremeRandoms_HitFullSpread()
        {
            var dir = BallisticsMath.ApplySpread(Vector2.up, 4f, 1f, 1f);

            Assert.That(Vector2.SignedAngle(Vector2.up, dir), Is.EqualTo(4f).Within(1e-3f));
        }

        [Test]
        public void ApplySpread_ZeroSpread_NormalizesOnly()
        {
            var dir = BallisticsMath.ApplySpread(new Vector2(0f, 3f), 0f, 0f, 1f);

            Assert.That(dir, Is.EqualTo(Vector2.up));
        }

        [Test]
        public void LaunchVelocity_AddsShooterMotion()
        {
            var v = BallisticsMath.LaunchVelocity(Vector2.up * 2f, 40f, new Vector2(1f, 3f));

            Assert.That(v.x, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(v.y, Is.EqualTo(43f).Within(1e-5f));
        }
    }
}
