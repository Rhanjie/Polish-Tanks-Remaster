using NUnit.Framework;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Camera shake sent by <see cref="TankGun"/> when it fires.</summary>
    public class TankGunShakeTests
    {
        private WeaponFixture fixture;

        [SetUp]
        public void SetUp() => fixture = new WeaponFixture();

        [TearDown]
        public void TearDown() => fixture.Dispose();

        [Test]
        public void TryFire_Fired_ShakesFromGunOppositeToShotWithShakeForce()
        {
            var root = fixture.Track(new GameObject("Gun"));
            root.SetActive(false);
            root.transform.position = new Vector3(5f, 3f, 0f);
            var gun = root.AddComponent<TankGun>();
            gun.Settings = new GunSettings { reloadTime = 1f, projectileSpeed = 10f, shakeForce = 0.5f };
            fixture.Inject(root);
            root.SetActive(true);

            gun.TryFire();

            var kick = fixture.CameraShake.Kicks[0];
            Assert.That(Vector2.Distance(kick.direction, -gun.LastShotDirection), Is.LessThan(0.001f));
            Assert.That(kick.force, Is.EqualTo(0.5f));
            Assert.That(kick.origin, Is.EqualTo(new Vector2(5f, 3f)));
        }
    }
}
