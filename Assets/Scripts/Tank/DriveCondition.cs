namespace PolishTanks.Tank
{
    /// <summary>
    /// How much of the drivetrain still works: engine power and each track from 0 (out) to 1 (intact).
    /// With one track out the hull can only pivot round it and creep by <see cref="CrippledCrawl"/> of top speed.
    /// When one track is weaker than the other, driving pulls the hull toward it by <see cref="TrackPull"/>
    /// of the turn rate at full throttle.
    /// </summary>
    public readonly struct DriveCondition
    {
        public readonly float Power;
        public readonly float LeftTrack;
        public readonly float RightTrack;
        public readonly float CrippledCrawl;
        public readonly float TrackPull;

        public DriveCondition(float power, float leftTrack, float rightTrack, float crippledCrawl, float trackPull = 0f)
        {
            Power = power;
            LeftTrack = leftTrack;
            RightTrack = rightTrack;
            CrippledCrawl = crippledCrawl;
            TrackPull = trackPull;
        }

        public static DriveCondition Healthy => new DriveCondition(1f, 1f, 1f, 0f);

        public bool LeftTrackOut => LeftTrack <= 0f;
        public bool RightTrackOut => RightTrack <= 0f;
    }
}
