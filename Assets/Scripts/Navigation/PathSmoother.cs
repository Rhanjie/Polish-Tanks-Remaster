using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Turns a staircase of grid cells into a few straight legs: from each kept point it keeps stretching the leg
    /// along the path while the straight line stays clear for an agent of the given radius. A shortcut never crosses a
    /// blocked cell, a cell too tight for the agent, a soft obstacle the original path went round, or a cell closer to
    /// a wall than both of its ends (up to the preferred clearance), so it doesn't pull the path back against walls.
    /// </summary>
    public static class PathSmoother
    {
        private const float SampleStep = 0.25f;

        public static List<Vector2> Smooth(NavGrid grid, IReadOnlyList<Vector2Int> cells, float agentRadius, float preferredClearance = 0f)
        {
            var points = new List<Vector2>();
            if (cells.Count == 0)
                return points;

            var anchor = 0;
            points.Add(grid.CellToWorld(cells[0]));
            while (anchor < cells.Count - 1)
            {
                var next = anchor + 1;
                for (var candidate = anchor + 2; candidate < cells.Count; candidate++)
                {
                    if (!IsClear(grid, cells[anchor], cells[candidate], agentRadius, preferredClearance))
                        break;
                    next = candidate;
                }
                points.Add(grid.CellToWorld(cells[next]));
                anchor = next;
            }
            return points;
        }

        public static bool IsClear(NavGrid grid, Vector2Int from, Vector2Int to, float agentRadius, float preferredClearance = 0f)
        {
            var allowedCost = Mathf.Max(grid.SoftCost(from), grid.SoftCost(to));
            var neededClearance = Mathf.Max(agentRadius, Mathf.Min(preferredClearance, Mathf.Min(grid.Clearance(from), grid.Clearance(to))));
            var a = new Vector2(from.x, from.y);
            var b = new Vector2(to.x, to.y);
            var steps = Mathf.CeilToInt(Vector2.Distance(a, b) / SampleStep);
            for (var i = 1; i < steps; i++)
            {
                var point = Vector2.Lerp(a, b, i / (float)steps);
                var cell = new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));
                if (!grid.IsWalkable(cell, neededClearance) || grid.SoftCost(cell) > allowedCost)
                    return false;
            }
            return true;
        }
    }
}
