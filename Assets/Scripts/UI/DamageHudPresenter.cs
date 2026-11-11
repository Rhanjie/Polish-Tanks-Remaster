using PolishTanks.Damage;

namespace PolishTanks.UI
{
    /// <summary>
    /// Turns the tank's part damage and repair state into what the damage HUD shows: a style class per part,
    /// whether the panel is up at all, the caption next to the repair key and the name of the part in the crew's hands.
    /// The panel only appears while something is wrong (a broken or patched part, or a fire).
    /// </summary>
    public static class DamageHudPresenter
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

        public static bool IsVisible(VehicleDamageModel model, bool burning) =>
            burning || (model != null && model.HasDamage);

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
