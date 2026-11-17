using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// <see cref="INavigation"/> over a prebuilt <see cref="NavGrid"/> with a single A* instance. <see cref="Use"/>
    /// swaps in a rebuilt grid, so everyone holding this object sees the new map.
    /// </summary>
    public sealed class GridNavigation : INavigation
    {
        private AStarPathfinder pathfinder;

        public GridNavigation(NavGrid grid)
        {
            Use(grid);
        }

        public NavGrid Grid { get; private set; }

        public void Use(NavGrid grid)
        {
            Grid = grid;
            pathfinder = new AStarPathfinder(grid);
        }

        public PathResult FindPath(PathRequest request) => pathfinder.FindPath(request);

        public bool IsWalkable(Vector2 position, float agentRadius) => Grid.IsWalkable(Grid.WorldToCell(position), agentRadius);

        public float Clearance(Vector2 position) => Grid.Clearance(position);
    }
}
