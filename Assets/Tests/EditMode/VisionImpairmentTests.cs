using NUnit.Framework;
using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Per-block view loss behind the vision damage screen effect and how it eases in and out.</summary>
    public class VisionImpairmentTests
    {
        private static VehicleDamageModel FourBlocks() =>
            new VehicleDamageModel(new PartDamageChances { HasTracks = true, HasTurretRing = true, VisionBlocks = 4 });

        [Test]
        public void Step_AfterBreakAndPatch_SpoilsOnlyThoseSectors()
        {
            var model = FourBlocks();
            model.Break(VehiclePart.VisionBlock, () => 0f);
            model.Break(VehiclePart.VisionBlock, () => 0f);
            model.FieldRepair(new PartId(VehiclePart.VisionBlock, 1));
            var impairment = new VisionImpairment();

            impairment.Aim(model, false);
            impairment.Step(10f, 1f, 1f);

            Assert.That(impairment.Loss(0), Is.EqualTo(1f));
            Assert.That(impairment.Loss(1), Is.EqualTo(0.5f));
            Assert.That(impairment.Loss(2), Is.Zero);
            Assert.That(impairment.Loss(3), Is.Zero);
        }

        [Test]
        public void Step_ViewSpoilsFasterThanItClears()
        {
            var model = FourBlocks();
            var block = model.Break(VehiclePart.VisionBlock, () => 0f).Value;
            var impairment = new VisionImpairment();

            impairment.Aim(model, false);
            impairment.Step(0.2f, 5f, 1f);
            var spoiled = impairment.Loss(block.Index);
            model.FieldRepair(block);
            impairment.Aim(model, false);
            impairment.Step(0.2f, 5f, 1f);

            Assert.That(spoiled, Is.EqualTo(1f));
            Assert.That(impairment.Loss(block.Index), Is.EqualTo(0.8f).Within(1e-5f));
        }

        [Test]
        public void IsActive_StaysOnUntilViewAndFireAlarmFullyClear()
        {
            var model = FourBlocks();
            var impairment = new VisionImpairment();
            impairment.Aim(model, true);
            impairment.Step(1f, 2f, 2f);

            impairment.Clear();
            impairment.Step(0.25f, 2f, 2f);
            var midway = impairment.IsActive;
            impairment.Step(0.25f, 2f, 2f);

            Assert.That(midway, Is.True);
            Assert.That(impairment.IsActive, Is.False);
        }
    }
}
