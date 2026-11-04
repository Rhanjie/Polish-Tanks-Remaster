using NUnit.Framework;
using PolishTanks.Bouncing;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class BounceColorSequenceTests
    {
        [Test]
        public void ColorFor_SameIndexTwice_IsIdentical()
        {
            var sequence = new BounceColorSequence(0.3f);

            Assert.AreEqual(sequence.ColorFor(7), sequence.ColorFor(7));
        }

        [Test]
        public void HueFor_TwoInstancesWithSameStart_Match()
        {
            Assert.AreEqual(new BounceColorSequence(0.42f).HueFor(12), new BounceColorSequence(0.42f).HueFor(12));
        }

        [Test]
        public void HueFor_ConsecutiveIndices_AreFarApartOnColorWheel()
        {
            var sequence = new BounceColorSequence(0.1f);
            for (var i = 0; i < 200; i++)
            {
                var diff = Mathf.Abs(sequence.HueFor(i + 1) - sequence.HueFor(i));
                var circular = Mathf.Min(diff, 1f - diff);
                Assert.Greater(circular, 0.35f, $"index {i}");
            }
        }

        [Test]
        public void HueFor_AnyIndex_StaysInZeroToOne()
        {
            var sequence = new BounceColorSequence(0.9f);
            foreach (var index in new[] { 0, 1, 2, 100, 1_000_000, int.MaxValue, -1, -57 })
            {
                var hue = sequence.HueFor(index);
                Assert.That(hue, Is.InRange(0f, 1f), $"index {index}");
            }
        }

        [Test]
        public void HueFor_StartHuePastOne_Wraps()
        {
            var sequence = new BounceColorSequence(1.25f);

            Assert.AreEqual(0.25f, sequence.StartHue, 1e-5f);
            Assert.AreEqual(0.25f, sequence.HueFor(0), 1e-5f);
        }

        [Test]
        public void StartHue_Negative_WrapsIntoRange()
        {
            Assert.AreEqual(0.75f, new BounceColorSequence(-0.25f).StartHue, 1e-5f);
        }

        [Test]
        public void HueFor_HugeIndex_MatchesDoublePrecisionReference()
        {
            var sequence = new BounceColorSequence(0f);
            var expected = (float)((int.MaxValue * BounceColorSequence.GoldenRatioConjugate) % 1.0);

            Assert.AreEqual(expected, sequence.HueFor(int.MaxValue), 1e-5f);
        }

        [Test]
        public void Constructor_OutOfRangeSaturationAndValue_AreClamped()
        {
            var sequence = new BounceColorSequence(0f, 5f, -2f);

            Assert.AreEqual(1f, sequence.Saturation);
            Assert.AreEqual(0f, sequence.Value);
        }
    }
}
