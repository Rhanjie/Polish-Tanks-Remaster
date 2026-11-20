namespace PolishTanks.Missions
{
    /// <summary>
    /// Where an objective stands: waiting for earlier ones, waiting for the player to drive into its start marker,
    /// in progress, done, or missed.
    /// </summary>
    public enum ObjectiveState
    {
        Pending,
        Waiting,
        Active,
        Completed,
        Failed,
    }
}
