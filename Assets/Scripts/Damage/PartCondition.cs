namespace PolishTanks.Damage
{
    /// <summary>
    /// State of one vehicle part. Every part takes two hits: the first leaves it damaged, the second destroys it.
    /// A field repair brings a destroyed part back to damaged; only a workshop restores it to intact.
    /// </summary>
    public enum PartCondition
    {
        Intact,
        Damaged,
        Destroyed,
    }
}
