namespace PolishTanks.GameLoop
{
    /// <summary>Where a gameplay scene stands for the player: playing, in the pause menu, or lost.</summary>
    public enum GameLoopPhase
    {
        Playing,
        Paused,
        Defeated,
    }
}
