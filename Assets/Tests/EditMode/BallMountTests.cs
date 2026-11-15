using NUnit.Framework;
using PolishTanks.Tank;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>How a ball mount swings toward a target without leaving its arc.</summary>
    public class BallMountTests
    {
        [TestCase(40f, 15f)]
        [TestCase(-40f, -15f)]
        public void Swing_TargetBeyondArc_StopsAtTheArcEdge(float target, float expected)
        {
            var angle = BallMount.Swing(0f, target, 15f, 1000f);

            Assert.That(angle, Is.EqualTo(expected));
        }

        [Test]
        public void Swing_SmallStep_MovesOnlyThatFar()
        {
            var angle = BallMount.Swing(0f, 10f, 15f, 3f);

            Assert.That(angle, Is.EqualTo(3f));
        }
    }
}
