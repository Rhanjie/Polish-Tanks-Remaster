using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Stand-in bound when a scene has no <see cref="NavigationGridSource"/>: every search fails and nothing is
    /// walkable. It keeps injection working in such scenes, so a unit that only needs a path when it drives does not
    /// break the whole scene.
    /// </summary>
    public sealed class NoNavigation : INavigation
    {
        public NavGrid Grid => null;

        public PathResult FindPath(PathRequest request) => PathResult.None;

        public bool IsWalkable(Vector2 position, float agentRadius) => false;

        public float Clearance(Vector2 position) => 0f;
    }
}
