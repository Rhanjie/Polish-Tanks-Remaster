using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>Path finding for the current scene, bound by the scene installer.</summary>
    public interface INavigation
    {
        NavGrid Grid { get; }

        PathResult FindPath(PathRequest request);

        bool IsWalkable(Vector2 position, float agentRadius);

        float Clearance(Vector2 position);
    }
}
