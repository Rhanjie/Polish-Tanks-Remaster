using System.Collections.Generic;
using PolishTanks.Navigation;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// Walks a soldier. <see cref="MoveTo"/> asks the navigation grid for a route sized to a person and follows it
    /// point by point; <see cref="StepTo"/> goes straight to a nearby point without a route (jumping out of a tank's way).
    /// The body is dynamic and light, so tanks shove a soldier aside. When it stops making progress it asks for a new
    /// route once and gives up after that. The soldier faces the point it was told to look at this frame, otherwise
    /// where it walks.
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
        private Vector2? facePoint;

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

        public void FaceThisFrame(Vector2 point) => facePoint = point;

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
            var result = navigation.FindPath(new PathRequest(Position, Destination, agentRadius, 0f));
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
            body.linearVelocity = velocity;

            var look = facePoint.HasValue ? facePoint.Value - position : velocity;
            facePoint = null;
            if (look.sqrMagnitude > 1e-4f)
            {
                var targetAngle = Mathf.Atan2(look.y, look.x) * Mathf.Rad2Deg - 90f;
                body.MoveRotation(Mathf.MoveTowardsAngle(body.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime));
            }
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
