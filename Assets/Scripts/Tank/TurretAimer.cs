using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Turns the turret toward an aim point with a delay. Sits on the hull; the turret
    /// is a child whose local up axis points down the barrel.
    /// </summary>
    public class TurretAimer : MonoBehaviour
    {
        [SerializeField] private Transform turret;
        [SerializeField] private TurretSettings settings = TurretSettings.Default;

        private TurretTraverse traverse;
        private Vector2 aimPoint;
        private bool hasAimPoint;

        public Transform Turret => turret;

        public TurretSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                if (traverse != null)
                    traverse.Settings = value;
            }
        }

        /// <summary>World-space direction the barrel points in.</summary>
        public Vector2 BarrelDirection => turret != null ? (Vector2)turret.up : (Vector2)transform.up;

        /// <summary>Degrees the barrel still has to turn to face the aim point.</summary>
        public float RemainingAngle => hasAimPoint ? Vector2.SignedAngle(BarrelDirection, aimPoint - (Vector2)turret.position) : 0f;

        public void SetAimPoint(Vector2 worldPoint)
        {
            aimPoint = worldPoint;
            hasAimPoint = true;
        }

        private void Awake()
        {
            if (turret == null)
            {
                Debug.LogError($"{nameof(TurretAimer)} on {name} has no turret assigned.", this);
                enabled = false;
                return;
            }

            traverse = new TurretTraverse(settings, turret.localEulerAngles.z);
        }

        private void OnValidate()
        {
            if (traverse != null)
                traverse.Settings = settings;
        }

        private void Update()
        {
            if (!hasAimPoint)
                return;

            var toTarget = aimPoint - (Vector2)turret.position;
            if (toTarget.sqrMagnitude < 0.0001f)
                return;

            // Angle is relative to the hull, so turning the hull drags the turret along
            // and the traverse then has to catch up with the cursor.
            var targetAngle = Vector2.SignedAngle(transform.up, toTarget);
            traverse.Step(targetAngle, Time.deltaTime);
            turret.localRotation = Quaternion.Euler(0f, 0f, traverse.Angle);
        }
    }
}
