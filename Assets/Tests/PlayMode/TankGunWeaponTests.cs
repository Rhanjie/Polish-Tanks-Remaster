using System.Collections;
using NUnit.Framework;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Projectile spawning, recoil and belt feed of <see cref="TankGun"/>.</summary>
    public class TankGunWeaponTests
    {
        private sealed class GunRig
        {
            public Rigidbody2D Body;
            public TankMovement Movement;
            public TankGun Gun;
        }

        private WeaponFixture fixture;

        [SetUp]
        public void SetUp() => fixture = new WeaponFixture();

        [TearDown]
        public void TearDown() => fixture.Dispose();

        private GunRig BuildGun(GunSettings settings, Vector2 position)
        {
            var root = fixture.Track(new GameObject("TankHull"));
            root.SetActive(false);
            root.transform.position = position;
            var rig = new GunRig { Body = root.AddComponent<Rigidbody2D>() };
            rig.Body.mass = 1f;
            rig.Movement = root.AddComponent<TankMovement>();

            var gunObject = new GameObject("Gun");
            gunObject.transform.SetParent(root.transform, false);
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(gunObject.transform, false);

            rig.Gun = muzzle.AddComponent<TankGun>();
            rig.Gun.Settings = settings;
            fixture.Inject(root);
            root.SetActive(true);
            return rig;
        }

        private static GunSettings Settings(float reloadTime = 0.1f, float recoilImpulse = 0f, ProjectileDefinition projectile = null,
            float speed = 10f, int magazineSize = 0, float magazineReloadTime = 0f) => new GunSettings
        {
            reloadTime = reloadTime,
            projectile = projectile,
            projectileSpeed = speed,
            recoilImpulse = recoilImpulse,
            barrelReturnTime = 0.3f,
            magazineSize = magazineSize,
            magazineReloadTime = magazineReloadTime,
        };

        private ProjectileDefinition BuildDefinition(out Projectile prefab)
        {
            prefab = fixture.CreateProjectilePrefab();
            return fixture.CreateDefinition(prefab);
        }

        private static IEnumerator WaitUntilReloaded(TankGun gun)
        {
            var frames = 0;
            while (!gun.IsReloaded && frames++ < 600)
                yield return null;
        }

        [Test]
        public void TryFire_WithProjectileDefinition_SpawnsOneProjectile()
        {
            var definition = BuildDefinition(out var prefab);
            var rig = BuildGun(Settings(projectile: definition), Vector2.zero);

            rig.Gun.TryFire();

            Assert.That(fixture.Pools.For(prefab).CountActive, Is.EqualTo(1));
        }

        [Test]
        public void TryFire_WithProjectileDefinition_LaunchesAlongMuzzleAtProjectileSpeed()
        {
            var definition = BuildDefinition(out _);
            var rig = BuildGun(Settings(projectile: definition, speed: 12f), Vector2.zero);

            rig.Gun.TryFire();

            var projectile = Object.FindAnyObjectByType<Projectile>();
            Assert.That(projectile.Velocity.x, Is.EqualTo(0f).Within(1e-3f));
            Assert.That(projectile.Velocity.y, Is.EqualTo(12f).Within(1e-3f));
        }

        [Test]
        public void TryFire_WithProjectileDefinition_ProjectileIgnoresShooterBody()
        {
            var definition = BuildDefinition(out _);
            var rig = BuildGun(Settings(projectile: definition), Vector2.zero);

            rig.Gun.TryFire();

            Assert.That(Object.FindAnyObjectByType<Projectile>().Shooter, Is.SameAs(rig.Body));
        }

        [Test]
        public void TryFire_WithoutProjectileDefinition_SpawnsNothingAndStillFires()
        {
            var rig = BuildGun(Settings(), Vector2.zero);

            var fired = rig.Gun.TryFire();

            Assert.That(fired, Is.True);
            Assert.That(Object.FindAnyObjectByType<Projectile>(), Is.Null);
        }

        [Test]
        public void TryFire_WithRecoil_PushesHullOppositeToShot()
        {
            var rig = BuildGun(Settings(recoilImpulse: 10f), Vector2.zero);

            rig.Gun.TryFire();

            var kick = rig.Movement.ImpulseVelocity;
            Assert.That(Vector2.Dot(kick.normalized, -rig.Gun.LastShotDirection), Is.EqualTo(1f).Within(1e-3f));
        }

        [Test]
        public void TryFire_WithRecoil_KickIsImpulseDividedByMass()
        {
            var rig = BuildGun(Settings(recoilImpulse: 10f), Vector2.zero);

            rig.Gun.TryFire();

            Assert.That(rig.Movement.ImpulseVelocity.magnitude, Is.EqualTo(10f / rig.Body.mass).Within(1e-3f));
        }

        [Test]
        public void TryFire_WithZeroRecoil_LeavesHullAlone()
        {
            var rig = BuildGun(Settings(recoilImpulse: 0f), Vector2.zero);

            rig.Gun.TryFire();

            Assert.That(rig.Movement.ImpulseVelocity, Is.EqualTo(Vector2.zero));
        }

        [UnityTest]
        public IEnumerator TryFire_WithLargerRecoilImpulse_MovesHullFurther()
        {
            var weak = BuildGun(Settings(recoilImpulse: 5f), new Vector2(-10f, 0f));
            var strong = BuildGun(Settings(recoilImpulse: 50f), new Vector2(10f, 0f));
            var weakStart = weak.Body.position;
            var strongStart = strong.Body.position;

            weak.Gun.TryFire();
            strong.Gun.TryFire();
            for (var i = 0; i < 10; i++)
                yield return new WaitForFixedUpdate();

            var weakDistance = Vector2.Distance(weakStart, weak.Body.position);
            var strongDistance = Vector2.Distance(strongStart, strong.Body.position);
            Assert.That(weakDistance, Is.GreaterThan(0f));
            Assert.That(strongDistance, Is.GreaterThan(weakDistance));
        }

        [UnityTest]
        public IEnumerator TryFire_AfterMagazineEmptied_RefusesUntilBeltChangeFinishes()
        {
            const int rounds = 3;
            var rig = BuildGun(Settings(reloadTime: 0.01f, magazineSize: rounds, magazineReloadTime: 1f), Vector2.zero);

            for (var i = 0; i < rounds; i++)
            {
                yield return WaitUntilReloaded(rig.Gun);
                Assert.That(rig.Gun.TryFire(), Is.True, $"shot {i + 1} of {rounds} should fire");
            }
            yield return WaitUntilReloaded(rig.Gun);

            Assert.That(rig.Gun.IsChangingMagazine, Is.True);
            Assert.That(rig.Gun.TryFire(), Is.False);
        }

        [UnityTest]
        public IEnumerator TryFire_AfterBeltChange_FiresAgain()
        {
            var rig = BuildGun(Settings(reloadTime: 0.01f, magazineSize: 1, magazineReloadTime: 0.1f), Vector2.zero);
            rig.Gun.TryFire();

            var frames = 0;
            while ((rig.Gun.IsChangingMagazine || !rig.Gun.IsReloaded) && frames++ < 600)
                yield return null;

            Assert.That(rig.Gun.TryFire(), Is.True);
        }

        [Test]
        public void TryFire_WithZeroMagazineSize_NeverRunsOutOfRounds()
        {
            var rig = BuildGun(Settings(reloadTime: 0.01f, magazineSize: 0), Vector2.zero);

            rig.Gun.TryFire();

            Assert.That(rig.Gun.IsChangingMagazine, Is.False);
        }

        [Test]
        public void RoundsInMagazine_AfterOneShot_DropsByOne()
        {
            var rig = BuildGun(Settings(magazineSize: 5, magazineReloadTime: 1f), Vector2.zero);

            rig.Gun.TryFire();

            Assert.That(rig.Gun.RoundsInMagazine, Is.EqualTo(4));
        }
    }
}
