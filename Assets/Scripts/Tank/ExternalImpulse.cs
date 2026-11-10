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

        public float AngularVelocity { get; private set; }

        public void Add(Vector2 impulse, Vector2 offset, float mass, float inertia)
        {
            if (mass > 0f)
                Velocity += impulse / mass;
            if (inertia > 0f)
                AngularVelocity += (offset.x * impulse.y - offset.y * impulse.x) / inertia * Mathf.Rad2Deg;
        }

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
