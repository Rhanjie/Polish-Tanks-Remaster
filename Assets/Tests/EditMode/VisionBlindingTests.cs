using NUnit.Framework;
using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Short blinding flashes on vision blocks: how long they last and how they combine with block damage.</summary>
    public class VisionBlindingTests
    {
        private static VehicleDamageModel FourBlocks() =>
            new VehicleDamageModel(new PartDamageChances { HasTracks = true, HasTurretRing = true, VisionBlocks = 4 });

        [Test]
        public void Tick_PastFlashDuration_EndsBlinding()
        {
            var blinding = new VisionBlinding();
            blinding.Flash(1, 1f, 0.8f);

            blinding.Tick(0.9f);
            var during = blinding.IsAnyBlinded;
            blinding.Tick(0.2f);

            Assert.That(during, Is.True);
            Assert.That(blinding.IsAnyBlinded, Is.False);
        }

        [Test]
        public void Visibility_DamagedBlockAndFlashOnDifferentBlocks_CombinesBothLosses()
        {
            var model = FourBlocks();
            TestDamage.Destroy(model, VehiclePart.VisionBlock, 0);
            var blinding = new VisionBlinding();
            blinding.Flash(1, 1f, 0.5f);

            var visibility = blinding.Visibility(model);

            Assert.That(visibility, Is.EqualTo((0f + 0.5f + 1f + 1f) / 4f).Within(1e-5f));
        }
    }
}
