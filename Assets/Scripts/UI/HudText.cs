using PolishTanks.Damage;

namespace PolishTanks.UI
{
    /// <summary>
    /// Every player-visible string of the tank HUD that code sets at runtime; static labels live in TankHud.uxml.
    /// Kept in one place so a localization module can replace them; values go in through format placeholders.
    /// </summary>
    public static class HudText
    {
        public const string GunReady = "Ready";
        public const string GunLoading = "Loading";
        public const string BeltChange = "Reloading";
        public const string UnlimitedBelt = "∞";
        public const string BeltCountFormat = "{0}/{1}";
        public const string RepairStart = "Repair";
        public const string RepairStop = "Stop";
        public const string VisionBlockFormat = "Periscope {0}";

        public static string PartName(PartId id) => id.Part switch
        {
            VehiclePart.Engine => "Engine",
            VehiclePart.LeftTrack => "Left track",
            VehiclePart.RightTrack => "Right track",
            VehiclePart.TurretRing => "Turret ring",
            VehiclePart.VisionBlock => string.Format(VisionBlockFormat, id.Index + 1),
            _ => id.Part.ToString(),
        };
    }
}
