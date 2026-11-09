using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>Offset that jumps back on a shot and eases home without overshooting.</summary>
    public sealed class RecoilSpring
    {
        private const float RestThreshold = 0.0001f;
        private float velocity;

        /// <summary>Current kick-back distance, always >= 0.</summary>
        public float Offset { get; private set; }

        /// <summary>Seconds to settle most of the way back.</summary>
        public float ReturnTime { get; set; }

        public RecoilSpring(float returnTime)
        {
            ReturnTime = returnTime;
        }

        /// <summary>Pushes the offset back by <paramref name="distance"/>; overlapping kicks don't stack beyond the larger one.</summary>
        public void Kick(float distance)
        {
            if (distance <= 0f)
                return;
            Offset = Mathf.Max(Offset, distance);
            velocity = 0f;
        }

        public void Step(float deltaTime)
        {
            if (Offset <= 0f || deltaTime <= 0f)
                return;

            // SmoothDamp is critically damped; a third of the return time gets ~95% of the way.
            Offset = Mathf.SmoothDamp(Offset, 0f, ref velocity, Mathf.Max(0.001f, ReturnTime / 3f), Mathf.Infinity, deltaTime);
            if (Offset < RestThreshold)
            {
                Offset = 0f;
                velocity = 0f;
            }
        }
    }
}
