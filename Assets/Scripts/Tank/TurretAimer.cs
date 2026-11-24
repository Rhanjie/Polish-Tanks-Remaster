using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Turns the turret it sits on toward an aim point with a delay. The turret's local up axis
    /// points down the barrel and its parent is the hull, so the angle is relative to the hull.
    /// <see cref="TraverseFactor"/> slows the traverse for a damaged ring; at zero the turret is locked to the hull.
    /// </summary>
    public class TurretAimer : MonoBehaviour
    {
        private TurretSettings settings = TurretSettings.Default;
        private float traverseFactor = 1f;

        private TurretTraverse traverse;
        private Vector2 aimPoint;
        private bool hasAimPoint;

        public TurretSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                ApplyTraverseSettings();
            }
        }

        public float TraverseFactor
        {
            get => traverseFactor;
            set
            {
                traverseFactor = Mathf.Clamp01(value);
                ApplyTraverseSettings();
            }
        }

        public bool IsLocked => traverseFactor <= 0f;

        public Vector2 BarrelDirection => transform.up;

        public float RemainingAngle => hasAimPoint ? Vector2.SignedAngle(BarrelDirection, aimPoint - (Vector2)transform.position) : 0f;

        public void SetAimPoint(Vector2 worldPoint)
        {
            aimPoint = worldPoint;
            hasAimPoint = true;
        }

        private void Awake()
        {
            traverse = new TurretTraverse(ScaledSettings(), transform.localEulerAngles.z);
        }

        private void Update()
        {
            if (!hasAimPoint || IsLocked)
                return;

            var toTarget = aimPoint - (Vector2)transform.position;
            if (toTarget.sqrMagnitude < 0.0001f)
                return;

            Vector2 hullForward = transform.parent != null ? transform.parent.up : Vector3.up;
            var targetAngle = Vector2.SignedAngle(hullForward, toTarget);
            traverse.Step(targetAngle, Time.deltaTime);
            transform.localRotation = Quaternion.Euler(0f, 0f, traverse.Angle);
        }

        private void ApplyTraverseSettings()
        {
            if (traverse == null)
                return;
            traverse.Settings = ScaledSettings();
            if (IsLocked)
                traverse.Stop();
        }

        private TurretSettings ScaledSettings()
        {
            var scaled = settings;
            scaled.maxTraverseSpeed *= traverseFactor;
            scaled.traverseAcceleration *= traverseFactor;
            return scaled;
        }
    }
}
