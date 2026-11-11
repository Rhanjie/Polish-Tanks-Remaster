using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// An engine fire: burns for a set time and goes out on its own, biting off hit points every interval.
    /// A new ignition while burning restarts the clock but keeps the tick rhythm.
    /// </summary>
    public sealed class FireState
    {
        private float untilTick;

        public bool IsBurning => Remaining > 0f;
        public float Remaining { get; private set; }

        public void Ignite(float duration, float tickInterval)
        {
            if (!IsBurning)
                untilTick = tickInterval;
            Remaining = Mathf.Max(Remaining, duration);
        }

        public void Extinguish()
        {
            Remaining = 0f;
            untilTick = 0f;
        }

        public int Tick(float deltaTime, float tickInterval)
        {
            if (!IsBurning || deltaTime <= 0f)
                return 0;

            var burned = Mathf.Min(deltaTime, Remaining);
            Remaining -= burned;
            untilTick -= burned;

            var ticks = 0;
            var interval = Mathf.Max(0.01f, tickInterval);
            while (untilTick <= 0f)
            {
                ticks++;
                untilTick += interval;
            }
            return ticks;
        }
    }
}
