namespace PolishTanks.AI
{
    /// <summary>
    /// Whether the player is close enough to be seen, before the line of sight is checked. Normally that is the sight
    /// range; a unit already hunting the player keeps it in view much further along a clear line, so the player can't
    /// shake it off just by driving out of the circle.
    /// </summary>
    public static class Perception
    {
        public static bool InReach(float distance, float sightRange, float pursuitRangeFactor, bool hunting) =>
            distance <= sightRange || (hunting && distance <= sightRange * pursuitRangeFactor);
    }
}
