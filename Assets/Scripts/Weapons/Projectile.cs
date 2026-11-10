using System.Collections.Generic;
using PolishTanks.Combat;
using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// A pooled shell or bullet. It has no collider of its own: each frame it sweeps a circle
    /// along its path, so fast shots can't tunnel through thin objects. It can glance off once, flying on
    /// with reduced power; after that any hit that fails to penetrate breaks it up.
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
        private Collider2D bouncedOff;

        public Vector2 Velocity => velocity;
        public Rigidbody2D Shooter => shot.Shooter;
        public bool HasRicocheted => shot.Ricocheted;
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
            bouncedOff = null;
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
                if (!Impact(hit, direction))
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
                if (!shot.Ricocheted && shot.Shooter != null && hit.rigidbody == shot.Shooter)
                    continue;
                if (bouncedOff != null && hit.collider == bouncedOff)
                    continue;
                if (!found || hit.distance < nearest.distance)
                {
                    nearest = hit;
                    found = true;
                }
            }
            return found;
        }

        private bool Impact(RaycastHit2D hit, Vector2 direction)
        {
            var canBounce = !shot.Ricocheted && hit.distance > 0f;
            var info = new ProjectileHit(hit.point, hit.normal, direction, shot.Damage, shot.Impulse, shot.Shooter,
                shot.PenetrationMin, shot.PenetrationMax, shot.Caliber, !canBounce);

            var response = HitResponse.Ignored;
            hit.collider.GetComponentsInParent(false, Handlers);
            foreach (var handler in Handlers)
            {
                var answer = handler.OnProjectileHit(info);
                if (answer.Outcome > response.Outcome)
                    response = answer;
            }
            Handlers.Clear();

            if (response.Outcome == HitOutcome.Ignored && canBounce
                && RicochetMath.Rolls(ArmorMath.ImpactAngle(hit.normal, direction), false, Random.value))
                response = new HitResponse(HitOutcome.Ricochet, hit.normal);

            if (response.Outcome == HitOutcome.Ricochet && !canBounce)
                response = new HitResponse(HitOutcome.Blocked, response.Normal);

            var glancing = response.Outcome == HitOutcome.Ricochet;
            Push(hit, direction, glancing ? shot.Impulse * RicochetMath.RetainedPower : shot.Impulse);

            if (glancing)
            {
                Bounce(hit, direction, response.Normal);
                return true;
            }

            var brokeUp = response.Outcome == HitOutcome.Blocked || (shot.Ricocheted && response.Outcome != HitOutcome.Penetrated);
            var effects = brokeUp && shot.BlockedPool != null ? shot.BlockedPool : shot.ImpactPool;
            effects?.Get(hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
            return false;
        }

        private void Push(RaycastHit2D hit, Vector2 direction, float impulse)
        {
            var body = hit.rigidbody;
            if (body != null && body.bodyType == RigidbodyType2D.Dynamic && impulse > 0f)
                body.AddForceAtPosition(direction * impulse, hit.point, ForceMode2D.Impulse);
        }

        private void Bounce(RaycastHit2D hit, Vector2 direction, Vector2 normal)
        {
            var outgoing = RicochetMath.Deflect(direction, normal, Random.value);
            velocity = outgoing * (velocity.magnitude * RicochetMath.RetainedSpeed);

            shot.Ricocheted = true;
            shot.Damage *= RicochetMath.RetainedPower;
            shot.Impulse *= RicochetMath.RetainedPower;
            shot.PenetrationMin *= RicochetMath.RetainedPower;
            shot.PenetrationMax *= RicochetMath.RetainedPower;
            bouncedOff = hit.collider;

            var rotation = Quaternion.LookRotation(Vector3.forward, outgoing);
            transform.SetPositionAndRotation(hit.centroid + normal.normalized * 0.02f, rotation);
            shot.RicochetPool?.Get(hit.point, rotation);
        }
    }
}
