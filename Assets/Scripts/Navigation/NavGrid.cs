using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Square cells over a world rectangle. Each cell is blocked or open, may carry a soft cost (crates and barrels a
    /// tank can shove through but would rather not) and knows its clearance: how far its centre is from the nearest
    /// blocked cell or the grid edge, in world units. Clearance lets one grid serve a tank and a soldier alike, an
    /// agent fits wherever clearance is at least its radius.
    /// </summary>
    public sealed class NavGrid
    {
        private readonly bool[] blocked;
        private readonly float[] softCost;
        private readonly float[] clearance;

        public NavGrid(Vector2 origin, int width, int height, float cellSize)
        {
            Origin = origin;
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            CellSize = Mathf.Max(0.01f, cellSize);
            blocked = new bool[Width * Height];
            softCost = new float[Width * Height];
            clearance = new float[Width * Height];
        }

        public static NavGrid Covering(Rect area, float cellSize)
        {
            var width = Mathf.CeilToInt(area.width / cellSize);
            var height = Mathf.CeilToInt(area.height / cellSize);
            return new NavGrid(area.min, width, height, cellSize);
        }

        public Vector2 Origin { get; }
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public int CellCount => Width * Height;
        public Rect Area => new Rect(Origin, new Vector2(Width * CellSize, Height * CellSize));

        public bool InBounds(Vector2Int cell) => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;

        public int Index(Vector2Int cell) => cell.y * Width + cell.x;

        public Vector2Int CellAt(int index) => new Vector2Int(index % Width, index / Width);

        public Vector2Int WorldToCell(Vector2 position)
        {
            var local = (position - Origin) / CellSize;
            return new Vector2Int(Mathf.FloorToInt(local.x), Mathf.FloorToInt(local.y));
        }

        public Vector2 CellToWorld(Vector2Int cell) => Origin + (new Vector2(cell.x, cell.y) + Vector2.one * 0.5f) * CellSize;

        public bool IsBlocked(Vector2Int cell) => !InBounds(cell) || blocked[Index(cell)];

        public float SoftCost(Vector2Int cell) => InBounds(cell) ? softCost[Index(cell)] : 0f;

        public float Clearance(Vector2Int cell) => InBounds(cell) ? clearance[Index(cell)] : 0f;

        public float Clearance(Vector2 position) => Clearance(WorldToCell(position));

        public bool IsWalkable(Vector2Int cell, float agentRadius) => InBounds(cell) && !blocked[Index(cell)] && clearance[Index(cell)] >= agentRadius;

        public void SetBlocked(Vector2Int cell, bool isBlocked)
        {
            if (InBounds(cell))
                blocked[Index(cell)] = isBlocked;
        }

        public void AddSoftCost(Vector2Int cell, float cost)
        {
            if (InBounds(cell))
                softCost[Index(cell)] = Mathf.Max(softCost[Index(cell)], cost);
        }

        public void RebuildClearance() => ClearanceField.Compute(this, blocked, clearance);
    }
}
