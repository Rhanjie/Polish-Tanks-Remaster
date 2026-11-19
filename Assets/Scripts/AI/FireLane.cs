using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// Whether an AI unit may fire at a point without hitting a friend. The lane is a cone from the muzzle: as wide as
    /// the shot at the start and widening with the weapon's spread, running a little past the target because a miss flies
    /// on. Any other live AI unit inside it closes the lane. Buildings and props don't count; a shot is still worth
    /// taking at a target behind a crate.
    /// </summary>
    public static class FireLane
    {
        public const float Overshoot = 6f;
        public const float FriendRadius = 0.6f;

        public static bool IsClear(Vector2 from, Vector2 to, IAiAgent shooter, float halfWidth, float spreadDegrees,
            IReadOnlyList<IAiAgent> friends)
        {
            var offset = to - from;
            var distance = offset.magnitude;
            if (distance <= Mathf.Epsilon)
                return true;
            var direction = offset / distance;
            var widening = Mathf.Tan(Mathf.Clamp(spreadDegrees, 0f, 45f) * Mathf.Deg2Rad);
            foreach (var friend in friends)
            {
                if (friend == shooter || !friend.IsActive)
                    continue;
                var toFriend = friend.Position - from;
                var ahead = Vector2.Dot(toFriend, direction);
                if (ahead <= 0f || ahead > distance + Overshoot)
                    continue;
                var lateral = Mathf.Abs(direction.x * toFriend.y - direction.y * toFriend.x);
                if (lateral <= halfWidth + ahead * widening + FriendRadius)
                    return false;
            }
            return true;
        }
    }
}
