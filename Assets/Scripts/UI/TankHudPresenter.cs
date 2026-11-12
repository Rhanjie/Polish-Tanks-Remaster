using PolishTanks.Damage;
using UnityEngine;

namespace PolishTanks.UI
{
    /// <summary>
    /// Turns the player tank's state into what the tank HUD shows: a style class per part, the speedometer reading
    /// and scale, the machine gun belt count, the caption next to the repair key and the name of the part in the
    /// crew's hands. Speeds come in world units per second and are shown in km/h.
    /// </summary>
    public static class TankHudPresenter
    {
        public const string IntactClass = "part--intact";
        public const string DestroyedClass = "part--destroyed";
        public const string PatchedClass = "part--patched";

        public static string PartClass(PartCondition condition) => condition switch
        {
            PartCondition.Destroyed => DestroyedClass,
            PartCondition.FieldRepaired => PatchedClass,
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

        public static string BeltText(int rounds, int size) => size > 0 ? $"{rounds}/{size}" : "∞";

        public static float BeltFill(int rounds, int size) => size > 0 ? Mathf.Clamp01((float)rounds / size) : 1f;

        public static string RepairCaption(bool repairing, bool hasWork)
        {
            if (repairing)
                return "Przerwij";
            return hasWork ? "Naprawa" : null;
        }

        public static string PartName(PartId id) => id.Part switch
        {
            VehiclePart.Engine => "Silnik",
            VehiclePart.LeftTrack => "Lewa gąsienica",
            VehiclePart.RightTrack => "Prawa gąsienica",
            VehiclePart.TurretRing => "Pierścień wieży",
            VehiclePart.VisionBlock => $"Peryskop {id.Index + 1}",
            _ => id.Part.ToString(),
        };
    }
}
