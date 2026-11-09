using System;
using UnityEngine;

namespace PolishTanks.Tank
{
    [Serializable]
    public struct TankDriveSettings
    {
        [Tooltip("Top speed going forward, units per second.")]
        [Min(0f)] public float maxForwardSpeed;

        [Tooltip("Top speed in reverse, units per second.")]
        [Min(0f)] public float maxReverseSpeed;

        [Tooltip("How fast the tank gains speed in the throttle direction, units/s².")]
        [Min(0f)] public float acceleration;

        [Tooltip("Deceleration when throttle opposes the current motion, units/s².")]
        [Min(0f)] public float braking;

        [Tooltip("Deceleration with no throttle, units/s².")]
        [Min(0f)] public float coastDeceleration;

        [Tooltip("Hull turn rate when standing still (pivot steer), degrees per second.")]
        [Min(0f)] public float turnRate;

        [Tooltip("Hull turn rate at top forward speed, degrees per second.")]
        [Min(0f)] public float turnRateAtMaxSpeed;

        [Tooltip("How fast the hull reaches the requested turn rate, degrees/s².")]
        [Min(0f)] public float turnAcceleration;

        public static TankDriveSettings Default => new TankDriveSettings
        {
            maxForwardSpeed = 6f,
            maxReverseSpeed = 2.5f,
            acceleration = 5f,
            braking = 12f,
            coastDeceleration = 6f,
            turnRate = 70f,
            turnRateAtMaxSpeed = 45f,
            turnAcceleration = 360f,
        };
    }
}
