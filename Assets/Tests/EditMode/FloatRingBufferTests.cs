using System;
using System.Linq;
using NUnit.Framework;
using PolishTanks.Bouncing;

namespace PolishTanks.Tests.EditMode
{
    public class FloatRingBufferTests
    {
        [TestCase(0)]
        [TestCase(-3)]
        public void Constructor_NonPositiveCapacity_Throws(int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FloatRingBuffer(capacity));
        }

        [Test]
        public void Add_BelowCapacity_KeepsInsertionOrder()
        {
            var buffer = new FloatRingBuffer(4);
            buffer.Add(1f);
            buffer.Add(2f);
            buffer.Add(3f);

            CollectionAssert.AreEqual(new[] { 1f, 2f, 3f }, buffer.ToArray());
        }

        [Test]
        public void Add_BeyondCapacity_OverwritesOldestAndIndexZeroIsOldest()
        {
            var buffer = new FloatRingBuffer(3);
            for (var i = 1; i <= 5; i++)
                buffer.Add(i);

            CollectionAssert.AreEqual(new[] { 3f, 4f, 5f }, buffer.ToArray());
        }

        [Test]
        public void Count_AfterOverflow_StaysAtCapacity()
        {
            var buffer = new FloatRingBuffer(2);
            for (var i = 0; i < 10; i++)
                buffer.Add(i);

            Assert.AreEqual(2, buffer.Count);
        }

        [Test]
        public void Indexer_OutOfRange_Throws()
        {
            var buffer = new FloatRingBuffer(3);
            buffer.Add(1f);

            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = buffer[1]; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = buffer[-1]; });
        }

        [Test]
        public void Average_Empty_IsZero()
        {
            Assert.AreEqual(0f, new FloatRingBuffer(3).Average());
        }

        [Test]
        public void Average_AfterOverflow_UsesOnlyRetainedValues()
        {
            var buffer = new FloatRingBuffer(2);
            buffer.Add(100f);
            buffer.Add(2f);
            buffer.Add(4f);

            Assert.AreEqual(3f, buffer.Average(), 1e-5f);
        }

        [Test]
        public void Clear_AfterOverflow_EmptiesAndRestartsOrder()
        {
            var buffer = new FloatRingBuffer(2);
            buffer.Add(1f);
            buffer.Add(2f);
            buffer.Add(3f);
            buffer.Clear();
            buffer.Add(9f);

            CollectionAssert.AreEqual(new[] { 9f }, buffer.ToArray());
        }
    }
}
