using System.Collections.Generic;
using PolishTanks.Ground;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// Whether a tank crew can see a point. Buildings, walls and trees block the view. Triggers, the two tanks
    /// themselves, low cover, impassable ground (water) and loose props (crates, barrels, other tanks and wrecks: anything
    /// on a dynamic body) don't, because a crew sits higher than they are. <see cref="ClearDistance"/> tells how far the
    /// view reaches in one direction.
    /// </summary>
    public static class LineOfSight
    {
        private static readonly List<RaycastHit2D> Hits = new List<RaycastHit2D>(16);

        public static bool IsClear(Vector2 from, Vector2 to, Rigidbody2D viewer, Rigidbody2D target)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            Physics2D.Linecast(from, to, filter, Hits);
            foreach (var hit in Hits)
            {
                if (!BlocksSight(hit, viewer, target))
                    continue;
                return false;
            }
            return true;
        }

        public static float ClearDistance(Vector2 from, Vector2 direction, float maxDistance, Rigidbody2D viewer)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            Physics2D.Raycast(from, direction.normalized, filter, Hits, maxDistance);
            var clear = maxDistance;
            foreach (var hit in Hits)
                if (hit.distance < clear && BlocksSight(hit, viewer, null))
                    clear = hit.distance;
            return clear;
        }

        private static bool BlocksSight(RaycastHit2D hit, Rigidbody2D viewer, Rigidbody2D target)
        {
            var body = hit.rigidbody;
            if (body != null && (body == viewer || body == target || body.bodyType == RigidbodyType2D.Dynamic))
                return false;
            return hit.collider.GetComponentInParent<LowCover>() == null && hit.collider.GetComponentInParent<GroundSurface>() == null;
        }
    }
}
