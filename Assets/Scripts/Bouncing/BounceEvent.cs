namespace PolishTanks.Bouncing
{
    public readonly struct BounceEvent
    {
        /// <summary>Total number of bounces including this one (1 for the first bounce).</summary>
        public readonly int Count;
        public readonly float ImpactSpeed;
        /// <summary>Seconds since the previous counted bounce; 0 for the first one.</summary>
        public readonly float Interval;
        public readonly float Time;

        public BounceEvent(int count, float impactSpeed, float interval, float time)
        {
            Count = count;
            ImpactSpeed = impactSpeed;
            Interval = interval;
            Time = time;
        }
    }
}
