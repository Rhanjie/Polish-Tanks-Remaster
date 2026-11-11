namespace PolishTanks.Damage
{
    /// <summary>
    /// State of one vehicle part. A field repair brings a destroyed part back to half its normal effect;
    /// only a workshop restores it fully.
    /// </summary>
    public enum PartCondition
    {
        Intact,
        Destroyed,
        FieldRepaired,
    }
}
