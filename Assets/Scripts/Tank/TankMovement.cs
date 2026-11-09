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

        /// <summary>Signed speed along the hull's forward axis, units per second.</summary>
        public float CurrentSpeed => model?.Speed ?? 0f;

        /// <summary>Velocity currently added on top of the drive by recoil and hits.</summary>
        public Vector2 ImpulseVelocity => kick.Velocity;

        /// <summary>
        /// Shoves the hull (gun recoil, a hit). Setting the body's velocity directly would be
        /// overwritten by the drive next step, so the shove is layered on and fades out.
        /// </summary>
        /// <param name="impulse">World impulse (mass × velocity).</param>
        /// <param name="worldPoint">Where it acts; off-center points also turn the hull.</param>
        public void AddImpulse(Vector2 impulse, Vector2 worldPoint)
        {
            if (body == null)
                return;
            kick.Add(impulse, worldPoint - body.worldCenterOfMass, body.mass, body.inertia);
        }

        /// <param name="newThrottle">-1 (full reverse) to 1 (full forward).</param>
        /// <param name="newSteer">-1 (left) to 1 (right).</param>
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
            // Read the pose from the body: with interpolation on, the transform lags one physics step.
            var forward = body.GetRelativeVector(Vector2.up);

            // Read back what physics did last step, so collisions slow the hull down.
            // The recoil part was added by us, so it doesn't count as track speed.
            var dt = Time.fixedDeltaTime;
            // What a collision took away is charged to the recoil first, so a blocked shove
            // doesn't later read as forward track speed.
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
