using NUnit.Framework;
using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    public class VehicleDamageModelTests
    {
        private static readonly PartId Engine = new PartId(VehiclePart.Engine);

        private static PartDamageChances Full => new PartDamageChances { HasTracks = true, HasTurretRing = true, VisionBlocks = 4 };

        private static float First() => 0f;

        private static void DestroyAllBlocks(VehicleDamageModel model)
        {
            for (var i = 0; i < model.VisionBlocks; i++)
                TestDamage.Destroy(model, VehiclePart.VisionBlock, i);
        }

        [Test]
        public void Break_TwoHits_DamageThenDestroyThePart()
        {
            var model = new VehicleDamageModel(Full);

            model.Break(VehiclePart.Engine, First);
            var afterFirst = model.ConditionOf(Engine);
            model.Break(VehiclePart.Engine, First);

            Assert.That(afterFirst, Is.EqualTo(PartCondition.Damaged));
            Assert.That(model.ConditionOf(Engine), Is.EqualTo(PartCondition.Destroyed));
        }

        [TestCase(VehiclePart.Engine, 0.6f)]
        [TestCase(VehiclePart.LeftTrack, 0.8f)]
        [TestCase(VehiclePart.TurretRing, 0.5f)]
        public void Break_FirstHit_LeavesThePartAtItsDamagedEffectiveness(VehiclePart part, float expected)
        {
            var model = new VehicleDamageModel(Full);

            var broken = model.Break(part, First).Value;

            Assert.That(model.Effectiveness(broken), Is.EqualTo(expected).Within(1e-5f));
        }

        [Test]
        public void Break_VisionHitOnDestroyedBlock_ChangesNothing()
        {
            var model = new VehicleDamageModel(Full);
            TestDamage.Destroy(model, VehiclePart.VisionBlock, 0);

            var broken = model.Break(VehiclePart.VisionBlock, First);

            Assert.That(broken, Is.Null);
            Assert.That(model.ConditionOf(new PartId(VehiclePart.VisionBlock, 1)), Is.EqualTo(PartCondition.Intact));
        }

        [Test]
        public void Visibility_OneOfFourBlocksDestroyed_IsThreeQuarters()
        {
            var model = new VehicleDamageModel(Full);

            TestDamage.Destroy(model, VehiclePart.VisionBlock);

            Assert.That(model.Visibility, Is.EqualTo(0.75f).Within(1e-5f));
        }

        [Test]
        public void CopyConditionsFrom_SmallerLayout_KeepsDamageOfSharedParts()
        {
            var old = new VehicleDamageModel(new PartDamageChances { HasTracks = true, VisionBlocks = 3 });
            TestDamage.Destroy(old, VehiclePart.LeftTrack);
            var rebuilt = new VehicleDamageModel(Full);

            rebuilt.CopyConditionsFrom(old);

            Assert.That(rebuilt.LeftTrack, Is.EqualTo(0f));
        }

        [Test]
        public void Visibility_AllBlocksDestroyed_IsZero()
        {
            var model = new VehicleDamageModel(Full);

            DestroyAllBlocks(model);

            Assert.That(model.Visibility, Is.EqualTo(0f));
        }

        [Test]
        public void Visibility_OneBlockRepairedAfterAllDestroyed_IsOneEighth()
        {
            var model = new VehicleDamageModel(Full);
            DestroyAllBlocks(model);

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
        public void FieldRepair_DamagedPart_IsRejected()
        {
            var model = new VehicleDamageModel(Full);
            model.Break(VehiclePart.Engine, First);

            Assert.That(model.FieldRepair(Engine), Is.False);
        }

        [Test]
        public void FieldRepair_DestroyedPart_BringsItBackToDamaged()
        {
            var model = new VehicleDamageModel(Full);
            TestDamage.Destroy(model, VehiclePart.Engine);

            model.FieldRepair(Engine);

            Assert.That(model.ConditionOf(Engine), Is.EqualTo(PartCondition.Damaged));
            Assert.That(model.EnginePower, Is.EqualTo(VehicleDamageModel.DamagedEngine));
        }

        [Test]
        public void Break_DestroyedPart_ReturnsNull()
        {
            var model = new VehicleDamageModel(Full);
            TestDamage.Destroy(model, VehiclePart.Engine);

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
