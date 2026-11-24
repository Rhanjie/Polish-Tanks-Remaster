using System;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// Where a fighting tank points its hull: not straight at the enemy but turned off by the doctrine's angle, so shells
    /// meet the front plate at a slant and glance off more often. The hull also sways a little around that angle and
    /// now and then swings over to the other side, which keeps the enemy gunner from settling on one plate. Steer
    /// follows <see cref="Navigation.TrackedSteering"/>: positive turns right.
    /// </summary>
    public sealed class HullAngling
    {
        public const float FullSteerAngle = 30f;
        public const float DeadZone = 2f;

        private readonly Func<float> random01;
        private float side = 1f;
        private float nextSwitch = float.NegativeInfinity;

        public HullAngling(Func<float> random01)
        {
            this.random01 = random01;
        }

        public float Side => side;

        public Vector2 DesiredForward(Vector2 toTarget, float time, CombatDoctrine doctrine)
        {
            if (time >= nextSwitch)
            {
                side = random01() < 0.5f ? -1f : 1f;
                nextSwitch = time + Mathf.Lerp(doctrine.HullSideSwitchMin, doctrine.HullSideSwitchMax, random01());
            }

            var period = Mathf.Max(0.1f, doctrine.HullSwayPeriod);
            var angle = side * doctrine.HullAngle + Mathf.Sin(time * Mathf.PI * 2f / period) * doctrine.HullSway;
            var direction = toTarget.sqrMagnitude > 1e-6f ? toTarget.normalized : Vector2.up;
            return Quaternion.Euler(0f, 0f, angle) * direction;
        }

        public static float Steer(Vector2 forward, Vector2 desired)
        {
            var angle = Vector2.SignedAngle(forward, desired);
            return Mathf.Abs(angle) <= DeadZone ? 0f : Mathf.Clamp(-angle / FullSteerAngle, -1f, 1f);
        }
    }
}
