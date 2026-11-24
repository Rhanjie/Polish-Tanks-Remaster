using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.Background;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class SilhouetteBuilderTests
    {
        private static void AssertValidMesh(List<Vector3> vertices, List<int> triangles)
        {
            Assert.AreEqual(0, triangles.Count % 3);
            foreach (var index in triangles)
                Assert.That(index, Is.InRange(0, vertices.Count - 1));
        }

        [Test]
        public void BuildHills_SameSeed_ProducesIdenticalGeometry()
        {
            var hills = HillSettings.Default;
            var v1 = new List<Vector3>();
            var t1 = new List<int>();
            var v2 = new List<Vector3>();
            var t2 = new List<int>();

            SilhouetteBuilder.BuildHills(hills, 42, v1, t1);
            SilhouetteBuilder.BuildHills(hills, 42, v2, t2);

            CollectionAssert.AreEqual(v1, v2);
            CollectionAssert.AreEqual(t1, t2);
        }

        [Test]
        public void BuildHills_DifferentSeeds_ProduceDifferentRidge()
        {
            var hills = HillSettings.Default;
            var v1 = new List<Vector3>();
            var v2 = new List<Vector3>();

            SilhouetteBuilder.BuildHills(hills, 1, v1, new List<int>());
            SilhouetteBuilder.BuildHills(hills, 2, v2, new List<int>());

            CollectionAssert.AreNotEqual(v1, v2);
        }

        [TestCase(8)]
        [TestCase(128)]
        public void BuildHills_Segments_VertexCountIsTwoPerColumn(int segments)
        {
            var hills = HillSettings.Default;
            hills.segments = segments;
            var vertices = new List<Vector3>();

            SilhouetteBuilder.BuildHills(hills, 5, vertices, new List<int>());

            Assert.AreEqual(2 * (segments + 1), vertices.Count);
        }

        [Test]
        public void BuildHills_Default_TriangleIndicesAreValid()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            SilhouetteBuilder.BuildHills(HillSettings.Default, 9, vertices, triangles);

            AssertValidMesh(vertices, triangles);
            Assert.AreEqual(HillSettings.Default.segments * 6, triangles.Count);
        }

        [Test]
        public void BuildHills_ZeroSegments_ClampsToOneSegment()
        {
            var hills = HillSettings.Default;
            hills.segments = 0;
            var vertices = new List<Vector3>();

            SilhouetteBuilder.BuildHills(hills, 1, vertices, new List<int>());

            Assert.AreEqual(4, vertices.Count);
        }

        [Test]
        public void BuildHills_RidgeFarBelowBottom_NeverGoesBelowBottom()
        {
            var hills = HillSettings.Default;
            hills.baseHeight = -20f;
            var vertices = new List<Vector3>();

            SilhouetteBuilder.BuildHills(hills, 3, vertices, new List<int>());

            foreach (var v in vertices)
                Assert.GreaterOrEqual(v.y, hills.bottom);
        }

        [Test]
        public void BuildTrees_SameSeed_ProducesIdenticalGeometry()
        {
            var v1 = new List<Vector3>();
            var t1 = new List<int>();
            var v2 = new List<Vector3>();
            var t2 = new List<int>();

            SilhouetteBuilder.BuildTrees(HillSettings.Default, TreeSettings.Default, 11, v1, t1);
            SilhouetteBuilder.BuildTrees(HillSettings.Default, TreeSettings.Default, 11, v2, t2);

            CollectionAssert.AreEqual(v1, v2);
            CollectionAssert.AreEqual(t1, t2);
        }

        [Test]
        public void BuildTrees_Default_TriangleIndicesAreValid()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            SilhouetteBuilder.BuildTrees(HillSettings.Default, TreeSettings.Default, 11, vertices, triangles);

            Assert.Greater(vertices.Count, 0);
            AssertValidMesh(vertices, triangles);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void BuildTrees_NonPositiveCount_AddsNoGeometry(int count)
        {
            var trees = TreeSettings.Default;
            trees.count = count;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            SilhouetteBuilder.BuildTrees(HillSettings.Default, trees, 1, vertices, triangles);

            Assert.AreEqual(0, vertices.Count);
            Assert.AreEqual(0, triangles.Count);
        }

        [Test]
        public void BuildTrees_MinHeightAboveMaxHeight_DoesNotThrow()
        {
            var trees = TreeSettings.Default;
            trees.minHeight = 2f;
            trees.maxHeight = 0.5f;

            Assert.DoesNotThrow(() =>
                SilhouetteBuilder.BuildTrees(HillSettings.Default, trees, 1, new List<Vector3>(), new List<int>()));
        }

        [Test]
        public void BuildTrees_ListsAlreadyFilled_IndicesOffsetPastExistingVertices()
        {
            var trees = TreeSettings.Default;
            trees.roundTreeShare = 1f;
            var vertices = new List<Vector3> { Vector3.zero, Vector3.one, Vector3.up };
            var triangles = new List<int>();

            SilhouetteBuilder.BuildTrees(HillSettings.Default, trees, 4, vertices, triangles);

            AssertValidMesh(vertices, triangles);
            Assert.That(triangles, Has.None.LessThan(3));
        }
    }
}
