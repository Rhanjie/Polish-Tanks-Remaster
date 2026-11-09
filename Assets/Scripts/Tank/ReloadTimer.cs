using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Time between shots. Fast weapons keep the leftover time of the frame they fired in,
    /// so their fire rate doesn't snap to whole frames; idle time is never banked.
    /// </summary>
    public sealed class ReloadTimer
    {
        // Positive: seconds until ready. Zero or negative: ready; a negative value is how far
        // this frame overshot the reload, and is only kept if the gun fires this frame.
        private float remaining;

        public float ReloadTime { get; set; }
        public bool IsReady => remaining <= 0f;

        /// <summary>0 right after a shot, 1 when ready to fire.</summary>
        public float Progress => ReloadTime > 0f ? 1f - Mathf.Clamp01(remaining / ReloadTime) : 1f;

        public ReloadTimer(float reloadTime)
        {
            ReloadTime = reloadTime;
        }

        public void Tick(float deltaTime)
        {
            if (remaining > 0f)
                remaining -= deltaTime;
            else
                remaining = 0f; // ready and idle for a whole frame: drop any leftover
        }

        /// <summary>Starts a reload and returns true if the gun was ready.</summary>
        public bool TryConsume()
        {
            if (!IsReady)
                return false;
            remaining += Mathf.Max(0.0001f, ReloadTime);
            return true;
        }
    }
}
