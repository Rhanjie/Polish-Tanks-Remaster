using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Shots fired from close behind <see cref="LowCover"/> fly over it; shots fired from further away, or with no
    /// clearance, still hit it. The target sits 3 units up the +Y line of fire.
    /// </summary>
    public class LowCoverTests
    {
        private const float Step = 0.5f;

        private WeaponFixture fixture;
        private Projectile prefab;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            prefab = fixture.CreateProjectilePrefab();
        }

        [TearDown]
        public void TearDown() => fixture.Dispose();

        private Damageable CreateCover()
        {
            var profile = fixture.Track(ScriptableObject.CreateInstance<DamageableProfile>());
            TankRig.SetField(profile, "hitPoints", 100f);
            TankRig.SetField(profile, "armor", new ArmorProfile(5f, 5f, 5f));

            var go = fixture.Track(new GameObject("Crate"));
            go.SetActive(false);
            go.transform.position = new Vector3(0f, 3f, 0f);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            go.AddComponent<LowCover>();
            var damageable = go.AddComponent<Damageable>();
            TankRig.SetField(damageable, "profile", profile);
            fixture.Inject(go);
            go.SetActive(true);
            return damageable;
        }

        private void Shoot(Vector2 firedFrom, float clearance)
        {
            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(prefab).Get(Vector2.zero, Quaternion.identity);
            projectile.Launch(new ProjectileShot
            {
                Velocity = new Vector2(0f, 10f),
                Lifetime = 5f,
                Damage = 30f,
                PenetrationMin = 100f,
                PenetrationMax = 100f,
                FiredFrom = firedFrom,
                CoverClearance = clearance,
            });
            projectile.Step(Step);
        }

        [Test]
        public void Shot_FiredCloseBehindCover_FliesOverIt()
        {
            var cover = CreateCover();

            Shoot(Vector2.zero, clearance: 5f);

            Assert.That(cover.HitPoints, Is.EqualTo(100f));
        }

        [Test]
        public void Shot_FiredFromBeyondClearance_HitsCover()
        {
            var cover = CreateCover();

            Shoot(new Vector2(0f, -10f), clearance: 5f);

            Assert.That(cover.HitPoints, Is.LessThan(100f));
        }

        [Test]
        public void Shot_WithoutClearance_HitsCover()
        {
            var cover = CreateCover();

            Shoot(Vector2.zero, clearance: 0f);

            Assert.That(cover.HitPoints, Is.LessThan(100f));
        }
    }
}
