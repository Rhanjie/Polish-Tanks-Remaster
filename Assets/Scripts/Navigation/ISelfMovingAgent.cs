namespace PolishTanks.Navigation
{
    /// <summary>
    /// Marks a body that walks on its own (a soldier). The navigation grid leaves it out, so a soldier standing somewhere
    /// when the grid is built never turns into an obstacle.
    /// </summary>
    public interface ISelfMovingAgent
    {
    }
}
