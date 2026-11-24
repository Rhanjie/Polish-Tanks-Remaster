using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Distance from every open cell to the nearest blocked cell (or the grid edge), by a two-pass chamfer transform
    /// with straight and diagonal steps. The result is measured from the cell centre to the near edge of the blocking
    /// cell, so a cell next to a wall has a clearance of half a cell.
    /// </summary>
    public static class ClearanceField
    {
        private const float Diagonal = 1.41421356f;

        public static void Compute(NavGrid grid, bool[] blocked, float[] clearance)
        {
            var width = grid.Width;
            var height = grid.Height;
            var distance = new float[width * height];

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var i = y * width + x;
                distance[i] = blocked[i] ? 0f : Mathf.Min(Mathf.Min(x + 1, y + 1), Mathf.Min(width - x, height - y));
            }

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var i = y * width + x;
                var d = distance[i];
                if (x > 0) d = Mathf.Min(d, distance[i - 1] + 1f);
                if (y > 0) d = Mathf.Min(d, distance[i - width] + 1f);
                if (x > 0 && y > 0) d = Mathf.Min(d, distance[i - width - 1] + Diagonal);
                if (x < width - 1 && y > 0) d = Mathf.Min(d, distance[i - width + 1] + Diagonal);
                distance[i] = d;
            }

            for (var y = height - 1; y >= 0; y--)
            for (var x = width - 1; x >= 0; x--)
            {
                var i = y * width + x;
                var d = distance[i];
                if (x < width - 1) d = Mathf.Min(d, distance[i + 1] + 1f);
                if (y < height - 1) d = Mathf.Min(d, distance[i + width] + 1f);
                if (x < width - 1 && y < height - 1) d = Mathf.Min(d, distance[i + width + 1] + Diagonal);
                if (x > 0 && y < height - 1) d = Mathf.Min(d, distance[i + width - 1] + Diagonal);
                distance[i] = d;
            }

            for (var i = 0; i < distance.Length; i++)
                clearance[i] = blocked[i] ? 0f : Mathf.Max(0f, distance[i] - 0.5f) * grid.CellSize;
        }
    }
}
