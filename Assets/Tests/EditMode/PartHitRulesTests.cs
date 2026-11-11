using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    public class PartHitRulesTests
    {
        private const float DefaultFactor = 0.4f;

        private readonly List<VehiclePart> result = new List<VehiclePart>();

        private static PartDamageChances Certain => new PartDamageChances
        {
            HasTracks = true,
            HasTurretRing = false,
            VisionBlocks = 4,
            TrackBreak = 1f,
            EngineBreak = 1f,
            TurretRingJam = 1f,
            VisionBlockBreak = 1f,
        };

        private void Roll(HitSide side, bool left, bool penetrated, PartDamageChances chances, float roll = 0f, float factor = DefaultFactor) =>
            PartHitRules.Roll(side, left, penetrated, chances, factor, () => roll, result);

        [TestCase(true, VehiclePart.LeftTrack)]
        [TestCase(false, VehiclePart.RightTrack)]
        public void Roll_PenetratingSideHit_BreaksTrackOnStruckSide(bool leftSide, VehiclePart expected)
        {
            Roll(HitSide.Side, leftSide, true, Certain);

            Assert.That(result, Is.EqualTo(new[] { expected }));
        }

        [Test]
        public void Roll_NonPenetratingSideHitRollBelowFactor_BreaksTrack()
        {
            Roll(HitSide.Side, true, false, Certain, roll: 0.3f);

            Assert.That(result, Does.Contain(VehiclePart.LeftTrack));
        }

        [Test]
        public void Roll_NonPenetratingSideHitRollAboveFactor_BreaksNothing()
        {
            Roll(HitSide.Side, true, false, Certain, roll: 0.5f);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Roll_NonPenetratingSideHitWithZeroFactor_BreaksNothing()
        {
            Roll(HitSide.Side, true, false, Certain, factor: 0f);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Roll_NonPenetratingRearHit_NeverBreaksEngine()
        {
            Roll(HitSide.Rear, false, false, Certain);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Roll_PenetratingRearHit_BreaksEngine()
        {
            Roll(HitSide.Rear, false, true, Certain);

            Assert.That(result, Does.Contain(VehiclePart.Engine));
        }

        [Test]
        public void Roll_PenetratingFrontHit_BreaksVisionBlock()
        {
            Roll(HitSide.Front, false, true, Certain);

            Assert.That(result, Does.Contain(VehiclePart.VisionBlock));
        }

        [Test]
        public void Roll_PenetratingFrontHitWithoutVisionBlocks_BreaksNothing()
        {
            var chances = Certain;
            chances.VisionBlocks = 0;

            Roll(HitSide.Front, false, true, chances);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Roll_PenetratingFrontHitOnTankWithTurretRing_CanJamRing()
        {
            var chances = Certain;
            chances.HasTurretRing = true;

            Roll(HitSide.Front, false, true, chances);

            Assert.That(result, Does.Contain(VehiclePart.TurretRing));
        }
    }
}
