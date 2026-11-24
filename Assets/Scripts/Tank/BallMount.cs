using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// The ball mount of a hull machine gun. Sits on the hull's front plate with its up axis along the hull's nose and
    /// swings the gun toward an aim point, but only within <see cref="Arc"/> degrees to each side.
    /// <see cref="CanReach"/> tells whether the aim point lies inside that arc, so the gun only fires at what it covers.
    /// </summary>
    public class BallMount : MonoBehaviour
    {
        private const float OnTargetAngle = 2f;

        [Tooltip("How far the mount swings to each side of the hull's nose, degrees.")]
        [SerializeField, Range(0f, 45f)] private float arc = 15f;

        [Tooltip("How fast the mount swings, degrees per second.")]
        [SerializeField, Min(1f)] private float traverseSpeed = 90f;

        private Vector2 aimPoint;
        private bool hasAimPoint;
        private float angle;

        public float Arc
        {
            get => arc;
            set => arc = Mathf.Clamp(value, 0f, 45f);
        }

        public float TraverseSpeed
        {
            get => traverseSpeed;
            set => traverseSpeed = Mathf.Max(1f, value);
        }

        public float Angle => angle;

        public bool CanReach => hasAimPoint && Mathf.Abs(TargetAngle()) <= arc + OnTargetAngle;

        public bool IsOnTarget => CanReach && Mathf.Abs(Mathf.DeltaAngle(angle, Mathf.Clamp(TargetAngle(), -arc, arc))) <= OnTargetAngle;

        public void SetAimPoint(Vector2 worldPoint)
        {
            aimPoint = worldPoint;
            hasAimPoint = true;
        }

        public static float Swing(float current, float target, float arc, float maxStep) =>
            Mathf.MoveTowards(current, Mathf.Clamp(target, -arc, arc), maxStep);

        private void Update()
        {
            var target = hasAimPoint && CanReach ? TargetAngle() : 0f;
            angle = Swing(angle, target, arc, traverseSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private float TargetAngle()
        {
            var toTarget = aimPoint - (Vector2)transform.position;
            if (toTarget.sqrMagnitude < 0.0001f)
                return 0f;
            Vector2 nose = transform.parent != null ? transform.parent.up : Vector3.up;
            return Vector2.SignedAngle(nose, toTarget);
        }
    }
}
