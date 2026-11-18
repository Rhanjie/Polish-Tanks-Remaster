using System.Collections.Generic;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// Every live enemy unit with a brain. Units add themselves while enabled. The player's gunfire makes every unit see
    /// further for a moment, and a player's shot that flies close past a unit tells it where the shooter is, however far
    /// away the player stands.
    /// </summary>
    public static class AiAgents
    {
        private static readonly List<IAiAgent> Agents = new List<IAiAgent>();

        public static IReadOnlyList<IAiAgent> All => Agents;

        public static void Register(IAiAgent agent)
        {
            if (!Agents.Contains(agent))
                Agents.Add(agent);
        }

        public static void Unregister(IAiAgent agent) => Agents.Remove(agent);

        public static void PlayerFired(GunMount mount, float time)
        {
            for (var i = Agents.Count - 1; i >= 0; i--)
                if (Agents[i].IsActive)
                    Agents[i].HearPlayerFire(mount, time);
        }

        public static void PlayerShotPassed(Vector2 from, Vector2 to, Vector2 shooterPosition, float time)
        {
            for (var i = Agents.Count - 1; i >= 0; i--)
            {
                var agent = Agents[i];
                if (agent.IsActive && SegmentMath.DistanceToSegment(agent.Position, from, to) <= agent.NearMissRadius)
                    agent.Alert(shooterPosition, AlertSource.NearMiss, time);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => Agents.Clear();
    }
}
