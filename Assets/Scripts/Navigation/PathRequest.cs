using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// What a unit asks the pathfinder for. <see cref="AgentRadius"/> is the hard limit: the path never comes closer
    /// to a wall. <see cref="PreferredClearance"/> is the room the unit would like (a tank turning on the spot sweeps
    /// half its diagonal), so the path keeps that far from walls where the map allows and squeezes closer only
    /// through narrow spots. <see cref="SoftCostWeight"/> scales how much the unit avoids crates and other soft
    /// obstacles (0 drives through).
    /// </summary>
    public readonly struct PathRequest
    {
        public PathRequest(Vector2 from, Vector2 to, float agentRadius, float softCostWeight = 1f, float preferredClearance = 0f)
        {
            PreferredClearance = preferredClearance;
            From = from;
            To = to;
            AgentRadius = agentRadius;
            SoftCostWeight = softCostWeight;
        }

        public Vector2 From { get; }
        public Vector2 To { get; }
        public float AgentRadius { get; }
        public float SoftCostWeight { get; }
        public float PreferredClearance { get; }
    }
}
