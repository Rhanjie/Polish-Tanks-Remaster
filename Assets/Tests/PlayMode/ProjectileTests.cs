using NUnit.Framework;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Drives <see cref="Projectile.Step"/> by hand. [Test] methods run inside one frame,
    /// so the projectile's own Update never interferes.
    /// </summary>
    public class ProjectileTests
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

        private Projectile Fire(Vector2 from, Vector2 velocity, float lifetime = 5f, Rigidbody2D owner = null, float impulse = 0f, float damage = 0f)
        {
            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(prefab).Get(from, Quaternion.identity);
            projectile.Launch(new ProjectileShot { Velocity = velocity, Lifetime = lifetime, Shooter = owner, Impulse = impulse, Damage = damage });
            return projectile;
        }

        [Test]
        public void Step_IntoStaticCollider_ReturnsProjectileToPool()
        {
            fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));

            projectile.Step(Step);

            Assert.That(projectile.IsSpawned, Is.False);
            Assert.That(projectile.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void Step_IntoStaticCollider_StopsAtSurface()
        {
            fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));

            projectile.Step(Step);

            Assert.That(projectile.transform.position.x, Is.EqualTo(1.5f - projectile.Radius).Within(0.02f));
        }

        [Test]
        public void Step_ShortOfTheCollider_KeepsFlying()
        {
            fixture.CreateStaticBox(new Vector2(20f, 0f), Vector2.one);
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));

            projectile.Step(Step);

            Assert.That(projectile.IsSpawned, Is.True);
            Assert.That(projectile.transform.position.x, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void Step_IntoDynamicBody_PushesItAlongTheShot()
        {
            var body = fixture.CreateDynamicBox(new Vector2(2f, 0f), Vector2.one);
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f), impulse: 2f);

            projectile.Step(Step);

            Assert.That(body.linearVelocity.x, Is.GreaterThan(0f));
        }

        [Test]
        public void Step_IntoDynamicBodyWithZeroImpulse_LeavesItAtRest()
        {
            var body = fixture.CreateDynamicBox(new Vector2(2f, 0f), Vector2.one);
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f), impulse: 0f);

            projectile.Step(Step);

            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Step_InsideShootersCollider_PassesThroughShooter()
        {
            var shooter = fixture.CreateDynamicBox(Vector2.zero, new Vector2(2f, 2f));
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f), owner: shooter, impulse: 2f);

            projectile.Step(0.1f);

            Assert.That(projectile.IsSpawned, Is.True);
            Assert.That(shooter.linearVelocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Step_InsideColliderWithoutOwner_HitsIt()
        {
            fixture.CreateDynamicBox(Vector2.zero, new Vector2(2f, 2f));
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));

            projectile.Step(0.1f);

            Assert.That(projectile.IsSpawned, Is.False);
        }

        [Test]
        public void Step_ThroughShooterIntoOtherBody_HitsTheOtherBody()
        {
            var shooter = fixture.CreateDynamicBox(Vector2.zero, new Vector2(2f, 2f));
            var target = fixture.CreateDynamicBox(new Vector2(4f, 0f), Vector2.one);
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f), owner: shooter, impulse: 2f);

            projectile.Step(Step);

            Assert.That(target.linearVelocity.x, Is.GreaterThan(0f));
        }

        [Test]
        public void Step_PastLifetime_ReturnsProjectileToPool()
        {
            var projectile = Fire(Vector2.zero, new Vector2(1f, 0f), lifetime: 0.5f);

            projectile.Step(0.3f);
            projectile.Step(0.3f);

            Assert.That(projectile.IsSpawned, Is.False);
        }

        [Test]
        public void Step_BeforeLifetimeEnds_KeepsProjectileAlive()
        {
            var projectile = Fire(Vector2.zero, new Vector2(1f, 0f), lifetime: 0.5f);

            projectile.Step(0.3f);

            Assert.That(projectile.IsSpawned, Is.True);
        }

        [Test]
        public void Step_WithZeroVelocity_ReturnsProjectileToPool()
        {
            var projectile = Fire(Vector2.zero, Vector2.zero);

            projectile.Step(Step);

            Assert.That(projectile.IsSpawned, Is.False);
        }

        [Test]
        public void Step_WithZeroOrNegativeDelta_DoesNotMoveProjectile()
        {
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));

            projectile.Step(0f);
            projectile.Step(-1f);

            Assert.That(projectile.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Step_OnProjectileAlreadyReturned_DoesNothing()
        {
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));
            projectile.ReturnToPool();

            projectile.Step(Step);

            Assert.That(projectile.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Step_IntoTrigger_IgnoresIt()
        {
            fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one, trigger: true);
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));

            projectile.Step(Step);

            Assert.That(projectile.IsSpawned, Is.True);
            Assert.That(projectile.transform.position.x, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void Step_IntoHitHandler_CallsItOnce()
        {
            var box = fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            var handler = box.gameObject.AddComponent<RecordingHitHandler>();
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));

            projectile.Step(Step);

            Assert.That(handler.HitCount, Is.EqualTo(1));
        }

        [Test]
        public void Step_IntoHitHandler_PassesDamageAndDirection()
        {
            var box = fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            var handler = box.gameObject.AddComponent<RecordingHitHandler>();
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f), damage: 42f);

            projectile.Step(Step);

            Assert.That(handler.LastHit.Damage, Is.EqualTo(42f));
            Assert.That(handler.LastHit.Direction.x, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void Step_HandlerOnParentOfCollider_StillReceivesHit()
        {
            var parent = fixture.Track(new GameObject("Parent"));
            var handler = parent.AddComponent<RecordingHitHandler>();
            var box = fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            box.transform.SetParent(parent.transform, true);
            var projectile = Fire(Vector2.zero, new Vector2(10f, 0f));

            projectile.Step(Step);

            Assert.That(handler.HitCount, Is.EqualTo(1));
        }

        [Test]
        public void ReturnToPool_ClearsShooterReference()
        {
            var shooter = fixture.CreateDynamicBox(Vector2.zero, Vector2.one);
            var projectile = Fire(new Vector2(5f, 5f), new Vector2(1f, 0f), owner: shooter);
            projectile.ReturnToPool();

            Assert.That(projectile.Shooter == null, Is.True);
        }
    }
}
