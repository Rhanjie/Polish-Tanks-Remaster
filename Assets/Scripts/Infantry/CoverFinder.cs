using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// Picks where a soldier hides from a threat: the spot right behind a cover object, on the far side from the
    /// threat. The nearest such spot wins, as long as the soldier can reach it quickly, it is not too close to the threat,
    /// a squad mate has not taken it already and the ground there is walkable. A spot stops being cover once the threat
    /// has moved round so that the object no longer stands between them.
    /// </summary>
    public static class CoverFinder
    {
        public const float StandOff = 0.55f;
        public const float TakenRadius = 1.1f;
        public const float MinThreatDistance = 4f;
        public const float ShieldAngle = 40f;

        public static Vector2 SpotBehind(in CoverObject cover, Vector2 threat)
        {
            var away = cover.Centre - threat;
            away = away.sqrMagnitude > 1e-6f ? away.normalized : Vector2.up;
            return cover.Centre + away * (cover.Radius + StandOff);
        }

        public static bool Shields(in CoverObject cover, Vector2 spot, Vector2 threat)
        {
            var fromCover = spot - cover.Centre;
            var awayFromThreat = cover.Centre - threat;
            return fromCover.sqrMagnitude > 1e-6f && awayFromThreat.sqrMagnitude > 1e-6f
                && Vector2.Angle(fromCover, awayFromThreat) <= ShieldAngle;
        }

        public static bool TryFind(Vector2 soldier, Vector2 threat, IReadOnlyList<CoverObject> objects, IReadOnlyList<Vector2> taken,
            float maxDistance, Func<Vector2, bool> isWalkable, out Vector2 spot, out CoverObject chosen)
        {
            spot = soldier;
            chosen = default;
            var best = float.PositiveInfinity;
            foreach (var cover in objects)
            {
                var candidate = SpotBehind(cover, threat);
                var distance = Vector2.Distance(soldier, candidate);
                if (distance > maxDistance || distance >= best)
                    continue;
                if (Vector2.Distance(candidate, threat) < MinThreatDistance + cover.Radius)
                    continue;
                if (IsTaken(candidate, taken) || (isWalkable != null && !isWalkable(candidate)))
                    continue;
                best = distance;
                spot = candidate;
                chosen = cover;
            }
            return best < float.PositiveInfinity;
        }

        public static bool IsTaken(Vector2 spot, IReadOnlyList<Vector2> taken)
        {
            foreach (var other in taken)
                if (Vector2.Distance(spot, other) < TakenRadius)
                    return true;
            return false;
        }
    }
}
