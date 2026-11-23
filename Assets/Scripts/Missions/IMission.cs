namespace PolishTanks.Missions
{
    /// <summary>The mission of the current scene. <see cref="Progress"/> is null in scenes without one (the sandbox).</summary>
    public interface IMission
    {
        MissionDefinition Definition { get; }
        MissionProgress Progress { get; }
    }
}
