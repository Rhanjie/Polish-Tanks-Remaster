namespace PolishTanks.Navigation
{
    /// <summary>Throttle and steer for <c>TankMovement.SetInput</c>: both -1..1, positive steer turns right.</summary>
    public readonly struct DriveInput
    {
        public static readonly DriveInput Stop = new DriveInput(0f, 0f);

        public DriveInput(float throttle, float steer)
        {
            Throttle = throttle;
            Steer = steer;
        }

        public float Throttle { get; }
        public float Steer { get; }
    }
}
