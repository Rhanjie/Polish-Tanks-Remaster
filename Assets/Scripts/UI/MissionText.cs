using System;
using PolishTanks.Missions;

namespace PolishTanks.UI
{
    /// <summary>
    /// Every player-visible string of the mission HUD and the mission summary that code sets at runtime; static labels
    /// live in the UXML files and objective descriptions in the mission assets. Values go in through format placeholders.
    /// </summary>
    public static class MissionText
    {
        public const string OptionalFormat = "Optional: {0}";
        public const string CountFormat = "{0} ({1}/{2})";
        public const string TimerFormat = "{0} ({1}:{2:00})";
        public const string RespectFormat = "+{0}";
        public const string ObjectiveComplete = "Objective complete";
        public const string ObjectiveFailed = "Objective failed";
        public const string NewObjective = "New objective";
        public const string NewOptionalObjective = "New optional objective";
        public const string TotalRespect = "Respect earned";
        public const string TanksDestroyedFormat = "Enemy tanks destroyed: {0}";
        public const string ModulesDestroyedFormat = "Enemy modules knocked out: {0}";
        public const string InfantryKilledFormat = "Enemy soldiers killed: {0}";

        public static string Describe(ObjectiveStatus status, bool markOptional = true)
        {
            var objective = status.Objective;
            var text = objective.Kind switch
            {
                MissionObjectiveKind.Destroy when status.Required > 1 =>
                    string.Format(CountFormat, objective.Description, Math.Min(status.Count, status.Required), status.Required),
                MissionObjectiveKind.Survive when status.State == ObjectiveState.Active =>
                    Timer(objective.Description, status.TimeLeft),
                _ => objective.Description,
            };
            return objective.Optional && markOptional ? string.Format(OptionalFormat, text) : text;
        }

        public static string Respect(int amount) => string.Format(RespectFormat, amount);

        public static string LossLine(EnemyLoss loss, int count) => loss switch
        {
            EnemyLoss.Tank => string.Format(TanksDestroyedFormat, count),
            EnemyLoss.Module => string.Format(ModulesDestroyedFormat, count),
            EnemyLoss.Infantry => string.Format(InfantryKilledFormat, count),
            _ => count.ToString(),
        };

        private static string Timer(string description, float seconds)
        {
            var whole = (int)Math.Ceiling(seconds);
            return string.Format(TimerFormat, description, whole / 60, whole % 60);
        }
    }
}
