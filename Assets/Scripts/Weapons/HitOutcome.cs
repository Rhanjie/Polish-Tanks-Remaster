namespace PolishTanks.Weapons
{
    /// <summary>What a target made of a hit, ordered by precedence when several handlers answer.</summary>
    public enum HitOutcome
    {
        Ignored,
        Blocked,
        Ricochet,
        Penetrated,
    }
}
