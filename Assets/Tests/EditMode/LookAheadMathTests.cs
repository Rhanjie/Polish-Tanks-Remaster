using NUnit.Framework;
using PolishTanks.CameraRig;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Look-ahead camera offset rules.</summary>
    public class LookAheadMathTests
    {
        private static readonly Vector2 Screen = new Vector2(1600f, 900f);
        private const float OrthoSize = 10f;
        private const float HalfWidth = OrthoSize * 1600f / 900f;

        [Test]
        public void Offset_CursorAtScreenCentre_IsZero()
        {
            var offset = LookAheadMath.Offset(Screen * 0.5f, Screen, OrthoSize, 0.5f, 100f);

            Assert.That(offset.magnitude, Is.EqualTo(0f).Within(1e-5f));
        }

        [Test]
        public void Offset_CursorAtRightEdge_IsStrengthTimesHalfWidth()
        {
            var offset = LookAheadMath.Offset(new Vector2(Screen.x, Screen.y * 0.5f), Screen, OrthoSize, 0.4f, 100f);

            Assert.That(offset.x, Is.EqualTo(0.4f * HalfWidth).Within(1e-4f));
        }

        [Test]
        public void Offset_CursorOutsideScreen_EqualsOffsetAtEdge()
        {
            var atEdge = LookAheadMath.Offset(new Vector2(Screen.x, 0f), Screen, OrthoSize, 0.4f, 100f);
            var outside = LookAheadMath.Offset(new Vector2(Screen.x * 5f, -Screen.y * 3f), Screen, OrthoSize, 0.4f, 100f);

            Assert.That(outside, Is.EqualTo(atEdge));
        }

        [Test]
        public void Offset_MaxDistanceBelowNaturalOffset_CapsMagnitude()
        {
            var offset = LookAheadMath.Offset(new Vector2(Screen.x, Screen.y), Screen, OrthoSize, 1f, 2f);

            Assert.That(offset.magnitude, Is.EqualTo(2f).Within(1e-4f));
        }
    }
}
