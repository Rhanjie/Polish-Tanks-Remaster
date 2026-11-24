using NUnit.Framework;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>A shot that punches through metal shows the penetration effect; through anything else, the plain impact.</summary>
    public class ProjectilePenetrationEffectTests
    {
        private WeaponFixture fixture;
        private PrefabPool<PooledEffect> impactPool;
        private PrefabPool<PooledEffect> penetratedPool;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            impactPool = fixture.Pools.For(fixture.CreatePrefab<PooledEffect>("Impact"));
            penetratedPool = fixture.Pools.For(fixture.CreatePrefab<PooledEffect>("Penetrated"));
        }

        [TearDown]
        public void TearDown() => fixture.Dispose();

        private void FireInto(SurfaceMaterial material)
        {
            var target = fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            var handler = target.gameObject.AddComponent<RecordingHitHandler>();
            handler.Outcome = HitOutcome.Penetrated;
            handler.Material = material;

            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(fixture.CreateProjectilePrefab()).Get(Vector2.zero, Quaternion.identity);
            projectile.Launch(new ProjectileShot
            {
                Velocity = new Vector2(10f, 0f), Lifetime = 5f, ImpactPool = impactPool, PenetratedPool = penetratedPool,
            });
            projectile.Step(0.5f);
        }

        [Test]
        public void Step_PenetratingMetal_ShowsPenetrationEffect()
        {
            FireInto(SurfaceMaterial.Metal);

            Assert.That(penetratedPool.CountActive, Is.EqualTo(1));
            Assert.That(impactPool.CountActive, Is.Zero);
        }

        [Test]
        public void Step_PenetratingOtherSurface_ShowsImpactEffect()
        {
            FireInto(SurfaceMaterial.Other);

            Assert.That(impactPool.CountActive, Is.EqualTo(1));
            Assert.That(penetratedPool.CountActive, Is.Zero);
        }
    }
}
