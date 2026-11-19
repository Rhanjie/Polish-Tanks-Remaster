using System.Collections.Generic;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Reads obstacles from the scene's 2D colliders without needing physics layers. Triggers, tanks and soldiers are skipped
    /// (they move, so they are avoided while moving, not baked). Colliders on dynamic bodies, the crates and barrels a
    /// tank can push or shoot apart, become soft cost. Everything else (buildings, walls, trees, wrecks, map edges)
    /// is solid.
    /// </summary>
    public sealed class Physics2DObstacleSource : IObstacleSource
    {
        private readonly float softCost;

        public Physics2DObstacleSource(float softCost)
        {
            this.softCost = softCost;
        }

        public IEnumerable<Obstacle> Collect(Rect area)
        {
            Physics2D.SyncTransforms();
            var filter = new ContactFilter2D { useTriggers = false };
            var colliders = new List<Collider2D>();
            Physics2D.OverlapArea(area.min, area.max, filter, colliders);

            foreach (var collider in colliders)
            {
                if (!TryClassify(collider, out var kind))
                    continue;
                var bounds = collider.bounds;
                var shape = collider;
                yield return new Obstacle(new Rect(bounds.min, bounds.size), kind, kind == ObstacleKind.Soft ? softCost : 0f,
                    point => Vector2.Distance(point, shape.ClosestPoint(point)));
            }
        }

        public static bool TryClassify(Collider2D collider, out ObstacleKind kind)
        {
            kind = ObstacleKind.Solid;
            if (collider == null || collider.isTrigger || !collider.enabled)
                return false;
            if (collider.GetComponentInParent<TankController>() != null || collider.GetComponentInParent<ISelfMovingAgent>() != null)
                return false;
            var body = collider.attachedRigidbody;
            if (body != null && body.bodyType == RigidbodyType2D.Dynamic)
                kind = ObstacleKind.Soft;
            return true;
        }
    }
}
