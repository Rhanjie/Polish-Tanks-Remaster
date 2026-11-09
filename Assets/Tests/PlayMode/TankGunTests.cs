using System.Collections;
using NUnit.Framework;
using PolishTanks.Tank;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class TankGunTests
    {
        private const float ReloadTime = 0.3f;

        private GameObject gunObject;
        private TankGun gun;
        private int fireCount;

        [SetUp]
        public void SetUp()
        {
            fireCount = 0;
            gunObject = new GameObject("TestGun");
            gunObject.SetActive(false); // configure before Awake builds the reload timer
            gun = gunObject.AddComponent<TankGun>();
            gun.Settings = new GunSettings { reloadTime = ReloadTime, muzzleFlashDuration = 0f };
            gunObject.SetActive(true);
            gun.Fired += _ => fireCount++;
        }

        [TearDown]
        public void TearDown()
        {
            if (gunObject != null) Object.Destroy(gunObject);
        }

        private static IEnumerator WaitSeconds(float seconds)
        {
            var end = Time.time + seconds;
            while (Time.time < end)
                yield return null;
        }

        [Test]
        public void TryFire_WhenReloaded_ReturnsTrue()
        {
            Assert.IsTrue(gun.TryFire());
        }

        [Test]
        public void TryFire_WhenReloaded_RaisesFiredOnce()
        {
            gun.TryFire();

            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void TryFire_BeforeReloadFinishes_ReturnsFalse()
        {
            gun.TryFire();

            Assert.IsFalse(gun.TryFire());
        }

        [Test]
        public void TryFire_BeforeReloadFinishes_DoesNotRaiseFiredAgain()
        {
            gun.TryFire();
            gun.TryFire();

            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void TryFire_WhenDisabled_ReturnsFalse()
        {
            gun.enabled = false;

            Assert.IsFalse(gun.TryFire());
        }

        [UnityTest]
        public IEnumerator TryFire_AfterReloadTime_FiresAgain()
        {
            gun.TryFire();

            yield return WaitSeconds(ReloadTime + 0.1f);

            Assert.IsTrue(gun.TryFire());
        }

        [UnityTest]
        public IEnumerator SetTrigger_Held_FiresAgainAfterReload()
        {
            gun.TryFire();
            gun.SetTrigger(true);

            yield return WaitSeconds(ReloadTime + 0.1f);

            Assert.AreEqual(2, fireCount);
        }

        [UnityTest]
        public IEnumerator SetTrigger_Released_DoesNotFireAfterReload()
        {
            gun.TryFire();
            gun.SetTrigger(true);
            gun.SetTrigger(false);

            yield return WaitSeconds(ReloadTime + 0.1f);

            Assert.AreEqual(1, fireCount);
        }

        [UnityTest]
        public IEnumerator IsReloaded_RightAfterShot_IsFalse()
        {
            gun.TryFire();
            yield return null;

            Assert.IsFalse(gun.IsReloaded);
        }
    }
}
