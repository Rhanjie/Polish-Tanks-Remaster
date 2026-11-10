using System.Collections;
using NUnit.Framework;
using PolishTanks.Tank;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class TankMovementTests
    {
        private GameObject tankObject;
        private GameObject wallObject;
        private TankMovement tank;

        [SetUp]
        public void SetUp()
        {
            tankObject = new GameObject("TestTank");
            tankObject.AddComponent<BoxCollider2D>().size = Vector2.one;
            tank = tankObject.AddComponent<TankMovement>();
        }

        [TearDown]
        public void TearDown()
        {
            if (tankObject != null) Object.Destroy(tankObject);
            if (wallObject != null) Object.Destroy(wallObject);
        }

        private static IEnumerator WaitFixedUpdates(int count)
        {
            for (var i = 0; i < count; i++)
                yield return new WaitForFixedUpdate();
        }

        private void BuildWallAhead()
        {
            wallObject = new GameObject("Wall");
            wallObject.transform.position = new Vector3(0f, 2.5f, 0f);
            wallObject.AddComponent<BoxCollider2D>().size = new Vector2(5f, 1f);
        }

        [UnityTest]
        public IEnumerator SetInput_FullThrottle_MovesAlongForwardAxis()
        {
            tank.SetInput(1f, 0f);

            yield return WaitFixedUpdates(50);

            Assert.Greater(tankObject.transform.position.y, 1f);
        }

        [UnityTest]
        public IEnumerator SetInput_FullThrottle_DoesNotDriftSideways()
        {
            tank.SetInput(1f, 0f);

            yield return WaitFixedUpdates(50);

            Assert.AreEqual(0f, tankObject.transform.position.x, 0.05f);
        }

        [UnityTest]
        public IEnumerator SetInput_ReverseThrottle_MovesBackward()
        {
            tank.SetInput(-1f, 0f);

            yield return WaitFixedUpdates(50);

            Assert.Less(tankObject.transform.position.y, -0.5f);
        }

        [UnityTest]
        public IEnumerator SetInput_SteerRight_RotatesClockwise()
        {
            tank.SetInput(0f, 1f);

            yield return WaitFixedUpdates(30);

            Assert.Less(Mathf.DeltaAngle(0f, tankObject.transform.eulerAngles.z), -5f);
        }

        [UnityTest]
        public IEnumerator SetInput_ValueAboveRange_IsClampedToTopSpeed()
        {
            tank.SetInput(100f, 0f);

            yield return WaitFixedUpdates(200);

            Assert.LessOrEqual(tank.CurrentSpeed, tank.Settings.maxForwardSpeed + 0.01f);
        }

        [UnityTest]
        public IEnumerator FixedUpdate_DrivingIntoWall_StopsBeforeWall()
        {
            BuildWallAhead();
            tank.SetInput(1f, 0f);

            yield return WaitFixedUpdates(150);

            Assert.Less(tankObject.transform.position.y, 1.6f);
        }

        [UnityTest]
        public IEnumerator CurrentSpeed_DrivingIntoWall_DropsFarBelowTopSpeed()
        {
            BuildWallAhead();
            tank.SetInput(1f, 0f);

            yield return WaitFixedUpdates(150);

            Assert.Less(tank.CurrentSpeed, tank.Settings.maxForwardSpeed * 0.3f);
        }

        [UnityTest]
        public IEnumerator OnDisable_WhileDriving_ClearsVelocity()
        {
            tank.SetInput(1f, 0f);
            yield return WaitFixedUpdates(20);

            tank.enabled = false;

            Assert.AreEqual(0f, tankObject.GetComponent<Rigidbody2D>().linearVelocity.magnitude, 0.0001f);
        }
    }
}
