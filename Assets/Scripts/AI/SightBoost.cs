using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// How much further a unit sees right after the player fired: the flash and the noise give the player away. The
    /// boost jumps up on a shot and fades back to normal over its duration; a weaker shot never cuts a stronger boost short.
    /// </summary>
    public sealed class SightBoost
    {
        private float peak = 1f;
        private float start;
        private float duration;

        public void Trigger(float time, float factor, float seconds)
        {
            if (factor < Factor(time))
                return;
            peak = factor;
            start = time;
            duration = seconds;
        }

        public float Factor(float time)
        {
            if (duration <= 0f)
                return 1f;
            var progress = (time - start) / duration;
            return progress >= 1f || progress < 0f ? 1f : Mathf.Lerp(peak, 1f, progress);
        }

        public void Clear() => duration = 0f;
    }
}
