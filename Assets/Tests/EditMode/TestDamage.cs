using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Hits a chosen part of a <see cref="VehicleDamageModel"/> until it is destroyed, picking the vision block by index.</summary>
    public static class TestDamage
    {
        public static PartId Destroy(VehicleDamageModel model, VehiclePart part, int index = 0)
        {
            var count = part == VehiclePart.VisionBlock ? model.VisionBlocks : 1;
            var pick = (index + 0.5f) / count;
            model.Break(part, () => pick);
            model.Break(part, () => pick);
            return new PartId(part, part == VehiclePart.VisionBlock ? index : 0);
        }
    }
}
