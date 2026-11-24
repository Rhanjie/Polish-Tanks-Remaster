namespace PolishTanks.Tank.Modules
{
    /// <summary>
    /// Optional add-on bolted onto the hull. Each subclass tweaks the computed stats,
    /// so new equipment needs no change in <see cref="TankStatsCalculator"/>.
    /// </summary>
    public abstract class EquipmentModule : TankModule
    {
        public override ModuleSlot Slot => ModuleSlot.Equipment;

        public abstract void Modify(TankStats stats);
    }
}
