namespace PolishTanks.AI
{
    /// <summary>Where a search stands: driving to the lost spot, looking down each open direction, roaming around, or given up.</summary>
    public enum SearchPhase
    {
        Approach,
        LookAround,
        Roam,
        Done,
    }
}
