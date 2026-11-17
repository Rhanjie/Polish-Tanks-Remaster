using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.Ground;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>
    /// Tests of how <see cref="GroundIndexMap"/> stores layers per cell and merges cells into collider rectangles.
    /// </summary>
    public class GroundIndexMapTests
    {
        private static GroundIndexMap FromRows(params string[] rowsTopFirst)
        {
            var height = rowsTopFirst.Length;
            var width = rowsTopFirst[0].Length;
            return GroundIndexMap.Build(new RectInt(0, 0, width, height),
                cell => rowsTopFirst[height - 1 - cell.y][cell.x] == '#' ? 1 : 0, 0);
        }

        private static HashSet<Vector2Int> Covered(IEnumerable<RectInt> rects)
        {
            var cells = new HashSet<Vector2Int>();
            foreach (var rect in rects)
            foreach (var position in rect.allPositionsWithin)
                Assert.IsTrue(cells.Add(position), $"Cell {position} is covered twice");
            return cells;
        }

        [Test]
        public void Build_NegativeLayer_UsesFallbackLayer()
        {
            var map = GroundIndexMap.Build(new RectInt(0, 0, 2, 1), cell => cell.x == 0 ? -1 : 3, 5);

            Assert.AreEqual(new byte[] { 5, 3 }, map.Indices);
        }

        [Test]
        public void Build_AnyOrigin_StoresRowsFromBottomLeft()
        {
            var map = GroundIndexMap.Build(new RectInt(5, 7, 2, 2), cell => (cell.x - 5) + 2 * (cell.y - 7), 0);

            Assert.AreEqual(new byte[] { 0, 1, 2, 3 }, map.Indices);
        }

        [Test]
        public void LayerAt_CellOutsideRect_ReturnsNearestEdgeCell()
        {
            var map = GroundIndexMap.Build(new RectInt(0, 0, 2, 2), cell => cell.x + 2 * cell.y, 0);

            Assert.AreEqual(3, map.LayerAt(new Vector2Int(50, 50)));
            Assert.AreEqual(0, map.LayerAt(new Vector2Int(-50, -50)));
        }

        [Test]
        public void MergedRects_SameRunInConsecutiveRows_MergesIntoOneRect()
        {
            var map = FromRows(".##.",
                               ".##.");

            var rects = map.MergedRects(layer => layer == 1);

            Assert.AreEqual(new[] { new RectInt(1, 0, 2, 2) }, rects.ToArray());
        }

        [Test]
        public void MergedRects_LShape_CoversExactlyTheIncludedCells()
        {
            var map = FromRows("#..",
                               "#..",
                               "###");

            var rects = map.MergedRects(layer => layer == 1);

            var expected = new HashSet<Vector2Int>
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(0, 1), new Vector2Int(0, 2),
            };
            Assert.AreEqual(2, rects.Count);
            Assert.IsTrue(expected.SetEquals(Covered(rects)));
        }

        [Test]
        public void MergedRects_EmptyRowBetweenRuns_DoesNotBridgeTheGap()
        {
            var map = FromRows("##",
                               "..",
                               "##");

            var rects = map.MergedRects(layer => layer == 1);

            Assert.AreEqual(2, rects.Count);
            Assert.AreEqual(4, Covered(rects).Count);
        }

        [Test]
        public void MergedRects_NothingIncluded_ReturnsNoRects()
        {
            var map = FromRows("..", "..");

            Assert.IsEmpty(map.MergedRects(layer => layer == 1));
        }
    }
}
