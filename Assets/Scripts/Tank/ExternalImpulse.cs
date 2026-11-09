using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Velocity from outside forces (gun recoil, hits) layered on top of the track drive.
    /// The drive overwrites the body's velocity every step, so these kicks are tracked here
    /// and fade out on their own.
    /// </summary>
    public sealed class ExternalImpulse
    {
        private const float RestLinear = 0.001f;
        private const float RestAngular = 0.01f;

        public Vector2 Velocity { get; private set; }

        /// <summary>Degrees per second, positive counter-clockwise.</summary>
        public float AngularVelocity { get; private set; }

        /// <param name="impulse">World impulse (mass × velocity).</param>
        /// <param name="offset">Where it acts, relative to the center of mass.</param>
        /// <param name="mass">Body mass; non-positive ignores the linear part.</param>
        /// <param name="inertia">Rotational inertia; non-positive ignores the spin.</param>
        public void Add(Vector2 impulse, Vector2 offset, float mass, float inertia)
        {
            if (mass > 0f)
                Velocity += impulse / mass;
            if (inertia > 0f)
                AngularVelocity += (offset.x * impulse.y - offset.y * impulse.x) / inertia * Mathf.Rad2Deg;
        }

        /// <param name="linearDecay">Per-second exponential decay rate of the linear part.</param>
        /// <param name="angularDecay">Per-second exponential decay rate of the spin.</param>
        public void Step(float deltaTime, float linearDecay, float angularDecay)
        {
            if (deltaTime <= 0f)
                return;
            Velocity *= Mathf.Exp(-linearDecay * deltaTime);
            AngularVelocity *= Mathf.Exp(-angularDecay * deltaTime);
            if (Velocity.sqrMagnitude < RestLinear * RestLinear)
                Velocity = Vector2.zero;
            if (Mathf.Abs(AngularVelocity) < RestAngular)
                AngularVelocity = 0f;
        }

        /// <summary>
        /// Lets a collision eat into the shove. <paramref name="velocityLost"/> is how much velocity
        /// physics took away compared with what was requested; the part along the shove cancels it,
        /// so a tank recoiling into a wall doesn't spring back off it.
        /// </summary>
        public void AbsorbLoss(Vector2 velocityLost)
        {
            var speed = Velocity.magnitude;
            if (speed <= 0f)
                return;
            var direction = Velocity / speed;
            var loss = Vector2.Dot(velocityLost, direction);
            if (loss <= 0f)
                return;
            Velocity = direction * Mathf.Max(0f, speed - loss);
        }

        public void Reset()
        {
            Velocity = Vector2.zero;
            AngularVelocity = 0f;
        }
    }
}
