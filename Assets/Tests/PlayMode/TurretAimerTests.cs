using System.Collections;
using System.Reflection;
using NUnit.Framework;
using PolishTanks.Tank;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class TurretAimerTests
    {
        private GameObject hull;
        private Transform turret;
        private TurretAimer aimer;

        [SetUp]
        public void SetUp()
        {
            hull = new GameObject("TestHull");
            hull.SetActive(false); // keep Awake from running until the turret is assigned

            turret = new GameObject("Turret").transform;
            turret.SetParent(hull.transform, false);

            aimer = hull.AddComponent<TurretAimer>();
            typeof(TurretAimer)
                .GetField("turret", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(aimer, turret);

            hull.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (hull != null) Object.Destroy(hull);
        }

        private static IEnumerator WaitSeconds(float seconds)
        {
            var end = Time.time + seconds;
            while (Time.time < end)
                yield return null;
        }

        [UnityTest]
        public IEnumerator SetAimPoint_SidePoint_TurretLagsBehindTarget()
        {
            aimer.SetAimPoint(new Vector2(-10f, 0f)); // 90 degrees to the left

            yield return null;
            yield return null;
            yield return null;

            Assert.Greater(Mathf.Abs(aimer.RemainingAngle), 10f);
        }

        [UnityTest]
        public IEnumerator SetAimPoint_SidePoint_TurretStartsTurning()
        {
            aimer.SetAimPoint(new Vector2(-10f, 0f));

            yield return WaitSeconds(0.3f);

            Assert.Less(Mathf.Abs(aimer.RemainingAngle), 89f);
        }

        [UnityTest]
        public IEnumerator SetAimPoint_SidePoint_ConvergesOnTarget()
        {
            aimer.SetAimPoint(new Vector2(-10f, 0f));

            yield return WaitSeconds(3f);

            Assert.Less(Mathf.Abs(aimer.RemainingAngle), 5f);
        }

        [UnityTest]
        public IEnumerator Update_HullTurns_TurretIsDraggedAwayFromTarget()
        {
            aimer.SetAimPoint(new Vector2(10f, 0f));
            yield return WaitSeconds(3f);

            hull.transform.Rotate(0f, 0f, 90f);

            Assert.Greater(Mathf.Abs(aimer.RemainingAngle), 45f);
        }

        [UnityTest]
        public IEnumerator Update_HullTurns_TurretCatchesUpWithTarget()
        {
            aimer.SetAimPoint(new Vector2(10f, 0f));
            yield return WaitSeconds(3f);
            hull.transform.Rotate(0f, 0f, 90f);

            yield return WaitSeconds(3f);

            Assert.Less(Mathf.Abs(aimer.RemainingAngle), 5f);
        }

        [UnityTest]
        public IEnumerator RemainingAngle_NoAimPoint_IsZero()
        {
            yield return null;

            Assert.AreEqual(0f, aimer.RemainingAngle);
        }

        [UnityTest]
        public IEnumerator SetAimPoint_PointOnTurretPivot_DoesNotRotateTurret()
        {
            aimer.SetAimPoint(turret.position);

            yield return WaitSeconds(0.2f);

            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, turret.localEulerAngles.z), 0.001f);
        }
    }
}
