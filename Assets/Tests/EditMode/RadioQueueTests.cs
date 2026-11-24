using NUnit.Framework;
using PolishTanks.Audio;

namespace PolishTanks.Tests.EditMode
{
    public class RadioQueueTests
    {
        [Test]
        public void Enqueue_SameLineWithinCooldown_IsDropped()
        {
            var queue = new RadioQueue(cooldown: 8f);
            queue.Enqueue(CrewLine.EngineFire, 0f);
            queue.TryNext(0f, out _);

            Assert.That(queue.Enqueue(CrewLine.EngineFire, 5f), Is.False);
            Assert.That(queue.Enqueue(CrewLine.EngineFire, 9f), Is.True);
        }

        [Test]
        public void Enqueue_LineAlreadyWaiting_IsNotRepeated()
        {
            var queue = new RadioQueue(cooldown: 0f);
            queue.Enqueue(CrewLine.TrackBroken, 0f);
            queue.Enqueue(CrewLine.TrackBroken, 0f);

            Assert.That(queue.Count, Is.EqualTo(1));
        }

        [Test]
        public void Enqueue_PastCapacity_DropsTheOldestReport()
        {
            var queue = new RadioQueue(cooldown: 0f, capacity: 2);
            queue.Enqueue(CrewLine.EngineDamaged, 0f);
            queue.Enqueue(CrewLine.TurretJammed, 0f);
            queue.Enqueue(CrewLine.RepairStarted, 0f);

            queue.TryNext(0f, out var first);
            queue.TryNext(0f, out var second);

            Assert.That(first, Is.EqualTo(CrewLine.TurretJammed));
            Assert.That(second, Is.EqualTo(CrewLine.RepairStarted));
        }
    }
}
