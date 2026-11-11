using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Drives the hull's Rigidbody2D from throttle and steer input.
    /// The hull's local up axis is its forward direction.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TankMovement : MonoBehaviour, IImpulseReceiver
    {
        private TankDriveSettings settings = TankDriveSettings.Default;
        private DriveCondition condition = DriveCondition.Healthy;

        private Rigidbody2D body;
        private TrackedDriveModel model;
        private readonly ExternalImpulse kick = new ExternalImpulse();
        private Vector2 requestedVelocity;
        private float throttle;
        private float steer;
        private float staggerLeft;

        public TankDriveSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                if (model != null)
                    model.Settings = value;
            }
        }

        public float CurrentSpeed => model?.Speed ?? 0f;

        public DriveCondition Condition
        {
            get => condition;
            set
            {
                condition = value;
                if (model != null)
                    model.Condition = value;
            }
        }

        public Vector2 ImpulseVelocity => kick.Velocity;

        public event System.Action<float> Staggered;

        public float StaggerRemaining => staggerLeft;
        public bool IsStaggered => staggerLeft > 0f;

        public void ReceiveHit(Vector2 impulse, Vector2 worldPoint)
        {
            AddImpulse(impulse, worldPoint);
            if (body == null)
                return;
            var seconds = HitStagger.Duration(impulse.magnitude, body.mass);
            if (seconds <= staggerLeft)
                return;
            staggerLeft = seconds;
            Staggered?.Invoke(seconds);
        }

        public void AddImpulse(Vector2 impulse, Vector2 worldPoint)
        {
            if (body == null)
                return;
            kick.Add(impulse, worldPoint - body.worldCenterOfMass, body.mass, body.inertia);
        }

        public void SetInput(float newThrottle, float newSteer)
        {
            throttle = Mathf.Clamp(newThrottle, -1f, 1f);
            steer = Mathf.Clamp(newSteer, -1f, 1f);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            model = new TrackedDriveModel(settings) { Condition = condition };
        }

        private void OnDisable()
        {
            throttle = 0f;
            steer = 0f;
            model?.Reset();
            kick.Reset();
            staggerLeft = 0f;
            requestedVelocity = Vector2.zero;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }

        private void FixedUpdate()
        {
            var forward = body.GetRelativeVector(Vector2.up);

            var dt = Time.fixedDeltaTime;
            kick.AbsorbLoss(requestedVelocity - body.linearVelocity);
            model.SyncSpeed(Vector2.Dot(body.linearVelocity - kick.Velocity, forward));
            var staggered = staggerLeft > 0f;
            staggerLeft = Mathf.Max(0f, staggerLeft - dt);
            model.Step(staggered ? 0f : throttle, staggered ? 0f : steer, dt);
            kick.Step(dt, settings.impulseLinearDecay, settings.impulseAngularDecay);

            requestedVelocity = forward * model.Speed + kick.Velocity;
            body.linearVelocity = requestedVelocity;
            body.angularVelocity = model.AngularSpeed + kick.AngularVelocity;
        }
    }
}
