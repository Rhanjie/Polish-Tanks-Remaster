using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// How an AI tank helps its own soldiers out of the way: with a friendly soldier in the lane ahead it eases off the
    /// throttle (more the closer the soldier is) and leans its steering away from it. It never stops dead for them, so a
    /// soldier that doesn't move can still end up under the tracks. Steer is positive to the right, as in
    /// <see cref="Navigation.TrackedSteering"/>.
    /// </summary>
    public static class InfantryCaution
    {
        public const float LookAhead = 9f;
        public const float LaneHalfWidth = 2.8f;
        public const float SlowDistance = 3f;
        public const float SlowestFactor = 0.3f;
        public const float SteerAway = 0.35f;

        public static void Yield(Vector2 position, Vector2 forward, IReadOnlyList<IAiAgent> agents, out float speedLimit, out float steerBias)
        {
            speedLimit = 1f;
            steerBias = 0f;
            var nearest = float.PositiveInfinity;
            foreach (var agent in agents)
            {
                if (!agent.IsOnFoot || !agent.IsActive)
                    continue;
                var offset = agent.Position - position;
                var ahead = Vector2.Dot(offset, forward);
                if (ahead < 0f || ahead > LookAhead)
                    continue;
                var lateral = forward.x * offset.y - forward.y * offset.x;
                if (Mathf.Abs(lateral) > LaneHalfWidth)
                    continue;

                var limit = Mathf.Lerp(SlowestFactor, 1f, Mathf.InverseLerp(SlowDistance, LookAhead, ahead));
                speedLimit = Mathf.Min(speedLimit, limit);
                if (ahead < nearest)
                {
                    nearest = ahead;
                    steerBias = lateral >= 0f ? SteerAway : -SteerAway;
                }
            }
        }
    }
}
