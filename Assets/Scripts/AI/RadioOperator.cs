using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// A unit's radio. While it has the player spotted it calls the player's position to every live friend within its
    /// own sight range, at most once per <see cref="Cooldown"/>, so the whole group joins the fight.
    /// </summary>
    public sealed class RadioOperator
    {
        public const float Cooldown = 3f;

        private float nextCall = float.NegativeInfinity;

        public int TryBroadcast(IAiAgent sender, Vector2 targetPosition, float range, float time, IReadOnlyList<IAiAgent> listeners)
        {
            if (time < nextCall)
                return 0;
            nextCall = time + Cooldown;

            var reached = 0;
            for (var i = listeners.Count - 1; i >= 0; i--)
            {
                var listener = listeners[i];
                if (listener == sender || !listener.IsActive || Vector2.Distance(sender.Position, listener.Position) > range)
                    continue;
                listener.Alert(targetPosition, AlertSource.Radio, time);
                reached++;
            }
            return reached;
        }

        public void Reset() => nextCall = float.NegativeInfinity;
    }
}
