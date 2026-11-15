using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// Geometry of a building compound made of axis-aligned blocks: which parts of each block's walls stay
    /// visible next to its neighbours, roof surface heights (gable roofs slope from eaves to ridge) and the
    /// drawing rank of each block (lower blocks first, so taller walls and roofs land on top; a wing whose ridge runs
    /// into a neighbour's roof draws right after that neighbour, so the valley stays on top).
    /// All points are in compound space.
    /// </summary>
    public static class CompoundLayout
    {
        private const float ProbeDistance = 0.01f;
        private const float Epsilon = 1e-4f;

        private static readonly List<float> Breaks = new List<float>(16);
        private static readonly List<int> Supports = new List<int>(8);
        private static readonly List<int> Order = new List<int>(16);
        private static readonly List<float> SortHeights = new List<float>(16);
        private static readonly List<bool> Extends = new List<bool>(16);

        public static float SurfaceHeight(in BlockShape block, Vector2 point)
        {
            if (!block.HasGable)
                return block.Height;
            var rect = block.Rect;
            var halfDepth = block.RidgeAlongX ? rect.height * 0.5f : rect.width * 0.5f;
            if (halfDepth <= 0f)
                return block.TopHeight;
            var fromRidge = block.RidgeAlongX ? Mathf.Abs(point.y - rect.center.y) : Mathf.Abs(point.x - rect.center.x);
            return block.Height + block.RidgeHeight * Mathf.Clamp01(1f - fromRidge / halfDepth);
        }

        public static float SupportHeight(IReadOnlyList<BlockShape> blocks, int self, Vector2 point)
        {
            var height = 0f;
            for (var i = 0; i < blocks.Count; i++)
                if (i != self && blocks[i].Rect.Contains(point))
                    height = Mathf.Max(height, SurfaceHeight(blocks[i], point));
            return height;
        }

        public static void WallSegments(IReadOnlyList<BlockShape> blocks, int index, List<WallSegment> result)
        {
            result.Clear();
            var block = blocks[index];
            var rect = block.Rect;
            var top = block.Height;
            if (top <= 0f)
                return;

            for (var edge = 0; edge < 4; edge++)
            {
                GetEdge(rect, edge, out var a, out var b, out var normal);
                CollectBreaks(blocks, index, a, b);
                for (var i = 0; i < Breaks.Count - 1; i++)
                {
                    var t0 = Breaks[i];
                    var t1 = Breaks[i + 1];
                    if (t1 - t0 < Epsilon)
                        continue;
                    var p0 = Vector2.Lerp(a, b, t0);
                    var p1 = Vector2.Lerp(a, b, t1);
                    var outside = normal * ProbeDistance;
                    CollectSupports(blocks, index, (p0 + p1) * 0.5f + outside);
                    var bottom0 = Mathf.Min(SupportAt(blocks, p0 + outside), top);
                    var bottom1 = Mathf.Min(SupportAt(blocks, p1 + outside), top);
                    if (bottom0 >= top - Epsilon && bottom1 >= top - Epsilon)
                        continue;
                    result.Add(new WallSegment(p0, p1, normal, bottom0, bottom1, top));
                }
            }
        }

        public static void GableEnds(IReadOnlyList<BlockShape> blocks, int index, List<GableEnd> result)
        {
            result.Clear();
            var block = blocks[index];
            if (!block.HasGable)
                return;

            var first = block.RidgeAlongX ? 1 : 0;
            for (var edge = first; edge < 4; edge += 2)
            {
                GetEdge(block.Rect, edge, out var a, out var b, out var normal);
                var probe = (a + b) * 0.5f + normal * ProbeDistance;
                if (SupportHeight(blocks, index, probe) >= block.Height - Epsilon)
                    continue;
                result.Add(new GableEnd(a, b, normal, block.Height, block.TopHeight));
            }
        }

        public static Vector2 RidgeExtensions(IReadOnlyList<BlockShape> blocks, int index)
        {
            var block = blocks[index];
            if (!block.HasGable)
                return Vector2.zero;
            var rect = block.Rect;
            var along = block.RidgeAlongX ? Vector2.right : Vector2.up;
            var start = block.RidgeAlongX ? new Vector2(rect.xMin, rect.center.y) : new Vector2(rect.center.x, rect.yMin);
            var end = block.RidgeAlongX ? new Vector2(rect.xMax, rect.center.y) : new Vector2(rect.center.x, rect.yMax);
            return new Vector2(RidgeExtension(blocks, index, start, -along, out _), RidgeExtension(blocks, index, end, along, out _));
        }

        private static float RidgeExtension(IReadOnlyList<BlockShape> blocks, int self, Vector2 ridgeEnd, Vector2 outward, out int into)
        {
            var block = blocks[self];
            var probe = ridgeEnd + outward * ProbeDistance;
            var extension = float.PositiveInfinity;
            into = -1;
            for (var i = 0; i < blocks.Count; i++)
            {
                var neighbour = blocks[i];
                if (i == self || !neighbour.HasGable || neighbour.RidgeAlongX == block.RidgeAlongX || !neighbour.Rect.Contains(probe))
                    continue;
                var rise = SurfaceHeight(neighbour, probe);
                if (rise < block.Height - Epsilon)
                    continue;
                if (rise >= block.TopHeight)
                {
                    extension = 0f;
                    into = -1;
                    continue;
                }
                var halfDepth = neighbour.RidgeAlongX ? neighbour.Rect.height * 0.5f : neighbour.Rect.width * 0.5f;
                var toRidge = Mathf.Abs(Vector2.Dot(neighbour.Rect.center - ridgeEnd, outward));
                var slope = neighbour.RidgeHeight / halfDepth;
                var reach = Mathf.Min(toRidge, (block.TopHeight - rise) / slope);
                if (reach < extension)
                {
                    extension = reach;
                    into = i;
                }
            }
            if (float.IsPositiveInfinity(extension) || extension <= 0f)
            {
                into = -1;
                return 0f;
            }
            return extension;
        }

        public static void Ranks(IReadOnlyList<BlockShape> blocks, int[] ranks, int maxRank)
        {
            Order.Clear();
            SortHeights.Clear();
            Extends.Clear();
            for (var i = 0; i < blocks.Count; i++)
            {
                Order.Add(i);
                var height = blocks[i].TopHeight;
                var extends = false;
                if (blocks[i].HasGable)
                {
                    var rect = blocks[i].Rect;
                    var along = blocks[i].RidgeAlongX ? Vector2.right : Vector2.up;
                    var start = blocks[i].RidgeAlongX ? new Vector2(rect.xMin, rect.center.y) : new Vector2(rect.center.x, rect.yMin);
                    var end = blocks[i].RidgeAlongX ? new Vector2(rect.xMax, rect.center.y) : new Vector2(rect.center.x, rect.yMax);
                    RidgeExtension(blocks, i, start, -along, out var intoStart);
                    RidgeExtension(blocks, i, end, along, out var intoEnd);
                    if (intoStart >= 0)
                        height = Mathf.Max(height, blocks[intoStart].TopHeight);
                    if (intoEnd >= 0)
                        height = Mathf.Max(height, blocks[intoEnd].TopHeight);
                    extends = intoStart >= 0 || intoEnd >= 0;
                }
                SortHeights.Add(height);
                Extends.Add(extends);
            }
            Order.Sort((x, y) =>
            {
                var byHeight = SortHeights[x].CompareTo(SortHeights[y]);
                if (byHeight != 0)
                    return byHeight;
                var byExtension = Extends[x].CompareTo(Extends[y]);
                return byExtension != 0 ? byExtension : x.CompareTo(y);
            });
            for (var i = 0; i < Order.Count; i++)
                ranks[Order[i]] = Mathf.Min(i, maxRank);
        }

        public static void GetEdge(Rect rect, int edge, out Vector2 a, out Vector2 b, out Vector2 normal)
        {
            switch (edge)
            {
                case 0:
                    a = new Vector2(rect.xMin, rect.yMin);
                    b = new Vector2(rect.xMax, rect.yMin);
                    normal = Vector2.down;
                    break;
                case 1:
                    a = new Vector2(rect.xMax, rect.yMin);
                    b = new Vector2(rect.xMax, rect.yMax);
                    normal = Vector2.right;
                    break;
                case 2:
                    a = new Vector2(rect.xMax, rect.yMax);
                    b = new Vector2(rect.xMin, rect.yMax);
                    normal = Vector2.up;
                    break;
                default:
                    a = new Vector2(rect.xMin, rect.yMax);
                    b = new Vector2(rect.xMin, rect.yMin);
                    normal = Vector2.left;
                    break;
            }
        }

        private static void CollectBreaks(IReadOnlyList<BlockShape> blocks, int self, Vector2 a, Vector2 b)
        {
            Breaks.Clear();
            Breaks.Add(0f);
            Breaks.Add(1f);
            var horizontal = Mathf.Abs(a.y - b.y) < Epsilon;
            var start = horizontal ? a.x : a.y;
            var length = horizontal ? b.x - a.x : b.y - a.y;
            if (Mathf.Abs(length) < Epsilon)
                return;
            for (var i = 0; i < blocks.Count; i++)
            {
                if (i == self)
                    continue;
                var rect = blocks[i].Rect;
                AddBreak(((horizontal ? rect.xMin : rect.yMin) - start) / length);
                AddBreak(((horizontal ? rect.xMax : rect.yMax) - start) / length);
                if (blocks[i].HasGable)
                    AddBreak(((horizontal ? rect.center.x : rect.center.y) - start) / length);
            }
            Breaks.Sort();
        }

        private static void AddBreak(float t)
        {
            if (t > Epsilon && t < 1f - Epsilon)
                Breaks.Add(t);
        }

        private static void CollectSupports(IReadOnlyList<BlockShape> blocks, int self, Vector2 probe)
        {
            Supports.Clear();
            for (var i = 0; i < blocks.Count; i++)
                if (i != self && blocks[i].Rect.Contains(probe))
                    Supports.Add(i);
        }

        private static float SupportAt(IReadOnlyList<BlockShape> blocks, Vector2 point)
        {
            var height = 0f;
            foreach (var i in Supports)
                height = Mathf.Max(height, SurfaceHeight(blocks[i], point));
            return height;
        }
    }
}
