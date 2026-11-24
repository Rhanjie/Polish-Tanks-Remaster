namespace PolishTanks.AI
{
    /// <summary>How an AI learned where the player is without seeing it: a shot flew past, a friend radioed, or it was hit.</summary>
    public enum AlertSource
    {
        NearMiss,
        Radio,
        Hit,
    }
}
