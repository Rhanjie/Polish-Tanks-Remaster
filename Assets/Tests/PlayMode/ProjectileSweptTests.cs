using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Steps a launched <see cref="Projectile"/> by hand and checks what <see cref="Projectile.Swept"/> reports.</summary>
    public class ProjectileSweptTests
    {
        private struct Sweep
        {
            public Rigidbody2D Shooter;
            public Vector2 From;
            public Vector2 To;
        }

        private readonly List<Sweep> sweeps = new List<Sweep>();
        private WeaponFixture fixture;
        private Projectile prefab;

        [SetUp]
        public void SetUp()
        {
            sweeps.Clear();
            fixture = new WeaponFixture();
            prefab = fixture.CreateProjectilePrefab();
            Projectile.Swept += Record;
        }

        [TearDown]
        public void TearDown()
        {
            Projectile.Swept -= Record;
            fixture.Dispose();
        }

        private void Record(Rigidbody2D shooter, Vector2 from, Vector2 to) =>
            sweeps.Add(new Sweep { Shooter = shooter, From = from, To = to });

        private void FireAndStep(Rigidbody2D shooter)
        {
            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(prefab).Get(Vector2.zero, Quaternion.identity);
            projectile.Launch(new ProjectileShot { Velocity = new Vector2(10f, 0f), Lifetime = 5f, Shooter = shooter, Caliber = 75f });
            projectile.Step(0.5f);
        }

        [Test]
        public void Step_LaunchedProjectile_ReportsTheShooterBody()
        {
            var shooter = fixture.CreateDynamicBox(new Vector2(0f, -8f), Vector2.one);

            FireAndStep(shooter);

            Assert.That(sweeps.Count, Is.EqualTo(1));
            Assert.That(sweeps[0].Shooter, Is.SameAs(shooter));
        }

        [Test]
        public void Step_LaunchedProjectile_ReportsTheStretchItFlew()
        {
            var shooter = fixture.CreateDynamicBox(new Vector2(0f, -8f), Vector2.one);

            FireAndStep(shooter);

            Assert.That(sweeps[0].From, Is.EqualTo(Vector2.zero));
            Assert.That((sweeps[0].To - sweeps[0].From).magnitude, Is.EqualTo(5f).Within(1e-3f));
        }

        [Test]
        public void Step_ProjectileThatHitsSomething_StillReportsTheShooter()
        {
            fixture.CreateStaticBox(new Vector2(2f, 0f), Vector2.one);
            var shooter = fixture.CreateDynamicBox(new Vector2(0f, -8f), Vector2.one);

            FireAndStep(shooter);

            Assert.That(sweeps[0].Shooter, Is.SameAs(shooter));
        }
    }
}
