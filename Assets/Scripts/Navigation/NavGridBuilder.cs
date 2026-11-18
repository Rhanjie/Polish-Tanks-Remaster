using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Rasterises obstacles into a <see cref="NavGrid"/>. A cell counts as touched when the obstacle comes within half
    /// a cell of its centre, so thin walls and edge colliders still block. Clearance is rebuilt at the end.
    /// </summary>
    public static class NavGridBuilder
    {
        public static NavGrid Build(Rect area, float cellSize, IObstacleSource source)
        {
            var grid = NavGrid.Covering(area, cellSize);
            var reach = cellSize * 0.5f;
            foreach (var obstacle in source.Collect(area))
                Stamp(grid, obstacle, reach);
            grid.RebuildClearance();
            return grid;
        }

        private static void Stamp(NavGrid grid, Obstacle obstacle, float reach)
        {
            var min = grid.WorldToCell(obstacle.Bounds.min - Vector2.one * reach);
            var max = grid.WorldToCell(obstacle.Bounds.max + Vector2.one * reach);
            min = Vector2Int.Max(min, Vector2Int.zero);
            max = Vector2Int.Min(max, new Vector2Int(grid.Width - 1, grid.Height - 1));

            for (var y = min.y; y <= max.y; y++)
            for (var x = min.x; x <= max.x; x++)
            {
                var cell = new Vector2Int(x, y);
                if (obstacle.DistanceTo(grid.CellToWorld(cell)) > reach)
                    continue;
                if (obstacle.Kind == ObstacleKind.Solid)
                    grid.SetBlocked(cell, true);
                else
                    grid.AddSoftCost(cell, obstacle.SoftCost);
            }
        }
    }
}
