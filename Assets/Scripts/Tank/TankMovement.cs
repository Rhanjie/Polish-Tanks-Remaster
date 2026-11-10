using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Drives the hull's Rigidbody2D from throttle and steer input.
    /// The hull's local up axis is its forward direction.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TankMovement : MonoBehaviour
    {
        [SerializeField] private TankDriveSettings settings = TankDriveSettings.Default;

        [Header("Recoil and hits")]
        [Tooltip("How fast a recoil shove fades, per second. Higher = tracks bite sooner.")]
        [SerializeField, Min(0f)] private float impulseLinearDecay = 4f;
        [SerializeField, Min(0f)] private float impulseAngularDecay = 6f;

        private Rigidbody2D body;
        private TrackedDriveModel model;
        private readonly ExternalImpulse kick = new ExternalImpulse();
        private Vector2 requestedVelocity;
        private float throttle;
        private float steer;

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

        public Vector2 ImpulseVelocity => kick.Velocity;

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
            model = new TrackedDriveModel(settings);
        }

        private void OnDisable()
        {
            throttle = 0f;
            steer = 0f;
            model?.Reset();
            kick.Reset();
            requestedVelocity = Vector2.zero;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }

        private void OnValidate()
        {
            if (model != null)
                model.Settings = settings;
        }

        private void FixedUpdate()
        {
            var forward = body.GetRelativeVector(Vector2.up);

            var dt = Time.fixedDeltaTime;
            kick.AbsorbLoss(requestedVelocity - body.linearVelocity);
            model.SyncSpeed(Vector2.Dot(body.linearVelocity - kick.Velocity, forward));
            model.Step(throttle, steer, dt);
            kick.Step(dt, impulseLinearDecay, impulseAngularDecay);

            requestedVelocity = forward * model.Speed + kick.Velocity;
            body.linearVelocity = requestedVelocity;
            body.angularVelocity = model.AngularSpeed + kick.AngularVelocity;
        }
    }
}
