using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// A pooled shell or bullet. It has no collider of its own: each frame it sweeps a circle
    /// along its path, so fast shots can't tunnel through thin objects.
    /// </summary>
    public class Projectile : PooledBehaviour
    {
        [Tooltip("Radius of the swept circle, world units.")]
        [SerializeField, Min(0.001f)] private float radius = 0.06f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private TrailRenderer trail;

        private static readonly List<RaycastHit2D> Hits = new List<RaycastHit2D>(16);

        private static readonly List<IProjectileHitHandler> Handlers = new List<IProjectileHitHandler>(4);

        private ProjectileShot shot;
        private Vector2 velocity;
        private float lifeLeft;

        public Vector2 Velocity => velocity;
        public Rigidbody2D Shooter => shot.Shooter;
        public float Radius => radius;

        public void Launch(in ProjectileShot projectileShot)
        {
            shot = projectileShot;
            velocity = shot.Velocity;
            lifeLeft = shot.Lifetime;

            if (velocity.sqrMagnitude > 0f)
                transform.rotation = Quaternion.LookRotation(Vector3.forward, velocity);

            if (trail != null)
            {
                trail.Clear();
                trail.emitting = true;
            }
        }

        protected internal override void OnReturned()
        {
            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }
            shot = default;
            velocity = Vector2.zero;
        }

        private void Update() => Step(Time.deltaTime);

        public void Step(float deltaTime)
        {
            if (!IsSpawned || deltaTime <= 0f)
                return;

            var speed = velocity.magnitude;
            if (speed <= 0f)
            {
                ReturnToPool();
                return;
            }

            Vector2 position = transform.position;
            var direction = velocity / speed;
            var distance = speed * deltaTime;

            if (TryFindHit(position, direction, distance, out var hit))
            {
                transform.position = hit.centroid;
                Impact(hit, direction);
                ReturnToPool();
                return;
            }

            transform.position = position + velocity * deltaTime;
            lifeLeft -= deltaTime;
            if (lifeLeft <= 0f)
                ReturnToPool();
        }

        private bool TryFindHit(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D nearest)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(hitMask);
            Physics2D.CircleCast(origin, radius, direction, filter, Hits, distance);

            nearest = default;
            var found = false;
            foreach (var hit in Hits)
            {
                if (shot.Shooter != null && hit.rigidbody == shot.Shooter)
                    continue;
                if (!found || hit.distance < nearest.distance)
                {
                    nearest = hit;
                    found = true;
                }
            }
            return found;
        }

        private void Impact(RaycastHit2D hit, Vector2 direction)
        {
            var body = hit.rigidbody;
            if (body != null && body.bodyType == RigidbodyType2D.Dynamic && shot.Impulse > 0f)
                body.AddForceAtPosition(direction * shot.Impulse, hit.point, ForceMode2D.Impulse);

            var info = new ProjectileHit(hit.point, hit.normal, direction, shot.Damage, shot.Impulse, shot.Shooter,
                shot.PenetrationMin, shot.PenetrationMax);
            var outcome = HitOutcome.Ignored;
            hit.collider.GetComponentsInParent(false, Handlers);
            foreach (var handler in Handlers)
            {
                var result = handler.OnProjectileHit(info);
                if (result > outcome)
                    outcome = result;
            }
            Handlers.Clear();

            var effects = outcome == HitOutcome.Blocked && shot.BlockedPool != null ? shot.BlockedPool : shot.ImpactPool;
            effects?.Get(hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
        }
    }
}
