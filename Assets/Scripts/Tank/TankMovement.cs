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

        private Rigidbody2D body;
        private TrackedDriveModel model;
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
            Vector2 forward = transform.up;

            // Read back what physics did last step, so collisions slow the hull down.
            model.SyncSpeed(Vector2.Dot(body.linearVelocity, forward));
            model.Step(throttle, steer, Time.fixedDeltaTime);

            body.linearVelocity = forward * model.Speed;
            body.angularVelocity = model.AngularSpeed;
        }
    }
}
