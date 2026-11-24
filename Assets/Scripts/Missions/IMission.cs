namespace PolishTanks.Missions
{
    /// <summary>
    /// The mission of the current scene. <see cref="Progress"/> is null in scenes without one (the sandbox).
    /// <see cref="Track"/> adds a unit spawned during the game, so its losses pay respect.
    /// </summary>
    public interface IMission
    {
        MissionDefinition Definition { get; }
        MissionProgress Progress { get; }
        void Track(UnityEngine.GameObject unit);
    }
}
