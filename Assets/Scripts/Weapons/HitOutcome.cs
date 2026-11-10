namespace PolishTanks.Weapons
{
    /// <summary>What a target made of a hit: not its business, stopped by armor, or taken.</summary>
    public enum HitOutcome
    {
        Ignored,
        Blocked,
        Penetrated,
    }
}
