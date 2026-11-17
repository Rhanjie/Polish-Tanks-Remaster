using NUnit.Framework;
using PolishTanks.Navigation;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class ClearanceFieldTests
    {
        private static readonly Rect Area = new Rect(0f, 0f, 9f, 9f);

        [Test]
        public void Compute_CellNextToWall_HasHalfCellClearance()
        {
            var grid = new BoxObstacleSource().Solid(new Rect(4.9f, 0f, 0.2f, 9f)).BuildGrid(Area);

            Assert.That(grid.Clearance(new Vector2Int(6, 4)), Is.EqualTo(0.5f).Within(0.01f));
        }

        [Test]
        public void Compute_CellFurtherFromWall_HasLargerClearance()
        {
            var grid = new BoxObstacleSource().Solid(new Rect(4.9f, 0f, 0.2f, 9f)).BuildGrid(Area);

            Assert.That(grid.Clearance(new Vector2Int(7, 4)), Is.GreaterThan(grid.Clearance(new Vector2Int(6, 4))));
        }

        [Test]
        public void Compute_GridEdge_CountsAsWall()
        {
            var grid = new BoxObstacleSource().BuildGrid(Area);

            Assert.That(grid.Clearance(new Vector2Int(0, 4)), Is.EqualTo(0.5f).Within(0.01f));
        }
    }
}
