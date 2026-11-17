using System;
using NUnit.Framework;
using PolishTanks.Navigation;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class GridNavigationTests
    {
        private static readonly Rect Area = new Rect(0f, 0f, 30f, 20f);
        private static readonly Vector2 Start = new Vector2(3.5f, 3.5f);
        private static readonly Vector2 Goal = new Vector2(26.5f, 3.5f);

        private static PathResult Find(NavGrid grid, Vector2 from, Vector2 to, float radius = 0.4f, float softWeight = 1f)
        {
            return new GridNavigation(grid).FindPath(new PathRequest(from, to, radius, softWeight));
        }

        private static bool CrossesDeepInside(NavGrid grid, Vector2 from, PathResult result, Func<Vector2Int, bool> predicate)
        {
            var previous = from;
            foreach (var point in result.Points)
            {
                var steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(previous, point) / 0.25f));
                for (var i = 0; i <= steps; i++)
                {
                    var sample = Vector2.Lerp(previous, point, i / (float)steps);
                    var cell = grid.WorldToCell(sample);
                    var offset = sample - grid.CellToWorld(cell);
                    if (Mathf.Abs(offset.x) < 0.4f && Mathf.Abs(offset.y) < 0.4f && predicate(cell))
                        return true;
                }
                previous = point;
            }
            return false;
        }

        [Test]
        public void FindPath_WallInTheWay_GoesAroundAndEndsAtGoal()
        {
            var grid = new BoxObstacleSource().Solid(new Rect(14.9f, 0f, 0.2f, 14f)).BuildGrid(Area);

            var result = Find(grid, Start, Goal);

            Assert.That(result.Status, Is.EqualTo(PathStatus.Found));
            Assert.That(result.Points[result.Points.Count - 1], Is.EqualTo(Goal));
            Assert.That(CrossesDeepInside(grid, Start, result, grid.IsBlocked), Is.False);
        }

        [Test]
        public void FindPath_TwoMetreGap_FitsSmallAgentButNotTank()
        {
            var grid = new BoxObstacleSource()
                .Solid(new Rect(14.9f, 0f, 0.2f, 8.9f))
                .Solid(new Rect(14.9f, 11.1f, 0.2f, 8.9f))
                .BuildGrid(Area);

            var soldier = Find(grid, Start, Goal, 0.4f);
            var tank = Find(grid, Start, Goal, 1.6f);

            Assert.That(soldier.Status, Is.EqualTo(PathStatus.Found));
            Assert.That(tank.Status, Is.EqualTo(PathStatus.Partial));
        }

        [Test]
        public void FindPath_GoalWalledOff_EndsAtReachableCellClosestToGoal()
        {
            var grid = new BoxObstacleSource().Solid(new Rect(14.9f, 0f, 0.2f, 20f)).BuildGrid(Area);

            var result = Find(grid, Start, Goal);

            Assert.That(result.Status, Is.EqualTo(PathStatus.Partial));
            Assert.That(Vector2.Distance(result.Points[result.Points.Count - 1], new Vector2(13.5f, 3.5f)), Is.LessThan(0.01f));
        }

        [Test]
        public void FindPath_GoalInsideObstacle_EndsInWalkableCellNextToIt()
        {
            var grid = new BoxObstacleSource().Solid(new Rect(20f, 5f, 4f, 4f)).BuildGrid(Area);

            var result = Find(grid, Start, new Vector2(22f, 7f));
            var end = result.Points[result.Points.Count - 1];

            Assert.That(result.Status, Is.EqualTo(PathStatus.Partial));
            Assert.That(grid.IsWalkable(grid.WorldToCell(end), 0.4f), Is.True);
        }

        [Test]
        public void FindPath_ShortDetourAroundCrates_AvoidsThem()
        {
            var grid = new BoxObstacleSource().Soft(new Rect(14.9f, 2f, 0.2f, 3f), 3f).BuildGrid(Area);

            var result = Find(grid, Start, Goal);

            Assert.That(CrossesDeepInside(grid, Start, result, cell => grid.SoftCost(cell) > 0f), Is.False);
        }

        [Test]
        public void FindPath_VeryLongDetourAroundCrates_DrivesThroughThem()
        {
            var grid = new BoxObstacleSource().Soft(new Rect(14.9f, 0f, 0.2f, 17.5f), 3f).BuildGrid(Area);

            var result = Find(grid, Start, Goal);

            Assert.That(CrossesDeepInside(grid, Start, result, cell => grid.SoftCost(cell) > 0f), Is.True);
        }

        [Test]
        public void FindPath_WhileRecording_TracesExpandedCellsFromStart()
        {
            var grid = new BoxObstacleSource().Solid(new Rect(14.9f, 0f, 0.2f, 14f)).BuildGrid(Area);
            var navigation = new GridNavigation(grid) { RecordSearches = true };

            var result = navigation.FindPath(new PathRequest(Start, Goal, 0.4f));

            var trace = navigation.LastSearch;
            Assert.That(trace.Result, Is.SameAs(result));
            Assert.That(trace.ExpandedCells[0], Is.EqualTo(grid.Index(grid.WorldToCell(Start))));
            Assert.That(trace.ExpandedCells, Has.Member(grid.Index(grid.WorldToCell(Goal))));
        }
    }
}
