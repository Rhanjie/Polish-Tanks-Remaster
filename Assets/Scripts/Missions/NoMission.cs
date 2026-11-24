namespace PolishTanks.Missions
{
    /// <summary><see cref="IMission"/> for scenes without a <see cref="MissionDirector"/>.</summary>
    public sealed class NoMission : IMission
    {
        public MissionDefinition Definition => null;
        public MissionProgress Progress => null;

        public void Track(UnityEngine.GameObject unit)
        {
        }
    }
}
