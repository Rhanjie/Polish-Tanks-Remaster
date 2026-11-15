using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// Groups the <see cref="Building"/> blocks under it into one building. Every block leans from the compound's
    /// anchor (its own position) instead of its own centre, so joined blocks stay joined, walls hidden by a
    /// neighbour are cut away and the blocks share one sorting step with a layer each, lowest block first.
    /// Blocks must be axis-aligned with the compound; the compound itself may be rotated. The layout is rebuilt
    /// lazily whenever a block moves, resizes or changes its roof.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BuildingCompound : MonoBehaviour
    {
        private readonly List<Building> blocks = new List<Building>(8);
        private readonly List<BlockShape> shapes = new List<BlockShape>(8);
        private readonly List<List<WallSegment>> walls = new List<List<WallSegment>>(8);
        private readonly List<List<GableEnd>> gableEnds = new List<List<GableEnd>>(8);
        private readonly List<Building> scratchBlocks = new List<Building>(8);
        private int[] ranks = new int[8];
        private bool dirty = true;
        private int lastRefreshFrame = -1;

        public Vector2 Anchor => transform.position;
        public IReadOnlyList<Building> Blocks
        {
            get
            {
                Refresh();
                return blocks;
            }
        }

        public int IndexOf(Building block)
        {
            Refresh();
            return blocks.IndexOf(block);
        }

        public BlockShape ShapeOf(int index)
        {
            Refresh();
            return shapes[index];
        }

        public int LayerOf(int index)
        {
            Refresh();
            return ranks[index];
        }

        public IReadOnlyList<WallSegment> WallsOf(int index)
        {
            Refresh();
            return walls[index];
        }

        public IReadOnlyList<GableEnd> GableEndsOf(int index)
        {
            Refresh();
            return gableEnds[index];
        }

        public void MarkDirty() => dirty = true;

        public void Refresh()
        {
            if (!dirty && Application.isPlaying && lastRefreshFrame == Time.frameCount)
                return;
            lastRefreshFrame = Time.frameCount;
            GetComponentsInChildren(false, scratchBlocks);
            if (!dirty && !BlocksChanged())
                return;
            dirty = false;
            Rebuild();
        }

        private void OnEnable() => dirty = true;

        private void OnTransformChildrenChanged() => dirty = true;

        private bool BlocksChanged()
        {
            if (scratchBlocks.Count != blocks.Count)
                return true;
            for (var i = 0; i < blocks.Count; i++)
                if (scratchBlocks[i] != blocks[i] || !ShapeFor(blocks[i]).SameAs(shapes[i]))
                    return true;
            return false;
        }

        private void Rebuild()
        {
            blocks.Clear();
            blocks.AddRange(scratchBlocks);
            shapes.Clear();
            foreach (var block in blocks)
                shapes.Add(ShapeFor(block));

            if (ranks.Length < blocks.Count)
                ranks = new int[blocks.Count];
            CompoundLayout.Ranks(shapes, ranks, HeightProjection.BuildingLayers - 1);

            while (walls.Count < blocks.Count)
            {
                walls.Add(new List<WallSegment>(8));
                gableEnds.Add(new List<GableEnd>(2));
            }
            for (var i = 0; i < blocks.Count; i++)
            {
                CompoundLayout.WallSegments(shapes, i, walls[i]);
                CompoundLayout.GableEnds(shapes, i, gableEnds[i]);
            }
        }

        private BlockShape ShapeFor(Building block)
        {
            var half = block.Size * 0.5f;
            var t = block.transform;
            Vector2 a = transform.InverseTransformPoint(t.TransformPoint(new Vector3(-half.x, -half.y)));
            Vector2 b = transform.InverseTransformPoint(t.TransformPoint(new Vector3(half.x, half.y)));
            var rect = Rect.MinMaxRect(Round(Mathf.Min(a.x, b.x)), Round(Mathf.Min(a.y, b.y)), Round(Mathf.Max(a.x, b.x)), Round(Mathf.Max(a.y, b.y)));
            return new BlockShape(rect, block.Height, block.Roof, block.RidgeHeight);
        }

        private static float Round(float value) => Mathf.Round(value * 1000f) / 1000f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f);
            var anchor = transform.position;
            Gizmos.DrawLine(anchor + Vector3.left, anchor + Vector3.right);
            Gizmos.DrawLine(anchor + Vector3.down, anchor + Vector3.up);
        }
    }
}
