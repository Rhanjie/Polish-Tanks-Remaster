using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.Prototypes;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class HeightProjectionTests
    {
        [Test]
        public void RoofOffset_PointsAwayFromViewCenter()
        {
            var offset = HeightProjection.RoofOffset(new Vector2(10f, 0f), Vector2.zero, 5f, 0.04f);

            Assert.That(offset.x, Is.EqualTo(2f).Within(1e-5f));
            Assert.That(offset.y, Is.EqualTo(0f));
        }

        [Test]
        public void RoofOffset_AtViewCenter_IsZero()
        {
            Assert.That(HeightProjection.RoofOffset(new Vector2(3f, 4f), new Vector2(3f, 4f), 10f, 0.04f), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void RoofOffset_TallerLeansFurther()
        {
            var low = HeightProjection.RoofOffset(new Vector2(5f, 5f), Vector2.zero, 2f, 0.04f);
            var high = HeightProjection.RoofOffset(new Vector2(5f, 5f), Vector2.zero, 8f, 0.04f);

            Assert.That(high.magnitude, Is.GreaterThan(low.magnitude));
        }

        [Test]
        public void ShadowOffset_FollowsSunAndHeight()
        {
            var offset = HeightProjection.ShadowOffset(new Vector2(0f, -2f), 4f, 0.5f);

            Assert.That(offset, Is.EqualTo(new Vector2(0f, -2f)));
        }

        [Test]
        public void ShadowOffset_ZeroSun_IsZero()
        {
            Assert.That(HeightProjection.ShadowOffset(Vector2.zero, 4f, 0.5f), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ExtrudedRectHull_NoOffset_IsTheRect()
        {
            var hull = new List<Vector2>();

            HeightProjection.ExtrudedRectHull(new Rect(0f, 0f, 2f, 1f), Vector2.zero, hull);

            Assert.That(hull, Has.Count.EqualTo(4));
        }

        [Test]
        public void ExtrudedRectHull_DiagonalOffset_IsHexagon()
        {
            var hull = new List<Vector2>();

            HeightProjection.ExtrudedRectHull(new Rect(0f, 0f, 2f, 2f), new Vector2(1f, 1f), hull);

            Assert.That(hull, Has.Count.EqualTo(6));
            Assert.That(hull, Does.Contain(new Vector2(3f, 3f)));
            Assert.That(hull, Does.Contain(new Vector2(0f, 0f)));
        }

        [Test]
        public void ConvexHull_IsCounterClockwise()
        {
            var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f) };
            var hull = new List<Vector2>();

            HeightProjection.ConvexHull(points, hull);

            var area = 0f;
            for (var i = 0; i < hull.Count; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Count];
                area += a.x * b.y - b.x * a.y;
            }
            Assert.That(hull, Has.Count.EqualTo(4));
            Assert.That(area, Is.GreaterThan(0f));
        }

        [Test]
        public void IsVisibleWall_FacingViewCenter_IsVisible()
        {
            var offset = new Vector2(3f, 0f);

            Assert.That(HeightProjection.IsVisibleWall(Vector2.left, offset), Is.True);
            Assert.That(HeightProjection.IsVisibleWall(Vector2.right, offset), Is.False);
            Assert.That(HeightProjection.IsVisibleWall(Vector2.up, offset), Is.False);
        }

        [Test]
        public void RoofOffset_ZeroHeight_IsZero()
        {
            Assert.That(HeightProjection.RoofOffset(new Vector2(10f, 5f), Vector2.zero, 0f, 0.04f), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ShadowOffset_ZeroHeight_IsZero()
        {
            Assert.That(HeightProjection.ShadowOffset(new Vector2(0.6f, -0.8f), 0f, 0.5f), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ShadowOffset_UnnormalizedSun_UsesDirectionOnly()
        {
            var offset = HeightProjection.ShadowOffset(new Vector2(0f, -10f), 4f, 0.5f);

            Assert.That(offset.y, Is.EqualTo(-2f).Within(1e-5f));
        }

        [Test]
        public void ExtrudedRectHull_AxisAlignedOffset_IsRectangleOfCombinedExtent()
        {
            var hull = new List<Vector2>();

            HeightProjection.ExtrudedRectHull(new Rect(0f, 0f, 2f, 2f), new Vector2(3f, 0f), hull);

            Assert.That(hull, Has.Count.EqualTo(4));
            Assert.That(hull, Does.Contain(new Vector2(5f, 2f)));
        }

        [Test]
        public void ConvexHull_CollinearPoints_ReturnsOnlyEndpoints()
        {
            var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(2f, 2f), new Vector2(3f, 3f) };
            var hull = new List<Vector2>();

            HeightProjection.ConvexHull(points, hull);

            Assert.That(hull, Has.Count.EqualTo(2));
            Assert.That(hull, Does.Contain(new Vector2(0f, 0f)));
            Assert.That(hull, Does.Contain(new Vector2(3f, 3f)));
        }

        [Test]
        public void ConvexHull_DuplicatePoints_AreCollapsed()
        {
            var points = new List<Vector2> { new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f) };
            var hull = new List<Vector2>();

            HeightProjection.ConvexHull(points, hull);

            Assert.That(hull, Has.Count.EqualTo(3));
        }

        [Test]
        public void ConvexHull_FewerThanThreePoints_ReturnsThemUnchanged()
        {
            var points = new List<Vector2> { new Vector2(2f, 2f), new Vector2(1f, 1f) };
            var hull = new List<Vector2>();

            HeightProjection.ConvexHull(points, hull);

            Assert.That(hull, Is.EqualTo(points));
        }

        [Test]
        public void ConvexHull_EmptyInput_ClearsResult()
        {
            var hull = new List<Vector2> { Vector2.one };

            HeightProjection.ConvexHull(new List<Vector2>(), hull);

            Assert.That(hull, Is.Empty);
        }

        [Test]
        public void IsVisibleWall_ZeroOffset_IsNeverVisible()
        {
            Assert.That(HeightProjection.IsVisibleWall(Vector2.left, Vector2.zero), Is.False);
        }
    }
}
