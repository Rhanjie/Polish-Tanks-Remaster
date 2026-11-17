using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Ground
{
    /// <summary>
    /// Ground layer of every cell in a rectangle of the tilemap, one byte per cell, row by row from the bottom.
    /// Cells with no tile get the fallback layer. It is uploaded as the index texture of the ground shader and also
    /// answers which layer lies under a point and which cells need a vehicle blocking collider.
    /// </summary>
    public sealed class GroundIndexMap
    {
        private readonly byte[] indices;

        private GroundIndexMap(RectInt cells, byte[] indices)
        {
            Cells = cells;
            this.indices = indices;
        }

        public RectInt Cells { get; }
        public byte[] Indices => indices;

        public static GroundIndexMap Build(RectInt cells, Func<Vector2Int, int> layerAt, int fallbackLayer)
        {
            var width = Mathf.Max(cells.width, 1);
            var height = Mathf.Max(cells.height, 1);
            cells = new RectInt(cells.x, cells.y, width, height);
            var fallback = (byte)Mathf.Clamp(fallbackLayer, 0, byte.MaxValue);
            var result = new byte[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var layer = layerAt(new Vector2Int(cells.x + x, cells.y + y));
                result[x + y * width] = layer < 0 ? fallback : (byte)Mathf.Min(layer, byte.MaxValue);
            }
            return new GroundIndexMap(cells, result);
        }

        public int LayerAt(Vector2Int cell)
        {
            var x = Mathf.Clamp(cell.x - Cells.x, 0, Cells.width - 1);
            var y = Mathf.Clamp(cell.y - Cells.y, 0, Cells.height - 1);
            return indices[x + y * Cells.width];
        }

        public List<RectInt> MergedRects(Func<int, bool> include)
        {
            var result = new List<RectInt>();
            var open = new Dictionary<(int, int), int>();
            var nextOpen = new Dictionary<(int, int), int>();
            for (var y = 0; y < Cells.height; y++)
            {
                nextOpen.Clear();
                var x = 0;
                while (x < Cells.width)
                {
                    if (!include(indices[x + y * Cells.width]))
                    {
                        x++;
                        continue;
                    }
                    var start = x;
                    while (x < Cells.width && include(indices[x + y * Cells.width]))
                        x++;
                    var key = (start, x);
                    if (open.TryGetValue(key, out var index))
                    {
                        var rect = result[index];
                        result[index] = new RectInt(rect.x, rect.y, rect.width, rect.height + 1);
                    }
                    else
                    {
                        index = result.Count;
                        result.Add(new RectInt(Cells.x + start, Cells.y + y, x - start, 1));
                    }
                    nextOpen[key] = index;
                }
                (open, nextOpen) = (nextOpen, open);
            }
            return result;
        }
    }
}
