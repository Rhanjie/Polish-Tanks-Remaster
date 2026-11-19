using System.Collections.Generic;
using PolishTanks.Navigation;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// Walks a soldier. <see cref="MoveTo"/> asks the navigation grid for a route sized to a person and follows it
    /// point by point; <see cref="StepTo"/> goes straight to a nearby point without a route (jumping out of a tank's way).
    /// The body is dynamic and light, so tanks shove a soldier aside, but a soldier never shoves anything: the route
    /// keeps clear of crates, and just ahead of its feet it feels for crates, barrels, tanks and walls the grid doesn't
    /// know about (tanks move, crates get pushed around) and steps round them or slides along them instead of pushing. When it stops making progress it asks for a new
    /// route once and gives up after that. The soldier faces the point it was told to look at this frame, otherwise
    /// where it walks. A route that ends short of the destination (the destination can't be reached) counts as failed
    /// once the soldier stands at its end.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class InfantryMover : MonoBehaviour, ISelfMovingAgent
    {
        private const float WaypointReach = 0.45f;
        private const float StuckProgress = 0.3f;

        [SerializeField, Min(0.1f)] private float walkSpeed = 2.6f;
        [SerializeField, Min(0.1f)] private float runSpeed = 4.2f;
        [Tooltip("Degrees per second the soldier turns.")]
        [SerializeField, Min(10f)] private float turnSpeed = 540f;
        [Tooltip("Radius used for path finding, metres.")]
        [SerializeField, Min(0.1f)] private float agentRadius = 0.4f;
        [SerializeField, Min(0.1f)] private float arriveDistance = 0.6f;
        [Tooltip("Seconds without getting closer before the soldier replans, then gives up.")]
        [SerializeField, Min(0.1f)] private float stuckSeconds = 1.5f;

        [Inject] private INavigation navigation;

        private readonly List<Vector2> route = new List<Vector2>();
        private Rigidbody2D body;
        private int waypoint;
        private bool running;
        private bool replanned;
        private float bestDistance;
        private float progressClock;
        private const float FaceHoldSeconds = 0.1f;
        private const float MinProbe = 0.35f;
        private const float ProbeSeconds = 0.25f;
        private const float CrateAvoidance = 6f;

        private static readonly float[] DetourAngles = { 35f, 70f, 100f };
        private static readonly RaycastHit2D[] CastHits = new RaycastHit2D[8];

        private float detourSide = 1f;

        private Vector2? facePoint;
        private float faceUntil;

        public bool IsMoving { get; private set; }
        public bool HasArrived { get; private set; }
        public bool PathFailed { get; private set; }
        public Vector2 Destination { get; private set; }
        public float AgentRadius => agentRadius;
        public Vector2 Position => body != null ? body.position : (Vector2)transform.position;
        public Vector2 Forward => transform.up;
        public INavigation Navigation => navigation;

        private void Awake() => body = GetComponent<Rigidbody2D>();

        public bool MoveTo(Vector2 destination, bool run = false)
        {
            Begin(destination, run);
            return Plan();
        }

        public void StepTo(Vector2 point)
        {
            Begin(point, true);
            route.Add(point);
            IsMoving = true;
        }

        public void Stop()
        {
            IsMoving = false;
            route.Clear();
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }

        public void FaceThisFrame(Vector2 point)
        {
            facePoint = point;
            faceUntil = Time.time + FaceHoldSeconds;
        }

        private void Begin(Vector2 destination, bool run)
        {
            Destination = destination;
            running = run;
            HasArrived = false;
            PathFailed = false;
            replanned = false;
            route.Clear();
            waypoint = 0;
            bestDistance = Vector2.Distance(Position, destination);
            progressClock = Time.time;
        }

        private bool Plan()
        {
            route.Clear();
            waypoint = 0;
            if (navigation == null)
                return Fail();
            var result = navigation.FindPath(new PathRequest(Position, Destination, agentRadius, CrateAvoidance));
            if (!result.HasPath)
                return Fail();
            route.AddRange(result.Points);
            IsMoving = route.Count > 0;
            if (!IsMoving)
                HasArrived = true;
            return true;
        }

        private bool Fail()
        {
            IsMoving = false;
            PathFailed = true;
            return false;
        }

        private void FixedUpdate()
        {
            var position = body.position;
            var velocity = Vector2.zero;
            if (IsMoving)
                velocity = Walk(position);
            if (velocity.sqrMagnitude > 1e-4f)
                velocity = AvoidObstacles(velocity);
            body.linearVelocity = velocity;

            var look = facePoint.HasValue && Time.time <= faceUntil ? facePoint.Value - position : velocity;
            if (look.sqrMagnitude > 1e-4f)
            {
                var targetAngle = Mathf.Atan2(look.y, look.x) * Mathf.Rad2Deg - 90f;
                body.MoveRotation(Mathf.MoveTowardsAngle(body.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime));
            }
        }

        private Vector2 AvoidObstacles(Vector2 velocity)
        {
            var speed = velocity.magnitude;
            var direction = velocity / speed;
            var probe = Mathf.Max(MinProbe, speed * ProbeSeconds);
            if (!IsBlocked(direction, probe, out var normal))
                return velocity;

            foreach (var angle in DetourAngles)
            {
                var detour = (Vector2)(Quaternion.Euler(0f, 0f, angle * detourSide) * direction);
                if (!IsBlocked(detour, probe, out _))
                    return detour * speed;
                detour = Quaternion.Euler(0f, 0f, -angle * detourSide) * direction;
                if (!IsBlocked(detour, probe, out _))
                {
                    detourSide = -detourSide;
                    return detour * speed;
                }
            }

            var slide = velocity - normal * Vector2.Dot(velocity, normal);
            return Vector2.Dot(velocity, normal) < 0f ? slide : velocity;
        }

        private bool IsBlocked(Vector2 direction, float distance, out Vector2 normal)
        {
            normal = Vector2.zero;
            var filter = new ContactFilter2D { useTriggers = false };
            var count = body.Cast(direction, filter, CastHits, distance);
            var nearest = float.PositiveInfinity;
            for (var i = 0; i < count; i++)
            {
                var hit = CastHits[i];
                if (hit.rigidbody != null && hit.rigidbody.TryGetComponent<ISelfMovingAgent>(out _))
                    continue;
                if (hit.distance < nearest)
                {
                    nearest = hit.distance;
                    normal = hit.normal;
                }
            }
            return nearest < float.PositiveInfinity;
        }

        private Vector2 Walk(Vector2 position)
        {
            if (Vector2.Distance(position, Destination) <= arriveDistance)
            {
                IsMoving = false;
                HasArrived = true;
                route.Clear();
                return Vector2.zero;
            }

            while (waypoint < route.Count - 1 && Vector2.Distance(position, route[waypoint]) <= WaypointReach)
                waypoint++;
            if (waypoint >= route.Count)
            {
                IsMoving = false;
                HasArrived = true;
                return Vector2.zero;
            }
            if (waypoint == route.Count - 1 && Vector2.Distance(position, route[waypoint]) <= WaypointReach)
            {
                Fail();
                route.Clear();
                return Vector2.zero;
            }

            TrackProgress(position);
            if (!IsMoving)
                return Vector2.zero;
            var toPoint = route[waypoint] - position;
            return toPoint.normalized * (running ? runSpeed : walkSpeed);
        }

        private void TrackProgress(Vector2 position)
        {
            var distance = Vector2.Distance(position, Destination);
            if (distance < bestDistance - StuckProgress)
            {
                bestDistance = distance;
                progressClock = Time.time;
                return;
            }
            if (Time.time - progressClock < stuckSeconds)
                return;
            if (replanned)
            {
                Fail();
                route.Clear();
                return;
            }
            replanned = true;
            progressClock = Time.time;
            bestDistance = distance;
            Plan();
        }
    }
}
