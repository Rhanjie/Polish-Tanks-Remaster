using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// An explosion on the ground. Soft targets (<see cref="IBlastReceiver"/>) are hurt by distance. Everything with
    /// armor is hit once, on the point of its outline closest to the blast, as if by a shell coming straight at that
    /// plate: the usual armor rules pick the plate (so a blast behind a tank tests the rear armor) and the penetration
    /// falls off with distance. A blast never glances off. Loose props get pushed away.
    /// </summary>
    public static class Blast
    {
        public const float EdgePower = 0.5f;

        private static readonly List<Collider2D> Colliders = new List<Collider2D>(32);
        private static readonly List<IProjectileHitHandler> Handlers = new List<IProjectileHitHandler>(4);
        private static readonly HashSet<Object> Struck = new HashSet<Object>();

        public static float Power(float distance, float radius) =>
            radius <= 0f || distance > radius ? 0f : Mathf.Lerp(1f, EdgePower, distance / radius);

        public static int Detonate(Vector2 centre, in BlastSettings blast, Rigidbody2D source)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            Physics2D.OverlapCircle(centre, blast.radius, filter, Colliders);
            Struck.Clear();
            foreach (var collider in Colliders)
            {
                var receiver = collider.GetComponentInParent<IBlastReceiver>();
                Object owner = receiver as Component;
                if (owner == null)
                    owner = collider.attachedRigidbody != null ? collider.attachedRigidbody : collider;
                if (!Struck.Add(owner))
                    continue;

                var point = collider.ClosestPoint(centre);
                var distance = Vector2.Distance(centre, point);
                if (distance > blast.radius)
                    continue;
                if (receiver != null)
                    receiver.OnBlast(centre, distance, blast, source);
                else
                    Strike(collider, centre, point, distance, blast, source);
            }
            return Struck.Count;
        }

        private static void Strike(Collider2D collider, Vector2 centre, Vector2 point, float distance, in BlastSettings blast, Rigidbody2D source)
        {
            var outward = centre - point;
            if (outward.sqrMagnitude < 1e-6f)
                outward = centre - (Vector2)collider.bounds.center;
            if (outward.sqrMagnitude < 1e-6f)
                outward = Vector2.up;
            outward.Normalize();

            var power = Power(distance, blast.radius);
            var hit = new ProjectileHit(point, outward, -outward, blast.damage, 0f, source,
                blast.penetrationMin * power, blast.penetrationMax * power, blast.caliber, true);
            collider.GetComponentsInParent(false, Handlers);
            foreach (var handler in Handlers)
                handler.OnProjectileHit(hit);
            Handlers.Clear();

            var body = collider.attachedRigidbody;
            if (body != null && body.bodyType == RigidbodyType2D.Dynamic && body.GetComponent<IImpulseReceiver>() == null)
                body.AddForceAtPosition(-outward * blast.impulse * power, point, ForceMode2D.Impulse);
        }
    }
}
