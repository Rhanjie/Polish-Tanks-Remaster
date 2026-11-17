using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.Navigation;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class PathSmootherTests
    {
        private static readonly Rect Area = new Rect(0f, 0f, 20f, 20f);

        [Test]
        public void Smooth_StaircaseOnEmptyGrid_CollapsesToOneLeg()
        {
            var grid = new BoxObstacleSource().BuildGrid(Area);
            var cells = new List<Vector2Int>();
            for (var i = 0; i < 6; i++)
            {
                cells.Add(new Vector2Int(2 + i, 2 + i));
                cells.Add(new Vector2Int(3 + i, 2 + i));
            }

            var points = PathSmoother.Smooth(grid, cells, 0.4f);

            Assert.That(points.Count, Is.EqualTo(2));
        }

        [Test]
        public void Smooth_PathAroundWall_NeverCrossesBlockedCell()
        {
            var grid = new BoxObstacleSource().Solid(new Rect(9.9f, 0f, 0.2f, 10.5f)).BuildGrid(Area);
            var cells = new List<Vector2Int>();
            for (var y = 5; y <= 12; y++) cells.Add(new Vector2Int(8, y));
            for (var x = 9; x <= 11; x++) cells.Add(new Vector2Int(x, 12));
            for (var y = 11; y >= 5; y--) cells.Add(new Vector2Int(11, y));

            var points = PathSmoother.Smooth(grid, cells, 0.4f);

            var crossed = false;
            for (var i = 1; i < points.Count; i++)
            {
                var steps = Mathf.CeilToInt(Vector2.Distance(points[i - 1], points[i]) / 0.25f);
                for (var s = 0; s <= steps; s++)
                {
                    var sample = Vector2.Lerp(points[i - 1], points[i], s / (float)steps);
                    var cell = grid.WorldToCell(sample);
                    var offset = sample - grid.CellToWorld(cell);
                    if (Mathf.Abs(offset.x) < 0.4f && Mathf.Abs(offset.y) < 0.4f && grid.IsBlocked(cell))
                        crossed = true;
                }
            }
            Assert.That(crossed, Is.False);
        }
    }
}
