using NUnit.Framework;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Drives <see cref="Projectile.Step"/> by hand and checks that the shooter's receiver hears about the impact.</summary>
    public class ProjectileShotOutcomeTests
    {
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

        private Projectile Fire(Rigidbody2D shooter)
        {
            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(prefab).Get(Vector2.zero, Quaternion.identity);
            projectile.Launch(new ProjectileShot { Velocity = new Vector2(10f, 0f), Lifetime = 5f, Shooter = shooter, Caliber = 75f });
            return projectile;
        }

        [Test]
        public void Step_ImpactOnTarget_ReportsTheHitedColliderToTheShootersReceiver()
        {
            var box = fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            var shooter = fixture.CreateDynamicBox(new Vector2(0f, -8f), Vector2.one);
            var receiver = shooter.gameObject.AddComponent<RecordingShotOutcomeReceiver>();

            Fire(shooter).Step(0.5f);

            Assert.That(receiver.Outcomes.ConvertAll(o => o.Target), Is.EqualTo(new Collider2D[] { box }));
        }

        [Test]
        public void Step_ShooterWithoutReceiver_StillHitsTheTarget()
        {
            fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            var shooter = fixture.CreateDynamicBox(new Vector2(0f, -8f), Vector2.one);

            var projectile = Fire(shooter);
            projectile.Step(0.5f);

            Assert.That(projectile.IsSpawned, Is.False);
        }
    }
}
