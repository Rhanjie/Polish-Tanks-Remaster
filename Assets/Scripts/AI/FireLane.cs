using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// Whether an AI unit may fire at a point without hitting a friend: a strip as wide as the shot's spread is swept
    /// from the muzzle to the target, and any other AI unit in it closes the lane. Buildings and props don't count;
    /// the shot is still worth taking at a target behind a crate.
    /// </summary>
    public static class FireLane
    {
        private static readonly List<RaycastHit2D> Hits = new List<RaycastHit2D>(16);

        public static bool IsClear(Vector2 from, Vector2 to, Rigidbody2D shooter, float halfWidth)
        {
            var offset = to - from;
            var distance = offset.magnitude;
            if (distance <= Mathf.Epsilon)
                return true;
            var filter = new ContactFilter2D { useTriggers = false };
            Physics2D.CircleCast(from, halfWidth, offset / distance, filter, Hits, distance);
            foreach (var hit in Hits)
            {
                var body = hit.rigidbody;
                if (body == null || body == shooter)
                    continue;
                if (body.TryGetComponent<IAiAgent>(out var friend) && friend.IsActive)
                    return false;
            }
            return true;
        }
    }
}
