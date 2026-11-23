using System;

namespace PolishTanks.Missions
{
    /// <summary>Runtime state of one <see cref="MissionObjective"/>: its state, destroyed targets and time left.</summary>
    public sealed class ObjectiveStatus
    {
        public ObjectiveStatus(MissionObjective objective)
        {
            Objective = objective;
            Required = objective.Kind == MissionObjectiveKind.Destroy ? Math.Max(1, objective.TargetCount) : 1;
            TimeLeft = objective.SurviveSeconds;
        }

        public MissionObjective Objective { get; }
        public ObjectiveState State { get; internal set; }
        public int Count { get; internal set; }
        public int Required { get; internal set; }
        public float TimeLeft { get; internal set; }
        public int EarnedRespect => State == ObjectiveState.Completed ? Objective.Respect : 0;
    }
}
