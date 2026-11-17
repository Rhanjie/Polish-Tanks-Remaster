using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// What a unit asks the pathfinder for. <see cref="AgentRadius"/> keeps the path that far from walls;
    /// <see cref="SoftCostWeight"/> scales how much the unit avoids crates and other soft obstacles (0 drives through).
    /// </summary>
    public readonly struct PathRequest
    {
        public PathRequest(Vector2 from, Vector2 to, float agentRadius, float softCostWeight = 1f)
        {
            From = from;
            To = to;
            AgentRadius = agentRadius;
            SoftCostWeight = softCostWeight;
        }

        public Vector2 From { get; }
        public Vector2 To { get; }
        public float AgentRadius { get; }
        public float SoftCostWeight { get; }
    }
}
