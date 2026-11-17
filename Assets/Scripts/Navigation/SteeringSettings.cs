using System;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>How a tracked vehicle drives toward a point. Angles in degrees, distances in world units.</summary>
    [Serializable]
    public struct SteeringSettings
    {
        [Tooltip("Heading error that gives full steer.")]
        [Min(1f)] public float fullSteerAngle;

        [Tooltip("Heading error at which the tank stops rolling forward and turns on the spot.")]
        [Min(1f)] public float pivotAngle;

        [Tooltip("A target further behind than this, and close, is reached in reverse instead of turning round.")]
        [Range(90f, 180f)] public float reverseAngle;

        [Min(0f)] public float reverseDistance;

        [Tooltip("Distance to the last waypoint where the tank starts to slow down.")]
        [Min(0.1f)] public float slowDownDistance;

        [Tooltip("Throttle kept while slowing down, so the tank doesn't crawl the last metre.")]
        [Range(0f, 1f)] public float minApproachThrottle;

        public static SteeringSettings Default => new SteeringSettings
        {
            fullSteerAngle = 30f,
            pivotAngle = 75f,
            reverseAngle = 140f,
            reverseDistance = 7f,
            slowDownDistance = 6f,
            minApproachThrottle = 0.3f,
        };
    }
}
