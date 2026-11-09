using UnityEngine;

namespace PolishTanks.Tank
{
    public sealed class ReloadTimer
    {
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
                remaining = Mathf.Max(0f, remaining - deltaTime);
        }

        /// <summary>Starts a reload and returns true if the gun was ready.</summary>
        public bool TryConsume()
        {
            if (!IsReady)
                return false;
            remaining = ReloadTime;
            return true;
        }
    }
}
