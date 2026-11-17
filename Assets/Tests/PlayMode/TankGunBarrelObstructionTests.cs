using NUnit.Framework;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// A shot whose barrel is pushed into a solid obstacle starts at the barrel base, while low cover between the
    /// base and the muzzle does not move the spawn point. The base is at the origin and the muzzle 2 units up.
    /// </summary>
    public class TankGunBarrelObstructionTests
    {
        private const float MuzzleDistance = 2f;

        private WeaponFixture fixture;
        private TankGun gun;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            var prefab = fixture.CreateProjectilePrefab();
            var definition = fixture.CreateDefinition(prefab);

            var root = fixture.Track(new GameObject("TankHull"));
            root.SetActive(false);
            root.AddComponent<Rigidbody2D>();
            root.AddComponent<TankMovement>();

            var barrel = new GameObject("Barrel");
            barrel.transform.SetParent(root.transform, false);
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(barrel.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, MuzzleDistance, 0f);

            gun = muzzle.AddComponent<TankGun>();
            gun.Settings = new GunSettings
            {
                reloadTime = 0.1f,
                projectile = definition,
                projectileSpeed = 10f,
                barrelReturnTime = 0.3f,
            };
            fixture.Inject(root);
            root.SetActive(true);
        }

        [TearDown]
        public void TearDown() => fixture.Dispose();

        private Vector3 FireAndGetProjectilePosition()
        {
            Physics2D.SyncTransforms();
            gun.TryFire();
            return Object.FindAnyObjectByType<Projectile>().transform.position;
        }

        [Test]
        public void TryFire_WallBetweenBaseAndMuzzle_SpawnsProjectileOnBaseSide()
        {
            fixture.CreateStaticBox(new Vector2(0f, 1f), new Vector2(2f, 0.2f));

            var position = FireAndGetProjectilePosition();

            Assert.That(position.y, Is.LessThan(1f));
        }

        [Test]
        public void TryFire_LowCoverBetweenBaseAndMuzzle_SpawnsProjectileAtMuzzle()
        {
            var cover = fixture.CreateStaticBox(new Vector2(0f, 1f), new Vector2(2f, 0.2f));
            cover.gameObject.AddComponent<LowCover>();

            var position = FireAndGetProjectilePosition();

            Assert.That(position.y, Is.EqualTo(MuzzleDistance).Within(1e-3f));
        }
    }
}
