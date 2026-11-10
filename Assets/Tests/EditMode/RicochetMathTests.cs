using NUnit.Framework;
using PolishTanks.Combat;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class RicochetMathTests
    {
        [Test]
        public void Chance_SquareHit_IsZero()
        {
            Assert.That(RicochetMath.Chance(0f, false), Is.EqualTo(0f));
            Assert.That(RicochetMath.Chance(RicochetMath.StartAngle, false), Is.EqualTo(0f));
        }

        [Test]
        public void Chance_GrazingHit_IsCertain()
        {
            Assert.That(RicochetMath.Chance(RicochetMath.FullAngle + 5f, false), Is.EqualTo(1f));
        }

        [Test]
        public void Chance_Overmatch_IsZeroEvenWhenGrazing()
        {
            Assert.That(RicochetMath.IsOvermatch(88f, 20f), Is.True);
            Assert.That(RicochetMath.Chance(80f, true), Is.EqualTo(0f));
        }

        [Test]
        public void IsOvermatch_SmallCaliberOnThickPlate_IsFalse()
        {
            Assert.That(RicochetMath.IsOvermatch(7.92f, 25f), Is.False);
        }

        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(1f)]
        public void Deflect_LeavesAwayFromThePlate(float random)
        {
            var shot = new Vector2(1f, -0.3f).normalized;

            var outgoing = RicochetMath.Deflect(shot, Vector2.up, random);

            Assert.That(Vector2.Dot(outgoing, Vector2.up), Is.GreaterThan(0f));
            Assert.That(outgoing.magnitude, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void Deflect_ShotLeavingThePlate_StillEndsUpAwayFromIt()
        {
            var outgoing = RicochetMath.Deflect(new Vector2(0.3f, -0.954f), Vector2.right, 0f);

            Assert.That(Vector2.Dot(outgoing, Vector2.right), Is.GreaterThan(0f));
        }

        [Test]
        public void Deflect_SkimsFlatterThanAMirror()
        {
            var shot = new Vector2(1f, -1f).normalized;

            var outgoing = RicochetMath.Deflect(shot, Vector2.up, 0.5f);

            Assert.That(Vector2.Angle(outgoing, Vector2.right), Is.LessThan(45f));
        }
    }
}
