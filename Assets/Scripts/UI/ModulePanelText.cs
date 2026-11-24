using PolishTanks.Tank.Modules;

namespace PolishTanks.UI
{
    /// <summary>
    /// Every player-visible string of the F1 module panel and its stat table that code sets at runtime; static labels
    /// live in DebugModulePanel.uxml. Kept in one place so a localization module can replace them; values go in through
    /// format placeholders, never by gluing sentences together.
    /// </summary>
    public static class ModulePanelText
    {
        public const string NoModule = "(none)";
        public const string ModuleCaptionFormat = "{0}  ·  T{1}  ·  {2:0.0} t";
        public const string DoesNotFitFormat = "{0}  ✕ doesn't fit";
        public const string ModifiedLoadoutFormat = "{0} (modified)";
        public const string WreckFormat = "{0} (wreck)";
        public const string EquipmentTitleFormat = "Equipment {0}/{1}";
        public const string ModuleDescriptionFormat = "{0}: {1}";
        public const string BuildFailed = "Couldn't build the tank, see the console.";
        public const string PresetInvalidFormat = "Preset {0} is invalid, see the console.";
        public const string DoesNotFitHullFormat = "{0} doesn't fit the {1}.";

        public const string Mass = "Mass";
        public const string PowerToWeight = "Power to weight";
        public const string TopSpeed = "Top speed";
        public const string Acceleration = "Acceleration";
        public const string HullTraverse = "Hull traverse";
        public const string TurretTraverse = "Turret traverse";
        public const string Reload = "Reload";
        public const string Penetration = "Penetration";
        public const string Damage = "Damage";
        public const string Dispersion = "Dispersion";
        public const string FrontArmor = "Front armor";
        public const string SideArmor = "Side armor";
        public const string RearArmor = "Rear armor";
        public const string TurretFrontArmor = "Turret front";
        public const string HitPoints = "Hit points";
        public const string ViewRange = "View range";
        public const string Ammo = "Ammo";
        public const string Cost = "Cost";

        public const string TonnesUnit = " t";
        public const string PowerToWeightUnit = " hp/t";
        public const string SpeedUnit = " km/h";
        public const string DegreesPerSecondUnit = "°/s";
        public const string SecondsUnit = " s";
        public const string MillimetresUnit = " mm";
        public const string DegreesUnit = "°";

        public static string SlotName(ModuleSlot slot) => slot switch
        {
            ModuleSlot.Hull => "Hull",
            ModuleSlot.Engine => "Engine",
            ModuleSlot.Transmission => "Transmission",
            ModuleSlot.Tracks => "Tracks",
            ModuleSlot.Turret => "Turret",
            ModuleSlot.MainGun => "Main gun",
            ModuleSlot.CoaxialMachineGun => "Coaxial MG",
            ModuleSlot.HullMachineGun => "Hull MG",
            ModuleSlot.Optics => "Optics",
            ModuleSlot.Equipment => "Equipment",
            _ => slot.ToString(),
        };
    }
}
