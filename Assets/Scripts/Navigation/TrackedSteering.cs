using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// Steering for a tracked hull toward one point. Steer grows with the heading error; throttle falls as the error
    /// grows, down to zero at the pivot angle, so a sharp corner is taken by turning on the spot rather than in a
    /// wide arc. A point close behind is reached in reverse. Approaching the final waypoint the tank slows down.
    /// </summary>
    public static class TrackedSteering
    {
        public static DriveInput Toward(Vector2 position, Vector2 forward, Vector2 target, bool isFinalPoint, in SteeringSettings settings)
        {
            var toTarget = target - position;
            var distance = toTarget.magnitude;
            if (distance < 0.0001f || forward.sqrMagnitude < 0.0001f)
                return DriveInput.Stop;

            var angle = Vector2.SignedAngle(forward, toTarget);
            var reverse = Mathf.Abs(angle) > settings.reverseAngle && distance < settings.reverseDistance;
            if (reverse)
                angle = Vector2.SignedAngle(-forward, toTarget);

            var steer = Mathf.Clamp(-angle / settings.fullSteerAngle, -1f, 1f);
            var throttle = Mathf.Clamp01(1f - Mathf.Abs(angle) / settings.pivotAngle);
            if (isFinalPoint)
                throttle *= Mathf.Max(settings.minApproachThrottle, Mathf.Clamp01(distance / settings.slowDownDistance));
            return new DriveInput(reverse ? -throttle : throttle, steer);
        }
    }
}
