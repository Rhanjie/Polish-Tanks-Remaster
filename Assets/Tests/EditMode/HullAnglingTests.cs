using NUnit.Framework;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class HullAnglingTests
    {
        private AiTestRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new AiTestRig()
                .WithDoctrine("hullAngle", 25f)
                .WithDoctrine("hullSway", 0f)
                .WithDoctrine("hullSideSwitchMin", 4f)
                .WithDoctrine("hullSideSwitchMax", 4f);
        }

        [TearDown]
        public void TearDown() => rig.Dispose();

        [TestCase(0.9f, 25f)]
        [TestCase(0.1f, -25f)]
        public void DesiredForward_TurnsOffTheTargetLineByTheAngleOnTheRolledSide(float roll, float expected)
        {
            var angling = new HullAngling(() => roll);

            var forward = angling.DesiredForward(Vector2.right * 10f, 0f, rig.Doctrine);

            Assert.That(Vector2.SignedAngle(Vector2.right, forward), Is.EqualTo(expected).Within(0.01f));
        }

        [Test]
        public void DesiredForward_RollsTheSideAgainOnlyAfterTheSwitchTime()
        {
            var roll = 0.9f;
            var angling = new HullAngling(() => roll);
            angling.DesiredForward(Vector2.right, 0f, rig.Doctrine);
            roll = 0.1f;

            angling.DesiredForward(Vector2.right, 3.9f, rig.Doctrine);
            var before = angling.Side;
            angling.DesiredForward(Vector2.right, 4f, rig.Doctrine);

            Assert.That((before, angling.Side), Is.EqualTo((1f, -1f)));
        }

        [Test]
        public void Steer_HeadingToTheLeft_TurnsLeftAndHoldsInsideTheDeadZone()
        {
            var left = HullAngling.Steer(Vector2.up, Quaternion.Euler(0f, 0f, 45f) * Vector2.up);
            var close = HullAngling.Steer(Vector2.up, Quaternion.Euler(0f, 0f, 1f) * Vector2.up);

            Assert.That((left, close), Is.EqualTo((-1f, 0f)));
        }
    }
}
