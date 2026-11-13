using System.Collections.Generic;
using PolishTanks.Combat;
using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// A pooled shell or bullet. It has no collider of its own: each frame it sweeps a circle
    /// along its path, so fast shots can't tunnel through thin objects. It can glance off once, flying on
    /// with reduced power; after that any hit that fails to penetrate breaks it up. A target that lets it pass
    /// through is ignored from then on and the shot carries on with the penetration it has left.
    /// </summary>
    public class Projectile : PooledBehaviour
    {
        private const float EmbeddedImpulseFactor = 3f;
        private const int MaxHitsPerStep = 4;
        private const float DamagedTrailWidth = 0.5f;
        private const float DamagedTracerScale = 0.65f;

        [Tooltip("Radius of the swept circle, world units.")]
        [SerializeField, Min(0.001f)] private float radius = 0.06f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private TrailRenderer trail;

        private static readonly List<RaycastHit2D> Hits = new List<RaycastHit2D>(16);

        private static readonly List<IProjectileHitHandler> Handlers = new List<IProjectileHitHandler>(4);

        private ProjectileShot shot;
        private Vector2 velocity;
        private float lifeLeft;
        private readonly List<Collider2D> ignoredColliders = new List<Collider2D>(4);
        private readonly List<Rigidbody2D> ignoredBodies = new List<Rigidbody2D>(4);
        private Transform tracer;
        private Vector3 tracerScale;
        private float trailWidth = 1f;
        private bool visualsCached;

        public Vector2 Velocity => velocity;
        public Rigidbody2D Shooter => shot.Shooter;
        public bool HasRicocheted => shot.Ricocheted;
        public float Radius => radius;

        public void Launch(in ProjectileShot projectileShot)
        {
            shot = projectileShot;
            ShowDamage(false);
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

        private void ShowDamage(bool damaged)
        {
            if (!visualsCached)
            {
                visualsCached = true;
                var sprite = GetComponentInChildren<SpriteRenderer>(true);
                tracer = sprite != null ? sprite.transform : null;
                tracerScale = tracer != null ? tracer.localScale : Vector3.one;
                trailWidth = trail != null ? trail.widthMultiplier : 1f;
            }

            if (tracer != null)
                tracer.localScale = damaged ? tracerScale * DamagedTracerScale : tracerScale;
            if (trail != null)
                trail.widthMultiplier = damaged ? trailWidth * DamagedTrailWidth : trailWidth;
        }

        protected internal override void OnReturned()
        {
            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }
            shot = default;
            ignoredColliders.Clear();
            ignoredBodies.Clear();
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

            for (var i = 0; i < MaxHitsPerStep && TryFindHit(position, direction, distance, out var hit); i++)
            {
                transform.position = hit.centroid;
                var result = Impact(hit, direction);
                if (result == ImpactResult.Stopped)
                    ReturnToPool();
                if (result != ImpactResult.PassedThrough)
                    return;
                distance -= hit.distance;
                position = hit.centroid;
            }

            transform.position = position + direction * distance;
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
                if (ignoredColliders.Contains(hit.collider)
                    || (hit.rigidbody != null && ignoredBodies.Contains(hit.rigidbody)))
                    continue;
                if (!found || hit.distance < nearest.distance)
                {
                    nearest = hit;
                    found = true;
                }
            }
            return found;
        }

        private ImpactResult Impact(RaycastHit2D hit, Vector2 direction)
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
                response = new HitResponse(HitOutcome.Ricochet, hit.normal, SurfaceMaterial.Other);

            if (response.Outcome == HitOutcome.Ricochet && !canBounce)
                response = new HitResponse(HitOutcome.Blocked, response.Normal, response.Material);

            if (response.Outcome == HitOutcome.Penetrated && response.PassesThrough)
            {
                PassThrough(hit, response.ResidualPenetration);
                return ImpactResult.PassedThrough;
            }

            var glancing = response.Outcome == HitOutcome.Ricochet;
            var embedded = response.Outcome == HitOutcome.Blocked && response.Material == SurfaceMaterial.Metal;
            var impulse = glancing ? shot.Impulse * RicochetMath.RetainedPower
                : embedded ? shot.Impulse * EmbeddedImpulseFactor
                : shot.Impulse;
            Push(hit, direction, impulse);

            if (glancing)
            {
                Bounce(hit, direction, response.Normal, response.Material);
                return ImpactResult.Bounced;
            }

            var brokeUp = response.Outcome == HitOutcome.Blocked || (shot.Ricocheted && response.Outcome != HitOutcome.Penetrated);
            var onMetal = response.Material == SurfaceMaterial.Metal;
            var effects = brokeUp && onMetal && shot.BlockedPool != null ? shot.BlockedPool : shot.ImpactPool;
            effects?.Get(hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
            return ImpactResult.Stopped;
        }

        private void PassThrough(RaycastHit2D hit, float residualPenetration)
        {
            shot.PenetrationMin = residualPenetration;
            shot.PenetrationMax = residualPenetration;
            Ignore(hit);
            shot.ImpactPool?.Get(hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
        }

        private void Ignore(RaycastHit2D hit)
        {
            ignoredColliders.Add(hit.collider);
            if (hit.rigidbody != null)
                ignoredBodies.Add(hit.rigidbody);
        }

        private void Push(RaycastHit2D hit, Vector2 direction, float impulse)
        {
            var body = hit.rigidbody;
            if (body == null || body.bodyType != RigidbodyType2D.Dynamic || impulse <= 0f)
                return;
            if (body.TryGetComponent<IImpulseReceiver>(out var receiver))
                receiver.ReceiveHit(direction * impulse, hit.point);
            else
                body.AddForceAtPosition(direction * impulse, hit.point, ForceMode2D.Impulse);
        }

        private void Bounce(RaycastHit2D hit, Vector2 direction, Vector2 normal, SurfaceMaterial material)
        {
            var outgoing = RicochetMath.Deflect(direction, normal, Random.value);
            velocity = outgoing * (velocity.magnitude * RicochetMath.RetainedSpeed);

            shot.Ricocheted = true;
            shot.Damage *= RicochetMath.RetainedPower;
            shot.Impulse *= RicochetMath.RetainedPower;
            shot.PenetrationMin *= RicochetMath.RetainedPower;
            shot.PenetrationMax *= RicochetMath.RetainedPower;
            ignoredColliders.Add(hit.collider);

            ShowDamage(true);
            var rotation = Quaternion.LookRotation(Vector3.forward, outgoing);
            transform.SetPositionAndRotation(hit.centroid + normal.normalized * 0.02f, rotation);
            var effects = material == SurfaceMaterial.Metal || shot.OtherRicochetPool == null ? shot.RicochetPool : shot.OtherRicochetPool;
            effects?.Get(hit.point, rotation);
        }

        private enum ImpactResult
        {
            Stopped,
            Bounced,
            PassedThrough,
        }
    }
}
