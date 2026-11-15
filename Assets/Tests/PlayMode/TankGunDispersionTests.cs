using System.Collections;
using NUnit.Framework;
using PolishTanks.Tank;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>A live <see cref="TankGun"/> opens its spread when it fires and lets it close while it waits.</summary>
    public class TankGunDispersionTests
    {
        private GameObject gunObject;
        private TankGun gun;

        [SetUp]
        public void SetUp()
        {
            gunObject = new GameObject("TestGun");
            gunObject.SetActive(false);
            gun = gunObject.AddComponent<TankGun>();
            gun.Settings = new GunSettings
            {
                reloadTime = 0.05f,
                dispersion = 0.5f,
                maxDispersion = 10f,
                shotDispersion = 3f,
                aimTime = 0.2f,
                barrelReturnTime = 0.3f,
            };
            gunObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (gunObject != null) Object.DestroyImmediate(gunObject);
        }

        [Test]
        public void Dispersion_AfterFiring_IsAboveSettled()
        {
            gun.TryFire();

            Assert.That(gun.Dispersion, Is.GreaterThan(gun.SettledDispersion + 1f));
        }

        [UnityTest]
        public IEnumerator Dispersion_AfterFiringAndWaiting_ReturnsToSettled()
        {
            gun.TryFire();

            var frames = 0;
            while (gun.Dispersion > gun.SettledDispersion + 0.1f && frames++ < 600)
                yield return null;

            Assert.That(gun.Dispersion, Is.EqualTo(gun.SettledDispersion).Within(0.1f));
        }
    }
}
