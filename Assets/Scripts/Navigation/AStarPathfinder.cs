using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// A* over a <see cref="NavGrid"/> in eight directions. Diagonals never cut a corner, so a tank doesn't clip a
    /// building's edge. A cell is open to an agent when its clearance covers the agent's radius, and costs more when
    /// it is closer to a wall than the agent's preferred clearance; soft cost makes
    /// crates a detour worth taking unless going round is much longer. A start or goal inside an obstacle (a tank
    /// parked against a wall, a goal set on a building) moves to the nearest open cell first. When the goal can't be
    /// reached the result is <see cref="PathStatus.Partial"/>, ending at the reachable cell closest to it.
    /// One instance is not thread-safe; it reuses its buffers between searches.
    /// </summary>
    public sealed class AStarPathfinder
    {
        public const int DefaultSearchRadius = 12;

        private const float Diagonal = 1.41421356f;
        private const float NearWallPenalty = 1.5f;

        private static readonly Vector2Int[] Directions =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
        };

        private readonly NavGrid grid;
        private readonly MinHeap open = new MinHeap();
        private readonly float[] cost;
        private readonly int[] parent;
        private readonly int[] seen;
        private readonly int[] closed;
        private int stamp;

        public AStarPathfinder(NavGrid grid, int maxExpandedCells = 0)
        {
            this.grid = grid;
            MaxExpandedCells = maxExpandedCells > 0 ? maxExpandedCells : grid.CellCount;
            cost = new float[grid.CellCount];
            parent = new int[grid.CellCount];
            seen = new int[grid.CellCount];
            closed = new int[grid.CellCount];
        }

        public NavGrid Grid => grid;

        public int MaxExpandedCells { get; set; }

        public PathResult FindPath(PathRequest request)
        {
            var radius = request.AgentRadius;
            if (!TryFindNearestWalkable(grid.WorldToCell(request.From), radius, out var start))
                return PathResult.None;
            var goalCell = grid.WorldToCell(request.To);
            var goalReachable = TryFindNearestWalkable(goalCell, radius, out var goal);
            if (!goalReachable)
                goal = goalCell;

            var cells = Search(start, goal, radius, request.SoftCostWeight, request.PreferredClearance, out var reachedGoal);
            var status = reachedGoal && goalReachable && goal == goalCell ? PathStatus.Found : PathStatus.Partial;
            var points = PathSmoother.Smooth(grid, cells, radius, request.PreferredClearance);
            if (status == PathStatus.Found)
                points[points.Count - 1] = request.To;
            if (points.Count > 1 && start == grid.WorldToCell(request.From))
                points.RemoveAt(0);
            return new PathResult(status, points);
        }

        public bool TryFindNearestWalkable(Vector2Int origin, float radius, out Vector2Int found, int searchRadius = DefaultSearchRadius)
        {
            found = origin;
            if (grid.IsWalkable(origin, radius))
                return true;

            for (var ring = 1; ring <= searchRadius; ring++)
            {
                var best = float.MaxValue;
                for (var dy = -ring; dy <= ring; dy++)
                for (var dx = -ring; dx <= ring; dx++)
                {
                    if (Mathf.Abs(dx) != ring && Mathf.Abs(dy) != ring)
                        continue;
                    var cell = new Vector2Int(origin.x + dx, origin.y + dy);
                    if (!grid.IsWalkable(cell, radius))
                        continue;
                    var distance = dx * dx + dy * dy;
                    if (distance >= best)
                        continue;
                    best = distance;
                    found = cell;
                }
                if (best < float.MaxValue)
                    return true;
            }
            return false;
        }

        private List<Vector2Int> Search(Vector2Int start, Vector2Int goal, float radius, float softWeight, float preferredClearance, out bool reachedGoal)
        {
            stamp++;
            open.Clear();
            var startIndex = grid.Index(start);
            var goalIndex = grid.InBounds(goal) ? grid.Index(goal) : -1;
            Visit(startIndex, 0f, -1);
            open.Push(startIndex, Heuristic(start, goal));

            var closest = startIndex;
            var closestHeuristic = Heuristic(start, goal);
            var expanded = 0;
            reachedGoal = false;

            while (open.Count > 0 && expanded < MaxExpandedCells)
            {
                var current = open.Pop();
                if (closed[current] == stamp)
                    continue;
                closed[current] = stamp;
                expanded++;

                if (current == goalIndex)
                {
                    reachedGoal = true;
                    closest = current;
                    break;
                }

                var cell = grid.CellAt(current);
                var h = Heuristic(cell, goal);
                if (h < closestHeuristic)
                {
                    closestHeuristic = h;
                    closest = current;
                }

                foreach (var direction in Directions)
                {
                    var next = cell + direction;
                    if (!grid.IsWalkable(next, radius))
                        continue;
                    var diagonal = direction.x != 0 && direction.y != 0;
                    if (diagonal && (!grid.IsWalkable(new Vector2Int(next.x, cell.y), radius) || !grid.IsWalkable(new Vector2Int(cell.x, next.y), radius)))
                        continue;

                    var nextIndex = grid.Index(next);
                    if (closed[nextIndex] == stamp)
                        continue;
                    var step = (diagonal ? Diagonal : 1f) * (1f + grid.SoftCost(next) * softWeight);
                    if (grid.Clearance(next) < preferredClearance)
                        step *= NearWallPenalty;
                    var newCost = cost[current] + step;
                    if (seen[nextIndex] == stamp && newCost >= cost[nextIndex])
                        continue;
                    Visit(nextIndex, newCost, current);
                    open.Push(nextIndex, newCost + Heuristic(next, goal));
                }
            }

            return Reconstruct(closest);
        }

        private void Visit(int index, float newCost, int from)
        {
            seen[index] = stamp;
            cost[index] = newCost;
            parent[index] = from;
        }

        private List<Vector2Int> Reconstruct(int end)
        {
            var cells = new List<Vector2Int>();
            for (var index = end; index >= 0; index = parent[index])
                cells.Add(grid.CellAt(index));
            cells.Reverse();
            return cells;
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            var dx = Mathf.Abs(a.x - b.x);
            var dy = Mathf.Abs(a.y - b.y);
            return Mathf.Max(dx, dy) + (Diagonal - 1f) * Mathf.Min(dx, dy);
        }
    }
}
