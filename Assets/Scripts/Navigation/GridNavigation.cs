using System;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// <see cref="INavigation"/> over a <see cref="NavGrid"/> with a single A* instance. Created with a build
    /// function, it builds the grid on first use, so it can be bound in the scene installer before every collider
    /// exists (the ground's water and cliffs appear in OnEnable). <see cref="Use"/> swaps in a rebuilt grid, so
    /// everyone holding this object sees the new map.
    /// </summary>
    public sealed class GridNavigation : INavigation
    {
        private Func<NavGrid> pendingBuild;
        private NavGrid grid;
        private AStarPathfinder pathfinder;
        private bool recordSearches;

        public GridNavigation(NavGrid grid)
        {
            Use(grid);
        }

        public GridNavigation(Func<NavGrid> buildGrid)
        {
            pendingBuild = buildGrid ?? throw new ArgumentNullException(nameof(buildGrid));
        }

        public bool IsBuilt => grid != null;

        public bool RecordSearches
        {
            get => recordSearches;
            set
            {
                recordSearches = value;
                if (pathfinder != null)
                    pathfinder.RecordSearches = value;
            }
        }

        public SearchTrace LastSearch => pathfinder?.LastSearch;

        public NavGrid Grid
        {
            get
            {
                if (grid == null && pendingBuild != null)
                    Use(pendingBuild());
                return grid;
            }
        }

        public void Use(NavGrid newGrid)
        {
            pendingBuild = null;
            grid = newGrid;
            pathfinder = new AStarPathfinder(newGrid) { RecordSearches = recordSearches };
        }

        public PathResult FindPath(PathRequest request) => Grid != null ? pathfinder.FindPath(request) : PathResult.None;

        public bool IsWalkable(Vector2 position, float agentRadius) => Grid != null && grid.IsWalkable(grid.WorldToCell(position), agentRadius);

        public float Clearance(Vector2 position) => Grid != null ? grid.Clearance(position) : 0f;
    }
}
