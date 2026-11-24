using NUnit.Framework;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>The revision counter and last heading of <see cref="TargetMemory"/> that the search relies on.</summary>
    public class TargetMemoryAwarenessTests
    {
        [Test]
        public void Observe_LosingSight_IncrementsRevision()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Vector2.zero, 0f);

            memory.Observe(false, Vector2.zero, 1f);

            Assert.That(memory.Revision, Is.EqualTo(1));
        }

        [Test]
        public void Observe_StillHiddenOnTheNextFrame_DoesNotIncrementRevisionAgain()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Vector2.zero, 0f);
            memory.Observe(false, Vector2.zero, 1f);

            memory.Observe(false, Vector2.zero, 2f);

            Assert.That(memory.Revision, Is.EqualTo(1));
        }

        [Test]
        public void Report_WhileHidden_IncrementsRevision()
        {
            var memory = new TargetMemory();

            memory.Report(new Vector2(5f, 5f), 1f);

            Assert.That(memory.Revision, Is.EqualTo(1));
        }

        [Test]
        public void Report_WhileTargetIsVisible_ChangesNothing()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Vector2.zero, 0f);

            memory.Report(new Vector2(5f, 5f), 1f);

            Assert.That(memory.Revision, Is.EqualTo(0));
            Assert.That(memory.LastKnownPosition, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void LastHeading_TargetMovesWhileVisible_PointsTheWayItMoved()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Vector2.zero, 0f);

            memory.Observe(true, new Vector2(0f, 2f), 1f);

            Assert.That(memory.LastHeading, Is.EqualTo(Vector2.up));
        }

        [Test]
        public void LastHeading_AfterAReport_IsUnknown()
        {
            var memory = new TargetMemory();
            memory.Observe(true, Vector2.zero, 0f);
            memory.Observe(true, new Vector2(0f, 2f), 1f);
            memory.Observe(false, Vector2.zero, 2f);

            memory.Report(new Vector2(9f, 9f), 3f);

            Assert.That(memory.LastHeading, Is.EqualTo(Vector2.zero));
        }
    }
}
