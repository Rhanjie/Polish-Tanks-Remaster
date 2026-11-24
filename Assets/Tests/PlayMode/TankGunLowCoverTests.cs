using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// A gun's cover clearance counts from its muzzle, so a tank with a long barrel fires over cover just past the muzzle
    /// even when that cover is further from the hull than the clearance. The muzzle sits 5 units up from the hull.
    /// </summary>
    public class TankGunLowCoverTests
    {
        private const float MuzzleDistance = 5f;
        private const float Clearance = 2.5f;

        private WeaponFixture fixture;
        private ProjectileDefinition definition;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            definition = fixture.CreateDefinition(fixture.CreateProjectilePrefab());
        }

        [TearDown]
        public void TearDown() => fixture.Dispose();

        private TankGun BuildGun()
        {
            var root = fixture.Track(new GameObject("TankHull"));
            root.SetActive(false);
            root.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            root.AddComponent<TankMovement>();
            var breech = new GameObject("Turret");
            breech.transform.SetParent(root.transform, false);
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(breech.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, MuzzleDistance, 0f);
            var gun = muzzle.AddComponent<TankGun>();
            TankRig.SetField(gun, "coverClearance", Clearance);
            gun.Settings = new GunSettings
            {
                reloadTime = 0.1f, projectile = definition, projectileSpeed = 20f, barrelReturnTime = 0.3f,
                damage = 30f, penetrationMin = 100f, penetrationMax = 100f,
            };
            fixture.Inject(root);
            root.SetActive(true);
            return gun;
        }

        private Damageable CreateCover(float y)
        {
            var profile = fixture.Track(ScriptableObject.CreateInstance<DamageableProfile>());
            TankRig.SetField(profile, "hitPoints", 100f);
            TankRig.SetField(profile, "armor", new ArmorProfile(5f, 5f, 5f));

            var go = fixture.Track(new GameObject("Crate"));
            go.SetActive(false);
            go.transform.position = new Vector3(0f, y, 0f);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            go.AddComponent<LowCover>();
            var damageable = go.AddComponent<Damageable>();
            TankRig.SetField(damageable, "profile", profile);
            fixture.Inject(go);
            go.SetActive(true);
            return damageable;
        }

        private void FireAndFly(TankGun gun)
        {
            Physics2D.SyncTransforms();
            Assert.That(gun.TryFire(), Is.True);
            Object.FindAnyObjectByType<Projectile>().Step(0.75f);
        }

        [Test]
        public void TryFire_CoverJustPastLongBarrel_FliesOverIt()
        {
            var gun = BuildGun();
            var cover = CreateCover(MuzzleDistance + 1.5f);

            FireAndFly(gun);

            Assert.That(cover.HitPoints, Is.EqualTo(100f));
        }

        [Test]
        public void TryFire_CoverBeyondClearancePastMuzzle_IsHit()
        {
            var gun = BuildGun();
            var cover = CreateCover(MuzzleDistance + Clearance + 2f);

            FireAndFly(gun);

            Assert.That(cover.HitPoints, Is.LessThan(100f));
        }
    }
}
