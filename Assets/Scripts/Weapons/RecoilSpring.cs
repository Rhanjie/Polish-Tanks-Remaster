using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>Offset that jumps back on a shot and eases home without overshooting.</summary>
    public sealed class RecoilSpring
    {
        private const float RestThreshold = 0.0001f;
        private float velocity;

        public float Offset { get; private set; }

        public float ReturnTime { get; set; }

        public RecoilSpring(float returnTime)
        {
            ReturnTime = returnTime;
        }

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

            Offset = Mathf.SmoothDamp(Offset, 0f, ref velocity, Mathf.Max(0.001f, ReturnTime / 3f), Mathf.Infinity, deltaTime);
            if (Offset < RestThreshold)
            {
                Offset = 0f;
                velocity = 0f;
            }
        }
    }
}
