namespace PolishTanks.Navigation
{
    /// <summary>How an obstacle affects the grid: solid ones block their cells, soft ones only make them costlier.</summary>
    public enum ObstacleKind
    {
        Solid,
        Soft,
    }
}
