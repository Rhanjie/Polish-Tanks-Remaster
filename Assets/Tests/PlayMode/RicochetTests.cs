using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Fires projectiles by hand at shallow angles (about 80 degrees from the plate normal, well past the 70 degree
    /// certain-ricochet threshold) so the random roll never decides the outcome.
    /// </summary>
    public class RicochetTests
    {
        private static readonly Vector2 GlancingDirection = new Vector2(Mathf.Cos(80f * Mathf.Deg2Rad), Mathf.Sin(80f * Mathf.Deg2Rad));
        private static readonly Vector2 GlancingDirectionAtCeiling = new Vector2(Mathf.Sin(80f * Mathf.Deg2Rad), Mathf.Cos(80f * Mathf.Deg2Rad));

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

        private BoxCollider2D CreateWall() => fixture.CreateStaticBox(new Vector2(6f, 0f), new Vector2(1f, 10f));

        private Projectile Fire(Vector2 from, Vector2 direction, float caliber = 0f, float penetration = 100f, bool ricocheted = false)
        {
            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(prefab).Get(from, Quaternion.identity);
            projectile.Launch(new ProjectileShot
            {
                Velocity = direction * 10f,
                Lifetime = 5f,
                Damage = 10f,
                PenetrationMin = penetration,
                PenetrationMax = penetration,
                Caliber = caliber,
                Ricocheted = ricocheted,
            });
            return projectile;
        }

        private Damageable CreatePlate(Vector2 position, float armor, float hitPoints = 100f)
        {
            var profile = fixture.Track(ScriptableObject.CreateInstance<DamageableProfile>());
            TankRig.SetField(profile, "hitPoints", hitPoints);
            TankRig.SetField(profile, "armor", new ArmorProfile(armor, armor, armor));

            var go = fixture.Track(new GameObject("Plate"));
            go.SetActive(false);
            go.transform.position = position;
            go.AddComponent<BoxCollider2D>().size = new Vector2(40f, 1f);
            var damageable = go.AddComponent<Damageable>();
            TankRig.SetField(damageable, "profile", profile);
            fixture.Inject(go);
            go.SetActive(true);
            return damageable;
        }

        private Projectile RicochetOffWallTowardPlate()
        {
            CreateWall();
            var projectile = Fire(new Vector2(5f, 0f), GlancingDirection, penetration: 100f);
            projectile.Step(0.5f);
            return projectile;
        }

        [Test]
        public void Step_GlancingHitOnPlainWall_KeepsFlyingWithReducedSpeed()
        {
            CreateWall();
            var projectile = Fire(new Vector2(5f, 0f), GlancingDirection);

            projectile.Step(0.5f);

            Assert.That(projectile.IsSpawned, Is.True);
            Assert.That(projectile.HasRicocheted, Is.True);
            Assert.That(projectile.Velocity.magnitude, Is.EqualTo(6f).Within(1e-3f));
        }

        [Test]
        public void Step_GlancingHitOnPlainWall_BouncesAwayFromWall()
        {
            CreateWall();
            var projectile = Fire(new Vector2(5f, 0f), GlancingDirection);

            projectile.Step(0.5f);

            Assert.That(projectile.Velocity.x, Is.LessThan(0f));
        }

        [Test]
        public void Step_RicochetedShotHitsArmor_DoesNotPenetrate()
        {
            var plate = CreatePlate(new Vector2(0f, 8f), armor: 50f);
            var projectile = RicochetOffWallTowardPlate();

            projectile.Step(1f);

            Assert.That(plate.HitPoints, Is.EqualTo(100f));
        }

        [Test]
        public void Step_RicochetedShotHitsArmor_ReturnsToPool()
        {
            CreatePlate(new Vector2(0f, 8f), armor: 50f);
            var projectile = RicochetOffWallTowardPlate();

            projectile.Step(1f);

            Assert.That(projectile.IsSpawned, Is.False);
        }

        [Test]
        public void Step_RicochetedShotHitsPlateAtSteepAngle_DoesNotBounceAgain()
        {
            var plate = CreatePlate(new Vector2(0f, 3f), armor: 50f);
            var projectile = Fire(Vector2.zero, GlancingDirectionAtCeiling, caliber: 10f, ricocheted: true);

            projectile.Step(2f);

            Assert.That(plate.LastOutcome, Is.Not.EqualTo(HitOutcome.Ricochet));
            Assert.That(projectile.IsSpawned, Is.False);
        }

        [Test]
        public void OnProjectileHit_SteepHitOnThinPlateAgainstLargeCaliber_DoesNotRicochet()
        {
            var plate = CreatePlate(new Vector2(0f, 3f), armor: 10f);
            var projectile = Fire(Vector2.zero, GlancingDirectionAtCeiling, caliber: 100f, penetration: 5f);

            projectile.Step(2f);

            Assert.That(plate.LastOutcome, Is.Not.EqualTo(HitOutcome.Ricochet));
        }

        [Test]
        public void OnProjectileHit_SteepHitOnThickPlateWithSmallCaliber_Ricochets()
        {
            var plate = CreatePlate(new Vector2(0f, 3f), armor: 50f);
            var projectile = Fire(Vector2.zero, GlancingDirectionAtCeiling, caliber: 10f);

            projectile.Step(2f);

            Assert.That(plate.LastOutcome, Is.EqualTo(HitOutcome.Ricochet));
        }

        [Test]
        public void OnProjectileHit_Ricochet_CostsNoHitPoints()
        {
            var plate = CreatePlate(new Vector2(0f, 3f), armor: 50f);
            var projectile = Fire(Vector2.zero, GlancingDirectionAtCeiling, caliber: 10f);

            projectile.Step(2f);

            Assert.That(plate.HitPoints, Is.EqualTo(100f));
        }
    }
}
