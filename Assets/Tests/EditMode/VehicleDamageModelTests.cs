using NUnit.Framework;
using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    public class VehicleDamageModelTests
    {
        private static readonly PartId Engine = new PartId(VehiclePart.Engine);

        private static PartDamageChances Full => new PartDamageChances { HasTracks = true, HasTurretRing = true, VisionBlocks = 4 };

        private static float First() => 0f;

        private static void BreakAllBlocks(VehicleDamageModel model)
        {
            for (var i = 0; i < model.VisionBlocks; i++)
                model.Break(VehiclePart.VisionBlock, First);
        }

        [Test]
        public void Visibility_OneOfFourBlocksDestroyed_IsThreeQuarters()
        {
            var model = new VehicleDamageModel(Full);

            model.Break(VehiclePart.VisionBlock, First);

            Assert.That(model.Visibility, Is.EqualTo(0.75f).Within(1e-5f));
        }

        [Test]
        public void CopyConditionsFrom_SmallerLayout_KeepsDamageOfSharedParts()
        {
            var old = new VehicleDamageModel(new PartDamageChances { HasTracks = true, VisionBlocks = 3 });
            old.Break(VehiclePart.LeftTrack, First);
            var rebuilt = new VehicleDamageModel(Full);

            rebuilt.CopyConditionsFrom(old);

            Assert.That(rebuilt.LeftTrack, Is.EqualTo(0f));
        }

        [Test]
        public void Visibility_AllBlocksDestroyed_IsZero()
        {
            var model = new VehicleDamageModel(Full);

            BreakAllBlocks(model);

            Assert.That(model.Visibility, Is.EqualTo(0f));
        }

        [Test]
        public void Visibility_OneBlockRepairedAfterAllDestroyed_IsOneEighth()
        {
            var model = new VehicleDamageModel(Full);
            BreakAllBlocks(model);

            model.FieldRepair(new PartId(VehiclePart.VisionBlock, 0));

            Assert.That(model.Visibility, Is.EqualTo(0.125f).Within(1e-5f));
        }

        [Test]
        public void FieldRepair_IntactPart_IsRejected()
        {
            var model = new VehicleDamageModel(Full);

            Assert.That(model.FieldRepair(Engine), Is.False);
        }

        [Test]
        public void FieldRepair_PartAlreadyRepaired_IsRejected()
        {
            var model = new VehicleDamageModel(Full);
            model.Break(VehiclePart.Engine, First);
            model.FieldRepair(Engine);

            Assert.That(model.FieldRepair(Engine), Is.False);
        }

        [Test]
        public void FieldRepair_DestroyedPart_RestoresHalfEffect()
        {
            var model = new VehicleDamageModel(Full);
            model.Break(VehiclePart.Engine, First);

            model.FieldRepair(Engine);

            Assert.That(model.EnginePower, Is.EqualTo(VehicleDamageModel.FieldRepairEffectiveness));
        }

        [Test]
        public void Break_AllPartsOfKindDestroyed_ReturnsNull()
        {
            var model = new VehicleDamageModel(Full);
            model.Break(VehiclePart.Engine, First);

            Assert.That(model.Break(VehiclePart.Engine, First), Is.Null);
        }

        [Test]
        public void Break_RandomAtUpperBound_PicksLastBlockWithoutOverflow()
        {
            var model = new VehicleDamageModel(Full);

            var broken = model.Break(VehiclePart.VisionBlock, () => 1f);

            Assert.That(broken, Is.EqualTo(new PartId(VehiclePart.VisionBlock, 3)));
        }

        [Test]
        public void Break_PartMissingFromLayout_ReturnsNull()
        {
            var model = new VehicleDamageModel(new PartDamageChances());

            Assert.That(model.Break(VehiclePart.LeftTrack, First), Is.Null);
        }

        [Test]
        public void LeftTrack_PartMissingFromLayout_CountsAsIntact()
        {
            var model = new VehicleDamageModel(new PartDamageChances());

            Assert.That(model.LeftTrack, Is.EqualTo(1f));
        }
    }
}
