using NUnit.Framework;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class TargetMemoryTests
    {
        private static readonly Vector2 Spot = new Vector2(3f, 4f);

        [Test]
        public void Observe_VisibleThenHidden_RemembersLastSeenPosition()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Spot, 1f);

            memory.Observe(false, Vector2.zero, 2f);

            Assert.That(memory.Awareness, Is.EqualTo(TargetAwareness.Remembered));
            Assert.That(memory.LastKnownPosition, Is.EqualTo(Spot));
        }

        [Test]
        public void Observe_NeverSeen_StaysUnaware()
        {
            var memory = new TargetMemory();

            memory.Observe(false, Spot, 1f);

            Assert.That(memory.Awareness, Is.EqualTo(TargetAwareness.Unaware));
        }

        [Test]
        public void IsSpotted_BeforeReactionTime_IsFalse()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Spot, 1f);

            Assert.That(memory.IsSpotted(1.5f, 1f), Is.False);
        }

        [Test]
        public void IsSpotted_AfterReactionTime_IsTrue()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Spot, 1f);
            memory.Observe(true, Spot, 1.5f);

            Assert.That(memory.IsSpotted(2f, 1f), Is.True);
        }

        [Test]
        public void IsSpotted_SightLostAndRegained_RestartsReactionTime()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Spot, 0f);
            memory.Observe(false, Spot, 1f);
            memory.Observe(true, Spot, 5f);

            Assert.That(memory.IsSpotted(5.2f, 1f), Is.False);
        }

        [Test]
        public void IsSpotted_HiddenTarget_IsFalseEvenAfterLongContact()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Spot, 0f);
            memory.Observe(false, Spot, 10f);

            Assert.That(memory.IsSpotted(20f, 0f), Is.False);
        }

        [Test]
        public void Report_TargetHidden_BecomesRememberedAtReportedPosition()
        {
            var memory = new TargetMemory();

            memory.Report(Spot, 2f);

            Assert.That(memory.Awareness, Is.EqualTo(TargetAwareness.Remembered));
            Assert.That(memory.LastKnownPosition, Is.EqualTo(Spot));
        }

        [Test]
        public void Report_TargetVisible_KeepsSeenPosition()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Spot, 1f);

            memory.Report(new Vector2(-9f, -9f), 2f);

            Assert.That(memory.LastKnownPosition, Is.EqualTo(Spot));
        }

        [Test]
        public void Forget_WithinMemory_KeepsRemembering()
        {
            var memory = new TargetMemory();
            memory.Report(Spot, 0f);

            memory.Forget(10f, 10f);

            Assert.That(memory.Awareness, Is.EqualTo(TargetAwareness.Remembered));
        }

        [Test]
        public void Forget_PastMemory_BecomesUnaware()
        {
            var memory = new TargetMemory();
            memory.Report(Spot, 0f);

            memory.Forget(10.1f, 10f);

            Assert.That(memory.Awareness, Is.EqualTo(TargetAwareness.Unaware));
        }

        [Test]
        public void Forget_TargetStillVisible_DoesNothing()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Spot, 0f);

            memory.Forget(100f, 10f);

            Assert.That(memory.IsVisible, Is.True);
        }
    }
}
