using NUnit.Framework;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class SegmentMathTests
    {
        private static readonly Vector2 From = Vector2.zero;
        private static readonly Vector2 To = new Vector2(10f, 0f);

        [Test]
        public void DistanceToSegment_PointBesideTheMiddle_IsThePerpendicularDistance()
        {
            Assert.That(SegmentMath.DistanceToSegment(new Vector2(5f, 3f), From, To), Is.EqualTo(3f).Within(1e-4f));
        }

        [Test]
        public void DistanceToSegment_PointBeyondTheEnd_IsTheDistanceToTheEndpoint()
        {
            Assert.That(SegmentMath.DistanceToSegment(new Vector2(13f, 4f), From, To), Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void DistanceToSegment_PointBeforeTheStart_IsTheDistanceToTheStartpoint()
        {
            Assert.That(SegmentMath.DistanceToSegment(new Vector2(-3f, 4f), From, To), Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void DistanceToSegment_ZeroLengthSegment_IsTheDistanceToThatPoint()
        {
            Assert.That(SegmentMath.DistanceToSegment(new Vector2(3f, 4f), From, From), Is.EqualTo(5f).Within(1e-4f));
        }
    }
}
