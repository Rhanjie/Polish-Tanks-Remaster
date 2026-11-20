namespace PolishTanks.Missions
{
    /// <summary>Where an objective stands: waiting for earlier ones, in progress, done, or missed.</summary>
    public enum ObjectiveState
    {
        Pending,
        Active,
        Completed,
        Failed,
    }
}
