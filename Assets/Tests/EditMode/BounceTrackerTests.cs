using System;
using NUnit.Framework;
using PolishTanks.Bouncing;

namespace PolishTanks.Tests.EditMode
{
    public class BounceTrackerTests
    {
        private static BounceTracker Create() => new BounceTracker(minInterval: 0.1f, minImpactSpeed: 1f, historySize: 3);

        [Test]
        public void TryRegister_FirstBounce_HasZeroInterval()
        {
            var tracker = Create();

            var ok = tracker.TryRegister(5f, 2f, out var bounce);

            Assert.IsTrue(ok);
            Assert.AreEqual(0f, bounce.Interval);
            Assert.AreEqual(1, bounce.Count);
        }

        [Test]
        public void TryRegister_SecondBounce_ReportsIntervalSincePrevious()
        {
            var tracker = Create();
            tracker.TryRegister(1f, 2f, out _);

            tracker.TryRegister(1.5f, 2f, out var bounce);

            Assert.AreEqual(0.5f, bounce.Interval, 1e-5f);
            Assert.AreEqual(0.5f, tracker.LastInterval, 1e-5f);
        }

        [Test]
        public void TryRegister_TooSoonAfterPrevious_IsRejected()
        {
            var tracker = Create();
            tracker.TryRegister(1f, 2f, out _);

            var ok = tracker.TryRegister(1.05f, 2f, out _);

            Assert.IsFalse(ok);
            Assert.AreEqual(1, tracker.Count);
        }

        [Test]
        public void TryRegister_ExactlyAtMinInterval_IsAccepted()
        {
            var tracker = new BounceTracker(0.5f, 0f, 3);
            tracker.TryRegister(1f, 2f, out _);

            Assert.IsTrue(tracker.TryRegister(1.5f, 2f, out _));
        }

        [Test]
        public void TryRegister_BelowMinSpeed_IsRejected()
        {
            var tracker = Create();

            Assert.IsFalse(tracker.TryRegister(1f, 0.99f, out _));
            Assert.AreEqual(0, tracker.Count);
        }

        [Test]
        public void TryRegister_NaNSpeed_IsRejected()
        {
            var tracker = Create();

            Assert.IsFalse(tracker.TryRegister(1f, float.NaN, out _));
        }

        [Test]
        public void TryRegister_Rejected_DoesNotAdvanceLastBounceTime()
        {
            var tracker = Create();
            tracker.TryRegister(1f, 2f, out _);
            tracker.TryRegister(1.05f, 2f, out _); // rejected

            tracker.TryRegister(1.2f, 2f, out var bounce);

            Assert.AreEqual(0.2f, bounce.Interval, 1e-5f);
        }

        [Test]
        public void PeakImpactSpeed_TracksMaximumNotLast()
        {
            var tracker = Create();
            tracker.TryRegister(1f, 5f, out _);
            tracker.TryRegister(2f, 2f, out _);

            Assert.AreEqual(5f, tracker.PeakImpactSpeed);
            Assert.AreEqual(2f, tracker.LastImpactSpeed);
        }

        [Test]
        public void BouncesPerMinute_WithOneSecondIntervals_IsSixty()
        {
            var tracker = Create();
            tracker.TryRegister(1f, 2f, out _);
            tracker.TryRegister(2f, 2f, out _);
            tracker.TryRegister(3f, 2f, out _);

            Assert.AreEqual(60f, tracker.BouncesPerMinute, 1e-3f);
        }

        [Test]
        public void BouncesPerMinute_AfterSingleBounce_IsZero()
        {
            var tracker = Create();
            tracker.TryRegister(1f, 2f, out _);

            Assert.AreEqual(0f, tracker.BouncesPerMinute);
        }

        [Test]
        public void ImpactHistory_BeyondCapacity_KeepsNewestOldestFirst()
        {
            var tracker = Create();
            for (var i = 1; i <= 5; i++)
                tracker.TryRegister(i, i + 1f, out _);

            CollectionAssert.AreEqual(new[] { 4f, 5f, 6f }, tracker.ImpactHistory);
        }

        [Test]
        public void Reset_AfterBounces_ClearsAllStatistics()
        {
            var tracker = Create();
            tracker.TryRegister(1f, 3f, out _);
            tracker.TryRegister(2f, 3f, out _);

            tracker.Reset();

            Assert.AreEqual(0, tracker.Count);
            Assert.AreEqual(0f, tracker.PeakImpactSpeed);
            Assert.AreEqual(0f, tracker.LastInterval);
            Assert.AreEqual(0, tracker.ImpactHistory.Count);
            Assert.AreEqual(0f, tracker.BouncesPerMinute);
        }

        [Test]
        public void Reset_ThenRegisterEarlierTime_FirstBounceHasZeroInterval()
        {
            var tracker = Create();
            tracker.TryRegister(100f, 3f, out _);
            tracker.Reset();

            tracker.TryRegister(1f, 3f, out var bounce);

            Assert.AreEqual(0f, bounce.Interval);
        }

        [Test]
        public void Constructor_NegativeMinInterval_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BounceTracker(-0.1f));
        }

        [Test]
        public void Constructor_NegativeMinSpeed_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BounceTracker(0.05f, -1f));
        }

        [Test]
        public void Constructor_ZeroHistorySize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BounceTracker(0.05f, 0.5f, 0));
        }
    }
}
