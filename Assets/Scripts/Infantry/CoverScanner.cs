using System.Collections.Generic;
using PolishTanks.AI;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// Finds cover around a soldier: every piece of low cover (crates, barrels, low walls) and every working tank of its
    /// own side. The player's tank is never cover.
    /// </summary>
    public static class CoverScanner
    {
        public const float TankCoverRadius = 2.2f;

        private static readonly List<Collider2D> Colliders = new List<Collider2D>(48);
        private static readonly HashSet<Object> Seen = new HashSet<Object>();

        public static void Scan(Vector2 position, float radius, List<CoverObject> results)
        {
            results.Clear();
            Seen.Clear();
            var filter = new ContactFilter2D { useTriggers = false };
            Physics2D.OverlapCircle(position, radius, filter, Colliders);
            foreach (var collider in Colliders)
            {
                var body = collider.attachedRigidbody;
                if (body != null && body.TryGetComponent<TankController>(out var tank))
                {
                    if (Seen.Add(body) && !tank.IsDestroyed && body.TryGetComponent<IAiAgent>(out var crew) && crew.IsActive)
                        results.Add(new CoverObject(body.position, TankCoverRadius));
                    continue;
                }

                var cover = collider.GetComponentInParent<LowCover>();
                if (cover == null || !Seen.Add(collider))
                    continue;
                var bounds = collider.bounds;
                results.Add(new CoverObject(bounds.center, Mathf.Max(bounds.extents.x, bounds.extents.y)));
            }
        }
    }
}
