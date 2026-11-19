using PolishTanks.Navigation;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// <see cref="ISearchSpace"/> over the scene: walkability comes from the navigation grid at the unit's own size, and
    /// the view is cut by whatever blocks <see cref="LineOfSight"/>.
    /// </summary>
    public sealed class WorldSearchSpace : ISearchSpace
    {
        private readonly INavigation navigation;
        private readonly float agentRadius;
        private readonly Rigidbody2D viewer;

        public WorldSearchSpace(INavigation navigation, float agentRadius, Rigidbody2D viewer)
        {
            this.navigation = navigation;
            this.agentRadius = agentRadius;
            this.viewer = viewer;
        }

        public bool IsWalkable(Vector2 position) => navigation != null && navigation.IsWalkable(position, agentRadius);

        public float ClearDistance(Vector2 from, Vector2 direction, float maxDistance) =>
            LineOfSight.ClearDistance(from, direction, maxDistance, viewer);
    }
}
