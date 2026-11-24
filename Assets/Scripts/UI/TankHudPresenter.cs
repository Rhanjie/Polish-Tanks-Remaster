using PolishTanks.Damage;
using UnityEngine;

namespace PolishTanks.UI
{
    /// <summary>
    /// Turns the player tank's state into what the tank HUD shows: a style class per part, the speedometer reading
    /// and scale, the machine gun belt count and the caption next to the repair key. Speeds come in world units per
    /// second and are shown in km/h. The words come from <see cref="HudText"/>.
    /// </summary>
    public static class TankHudPresenter
    {
        public const string IntactClass = "part--intact";
        public const string DestroyedClass = "part--destroyed";
        public const string DamagedClass = "part--damaged";

        public static string PartClass(PartCondition condition) => condition switch
        {
            PartCondition.Destroyed => DestroyedClass,
            PartCondition.Damaged => DamagedClass,
            _ => IntactClass,
        };

        public static int SpeedKmh(float unitsPerSecond, float kmhPerUnit) =>
            Mathf.RoundToInt(Mathf.Abs(unitsPerSecond) * kmhPerUnit);

        public static int GaugeMaxKmh(float maxUnitsPerSecond, float kmhPerUnit)
        {
            var top = Mathf.Max(1f, maxUnitsPerSecond * kmhPerUnit);
            return Mathf.CeilToInt(top / HudDial.TickStepKmh) * HudDial.TickStepKmh;
        }

        public static float GaugeFill(float unitsPerSecond, float kmhPerUnit, int gaugeMaxKmh) =>
            gaugeMaxKmh > 0 ? Mathf.Clamp01(Mathf.Abs(unitsPerSecond) * kmhPerUnit / gaugeMaxKmh) : 0f;

        public static string BeltText(int rounds, int size) => size > 0 ? string.Format(HudText.BeltCountFormat, rounds, size) : HudText.UnlimitedBelt;

        public static float BeltFill(int rounds, int size) => size > 0 ? Mathf.Clamp01((float)rounds / size) : 1f;

        public static string RepairCaption(bool repairing, bool hasWork)
        {
            if (repairing)
                return HudText.RepairStop;
            return hasWork ? HudText.RepairStart : null;
        }
    }
}
