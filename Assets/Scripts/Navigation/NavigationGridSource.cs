using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// The scene's navigation grid: covers <see cref="Area"/> and is built from the scene colliders on Start, once
    /// every object has enabled and made its colliders (the ground makes water and cliffs in OnEnable), or earlier if
    /// something asks for a path first. Nothing needs baking. <see cref="Rebuild"/> refreshes it after the
    /// map changes. Selected in the editor it draws the area, and with Draw Cells on also the blocked (red) and soft
    /// (yellow) cells; "Preview Grid" in the component menu builds the grid outside Play Mode.
    /// </summary>
    public class NavigationGridSource : MonoBehaviour
    {
        [SerializeField] private Rect area = new Rect(-150f, -150f, 300f, 300f);
        [SerializeField, Min(0.25f)] private float cellSize = 1f;
        [Tooltip("Extra cost of driving through a cell with a crate or barrel in it, relative to an empty cell.")]
        [SerializeField, Min(0f)] private float softObstacleCost = 4f;
        [SerializeField] private bool drawCells;

        private GridNavigation navigation;

        public Rect Area => area;

        public INavigation Navigation => navigation ??= new GridNavigation(BuildGrid);

        public void Rebuild()
        {
            if (navigation == null)
                navigation = new GridNavigation(BuildGrid());
            else
                navigation.Use(BuildGrid());
        }

        private NavGrid BuildGrid()
        {
            var timer = Stopwatch.StartNew();
            var grid = NavGridBuilder.Build(area, cellSize, new Physics2DObstacleSource(softObstacleCost));
            Debug.Log($"[Navigation] {name}: {grid.Width}x{grid.Height} grid built in {timer.ElapsedMilliseconds} ms.", this);
            return grid;
        }

        private void Start()
        {
            if (!((GridNavigation)Navigation).IsBuilt)
                Rebuild();
        }

        [ContextMenu("Preview Grid")]
        private void PreviewGrid()
        {
            Rebuild();
            drawCells = true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireCube(area.center, area.size);
            if (!drawCells || navigation == null || !navigation.IsBuilt)
                return;

            var grid = navigation.Grid;
            var size = Vector3.one * grid.CellSize * 0.9f;
            for (var index = 0; index < grid.CellCount; index++)
            {
                var cell = grid.CellAt(index);
                if (grid.IsBlocked(cell))
                    Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
                else if (grid.SoftCost(cell) > 0f)
                    Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.45f);
                else
                    continue;
                Gizmos.DrawCube(grid.CellToWorld(cell), size);
            }
        }
    }
}
