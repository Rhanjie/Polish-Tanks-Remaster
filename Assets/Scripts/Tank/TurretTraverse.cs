using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Turns a turret toward a target angle with limited speed and acceleration,
    /// so it trails behind the aim point instead of snapping to it.
    /// </summary>
    public sealed class TurretTraverse
    {
        public TurretSettings Settings { get; set; }

        public float Angle { get; private set; }

        public float Velocity { get; private set; }

        public TurretTraverse(TurretSettings settings, float startAngle = 0f)
        {
            Settings = settings;
            Angle = Normalize(startAngle);
        }

        public void Step(float targetAngle, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            var s = Settings;
            var error = Mathf.DeltaAngle(Angle, targetAngle);
            var desired = Mathf.Clamp(error / Mathf.Max(0.01f, s.responseTime), -s.maxTraverseSpeed, s.maxTraverseSpeed);
            Velocity = Mathf.MoveTowards(Velocity, desired, s.traverseAcceleration * deltaTime);

            var step = Velocity * deltaTime;
            if (Mathf.Abs(step) >= Mathf.Abs(error) && Mathf.Sign(step) == Mathf.Sign(error))
            {
                Angle = Normalize(targetAngle);
                Velocity = 0f;
                return;
            }

            Angle = Normalize(Angle + step);
        }

        private static float Normalize(float angle) => Mathf.DeltaAngle(0f, angle);
    }
}
