namespace PolishTanks.Missions
{
    /// <summary>
    /// What a mission objective asks of the player: drive into a zone, destroy marked targets, hold out, or keep away
    /// from marked obstacles.
    /// </summary>
    public enum MissionObjectiveKind
    {
        Reach,
        Destroy,
        Survive,
        Avoid,
    }
}
