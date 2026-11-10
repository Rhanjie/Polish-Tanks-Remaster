using NUnit.Framework;
using PolishTanks.Combat;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class ArmorMathTests
    {
        [TestCase(0f, 1f, HitSide.Front)]
        [TestCase(1f, 0f, HitSide.Side)]
        [TestCase(0f, -1f, HitSide.Rear)]
        public void GetSide_PicksPlateFromSurfaceNormal(float normalX, float normalY, HitSide expected)
        {
            Assert.That(ArmorMath.GetSide(Vector2.up, new Vector2(normalX, normalY)), Is.EqualTo(expected));
        }

        [Test]
        public void GetSide_FollowsTargetRotation()
        {
            Assert.That(ArmorMath.GetSide(Vector2.right, Vector2.right), Is.EqualTo(HitSide.Front));
        }

        [Test]
        public void SnapToFace_CornerNormal_PicksDominantFace()
        {
            Assert.That(ArmorMath.SnapToFace(new Vector2(0.6f, -0.8f)), Is.EqualTo(Vector2.down));
            Assert.That(ArmorMath.SnapToFace(new Vector2(-0.9f, 0.3f)), Is.EqualTo(Vector2.left));
        }

        [Test]
        public void SnapToFace_CornerGraze_PicksFaceTheShotTravelsInto()
        {
            var face = ArmorMath.SnapToFace(new Vector2(0.8f, 0.6f), new Vector2(0.3f, -0.954f));

            Assert.That(face, Is.EqualTo(Vector2.up));
        }

        [Test]
        public void EffectiveArmor_SquareHit_IsNominal()
        {
            Assert.That(ArmorMath.EffectiveArmor(50f, Vector2.up, Vector2.down), Is.EqualTo(50f).Within(1e-3f));
        }

        [Test]
        public void EffectiveArmor_AngledHit_IsThicker()
        {
            var shot = new Vector2(1f, -1f).normalized;

            Assert.That(ArmorMath.EffectiveArmor(50f, Vector2.up, shot), Is.EqualTo(50f / Mathf.Cos(45f * Mathf.Deg2Rad)).Within(1e-3f));
        }

        [Test]
        public void EffectiveArmor_GrazingHit_IsCapped()
        {
            var effective = ArmorMath.EffectiveArmor(10f, Vector2.up, Vector2.right);

            Assert.That(effective, Is.EqualTo(10f / Mathf.Cos(ArmorMath.MaxImpactAngle * Mathf.Deg2Rad)).Within(1e-3f));
        }

        [Test]
        public void Resolve_RollWithinRange_DecidesPenetration()
        {
            var armor = new ArmorProfile(100f, 50f, 20f);

            var blocked = ArmorMath.Resolve(armor, Vector2.up, Vector2.up, Vector2.down, 60f, 90f, 1f);
            var rearPen = ArmorMath.Resolve(armor, Vector2.up, Vector2.down, Vector2.up, 60f, 90f, 0f);

            Assert.That(blocked.Penetrated, Is.False);
            Assert.That(rearPen.Penetrated, Is.True);
            Assert.That(rearPen.Penetration, Is.EqualTo(60f));
        }
    }
}
