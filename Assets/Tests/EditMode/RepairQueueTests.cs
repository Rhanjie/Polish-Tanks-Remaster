using NUnit.Framework;
using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Order in which <see cref="RepairQueue"/> hands out destroyed parts.</summary>
    public class RepairQueueTests
    {
        private static PartDamageChances Full => new PartDamageChances { HasTracks = true, HasTurretRing = true, VisionBlocks = 4 };

        private static float First() => 0f;

        private static VehicleDamageModel ModelWith(params VehiclePart[] destroyed)
        {
            var model = new VehicleDamageModel(Full);
            foreach (var part in destroyed)
                model.Break(part, First);
            return model;
        }

        [Test]
        public void Next_EngineAndTrackDestroyed_ReturnsEngine()
        {
            var model = ModelWith(VehiclePart.LeftTrack, VehiclePart.VisionBlock, VehiclePart.Engine);

            Assert.That(RepairQueue.Next(model), Is.EqualTo(new PartId(VehiclePart.Engine)));
        }

        [Test]
        public void Next_AllVisionBlocksAndTrackDestroyed_ReturnsVisionBlockFirst()
        {
            var model = ModelWith(VehiclePart.LeftTrack, VehiclePart.VisionBlock, VehiclePart.VisionBlock,
                VehiclePart.VisionBlock, VehiclePart.VisionBlock);

            Assert.That(RepairQueue.Next(model)?.Part, Is.EqualTo(VehiclePart.VisionBlock));
        }

        [Test]
        public void Next_OneVisionBlockAndTrackDestroyed_ReturnsTrackFirst()
        {
            var model = ModelWith(VehiclePart.LeftTrack, VehiclePart.VisionBlock);

            Assert.That(RepairQueue.Next(model), Is.EqualTo(new PartId(VehiclePart.LeftTrack)));
        }

        [Test]
        public void Next_TrackAndTurretRingDestroyed_ReturnsTrackFirst()
        {
            var model = ModelWith(VehiclePart.TurretRing, VehiclePart.RightTrack);

            Assert.That(RepairQueue.Next(model), Is.EqualTo(new PartId(VehiclePart.RightTrack)));
        }

        [Test]
        public void Next_TurretRingAndVisionBlockDestroyed_ReturnsTurretRingFirst()
        {
            var model = ModelWith(VehiclePart.VisionBlock, VehiclePart.TurretRing);

            Assert.That(RepairQueue.Next(model), Is.EqualTo(new PartId(VehiclePart.TurretRing)));
        }

        [Test]
        public void Next_DestroyedEngineAlreadyFieldRepaired_SkipsIt()
        {
            var model = ModelWith(VehiclePart.Engine, VehiclePart.LeftTrack);
            model.FieldRepair(new PartId(VehiclePart.Engine));

            Assert.That(RepairQueue.Next(model), Is.EqualTo(new PartId(VehiclePart.LeftTrack)));
        }

        [Test]
        public void Next_NothingDestroyed_ReturnsNull()
        {
            Assert.That(RepairQueue.Next(ModelWith()), Is.Null);
        }

        [Test]
        public void Next_OnlyFieldRepairedParts_ReturnsNull()
        {
            var model = ModelWith(VehiclePart.Engine);
            model.FieldRepair(new PartId(VehiclePart.Engine));

            Assert.That(RepairQueue.Next(model), Is.Null);
        }

        [Test]
        public void Next_NullModel_ReturnsNull()
        {
            Assert.That(RepairQueue.Next(null), Is.Null);
        }
    }
}
