using System;
using System.Collections.Generic;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Drives a tank along a path to a destination: asks <see cref="INavigation"/> for a route sized to the hull
    /// (half its width as the hard limit, half its diagonal as the room it likes for turning on the spot), then feeds
    /// <see cref="TrackedSteering"/> into <see cref="TankMovement"/> waypoint by waypoint. It moves on to the next
    /// waypoint early only when the straight line to it is clear, so it doesn't cut inside corners. A route that
    /// can't get any closer than where the tank already stands counts as failed, not arrived. When the tank
    /// makes no headway (pinned on a crate, another tank in the way) it backs off for a moment and plans again; it gives
    /// up (<see cref="PathFailed"/>) after a few replans that bring it no closer to the destination.
    /// It owns the throttle while moving, so the player's <see cref="TankInput"/> must be off; AI and scripted
    /// scenes call <see cref="MoveTo"/> and <see cref="Stop"/>. A wreck stops on its own.
    /// </summary>
    [RequireComponent(typeof(TankMovement))]
    public class TankPathFollower : MonoBehaviour
    {
        [SerializeField] private SteeringSettings steering = SteeringSettings.Default;

        [Tooltip("Distance to the destination that counts as arrived.")]
        [SerializeField, Min(0.1f)] private float arriveDistance = 1.5f;

        [Tooltip("Distance at which the tank moves on to the next waypoint.")]
        [SerializeField, Min(0.1f)] private float waypointDistance = 2.5f;

        [Tooltip("Extra room kept between the hull and walls on top of half the hull's width.")]
        [SerializeField, Min(0f)] private float clearanceMargin = 0.3f;

        [Tooltip("How much the tank avoids crates and barrels. 0 drives straight through them.")]
        [SerializeField, Min(0f)] private float softObstacleAvoidance = 1f;

        [Header("Getting unstuck")]
        [SerializeField, Min(0.5f)] private float stuckTime = 2f;
        [Tooltip("Less movement than this (and almost no turning) within Stuck Time means stuck.")]
        [SerializeField, Min(0.05f)] private float stuckDistance = 0.6f;
        [SerializeField, Min(0f)] private float backOffTime = 1f;
        [SerializeField, Min(0)] private int maxReplans = 3;

        private const float ProgressForFreshReplans = 5f;

        [Inject] private INavigation injectedNavigation;

        private readonly List<Vector2> path = new List<Vector2>();
        private TankMovement movement;
        private TankController controller;
        private BoxCollider2D hull;
        private INavigation navigation;
        private int waypoint;
        private Vector2 checkpointPosition;
        private float checkpointRotation;
        private float checkpointTime;
        private float backOffUntil;
        private int replans;
        private float closestToDestination;

        public event Action<TankPathFollower> Arrived;
        public event Action<TankPathFollower> PathFailed;

        public INavigation Navigation
        {
            get => navigation ?? injectedNavigation;
            set => navigation = value;
        }

        public bool IsMoving { get; private set; }
        public bool HasArrived { get; private set; }
        public Vector2 Destination { get; private set; }
        public PathStatus LastStatus { get; private set; } = PathStatus.NoPath;
        public IReadOnlyList<Vector2> Path => path;

        public float AgentRadius => HullSize.x * 0.5f + clearanceMargin;

        public float PivotRadius => HullSize.magnitude * 0.5f + clearanceMargin;

        private Vector2 HullSize
        {
            get
            {
                if (hull == null)
                    return new Vector2(2.6f, 4.1f);
                var scale = hull.transform.lossyScale;
                return new Vector2(hull.size.x * Mathf.Abs(scale.x), hull.size.y * Mathf.Abs(scale.y));
            }
        }

        public bool MoveTo(Vector2 destination)
        {
            Destination = destination;
            HasArrived = false;
            replans = 0;
            closestToDestination = Vector2.Distance(transform.position, destination);
            return Plan();
        }

        public void Stop()
        {
            IsMoving = false;
            backOffUntil = 0f;
            path.Clear();
            if (movement != null)
                movement.SetInput(0f, 0f);
        }

        private void Awake()
        {
            movement = GetComponent<TankMovement>();
            controller = GetComponent<TankController>();
            hull = GetComponent<BoxCollider2D>();
        }

        private void OnDisable() => Stop();

        private bool Plan()
        {
            var nav = Navigation;
            path.Clear();
            if (nav == null || nav.Grid == null)
            {
                Debug.LogError($"{name}: no navigation in this scene, can't drive to {Destination}.", this);
                return Fail();
            }

            var position = (Vector2)transform.position;
            var result = nav.FindPath(new PathRequest(position, Destination, AgentRadius, softObstacleAvoidance, PivotRadius));
            LastStatus = result.Status;
            if (!result.HasPath)
                return Fail();
            if (result.Status == PathStatus.Partial && Vector2.Distance(position, result.Points[result.Points.Count - 1]) < arriveDistance)
                return Fail();

            path.AddRange(result.Points);
            waypoint = 0;
            IsMoving = true;
            ResetCheckpoint();
            return true;
        }

        private bool Fail()
        {
            Stop();
            PathFailed?.Invoke(this);
            return false;
        }

        private void Update()
        {
            if (!IsMoving)
                return;
            if (controller != null && controller.IsDestroyed)
            {
                Stop();
                return;
            }

            if (Time.time < backOffUntil)
                return;
            if (backOffUntil > 0f)
            {
                backOffUntil = 0f;
                Replan();
                return;
            }

            var position = (Vector2)transform.position;
            while (waypoint < path.Count - 1 && CanMoveOn(position))
                waypoint++;
            TrackProgress(position);

            var isFinal = waypoint == path.Count - 1;
            if (isFinal && Vector2.Distance(position, path[waypoint]) < arriveDistance)
            {
                Arrive();
                return;
            }

            if (IsStuck(position))
            {
                BackOff();
                return;
            }

            var drive = TrackedSteering.Toward(position, transform.up, path[waypoint], isFinal, steering);
            movement.SetInput(drive.Throttle, drive.Steer);
        }

        private void TrackProgress(Vector2 position)
        {
            var distance = Vector2.Distance(position, Destination);
            if (distance > closestToDestination - ProgressForFreshReplans)
                return;
            closestToDestination = distance;
            replans = 0;
        }

        private bool CanMoveOn(Vector2 position)
        {
            var distance = Vector2.Distance(position, path[waypoint]);
            if (distance < arriveDistance)
                return true;
            if (distance >= waypointDistance)
                return false;
            var grid = Navigation?.Grid;
            return grid == null || PathSmoother.IsClear(grid, grid.WorldToCell(position), grid.WorldToCell(path[waypoint + 1]), AgentRadius);
        }

        private void Arrive()
        {
            IsMoving = false;
            HasArrived = true;
            movement.SetInput(0f, 0f);
            Arrived?.Invoke(this);
        }

        private bool IsStuck(Vector2 position)
        {
            if (Time.time - checkpointTime < stuckTime)
                return false;
            var moved = Vector2.Distance(position, checkpointPosition);
            var turned = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, checkpointRotation));
            ResetCheckpoint();
            return moved < stuckDistance && turned < 15f;
        }

        private void BackOff()
        {
            var steer = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            movement.SetInput(-1f, steer);
            backOffUntil = Time.time + backOffTime;
        }

        private void Replan()
        {
            if (replans >= maxReplans)
            {
                Fail();
                return;
            }
            replans++;
            Plan();
        }

        private void ResetCheckpoint()
        {
            checkpointPosition = transform.position;
            checkpointRotation = transform.eulerAngles.z;
            checkpointTime = Time.time;
        }

        private void OnDrawGizmosSelected()
        {
            if (path.Count == 0)
                return;
            Gizmos.color = Color.cyan;
            var previous = (Vector2)transform.position;
            for (var i = waypoint; i < path.Count; i++)
            {
                Gizmos.DrawLine(previous, path[i]);
                Gizmos.DrawWireSphere(path[i], 0.3f);
                previous = path[i];
            }
        }
    }
}
