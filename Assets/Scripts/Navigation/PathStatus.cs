namespace PolishTanks.Navigation
{
    /// <summary>
    /// How a path search ended. Partial means the goal can't be reached and the path stops at the reachable cell
    /// closest to it, which is usually what an AI wants (get as near as possible).
    /// </summary>
    public enum PathStatus
    {
        Found,
        Partial,
        NoPath,
    }
}
