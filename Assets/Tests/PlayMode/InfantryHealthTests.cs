using NUnit.Framework;
using PolishTanks.Infantry;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>A soldier built in code (body, collider, <see cref="InfantryHealth"/>) at the origin, wounded by hand-made hits and blasts.</summary>
    public class InfantryHealthTests
    {
        private WeaponFixture fixture;
        private InfantryHealth soldier;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            var go = fixture.Track(new GameObject("Soldier"));
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            go.AddComponent<CircleCollider2D>().radius = 0.3f;
            soldier = go.AddComponent<InfantryHealth>();
        }

        [TearDown]
        public void TearDown() => fixture.Dispose();

        private static ProjectileHit Bullet(float caliber) =>
            new ProjectileHit(Vector2.zero, Vector2.up, Vector2.down, 5f, 0f, null, 5f, 8f, caliber);

        [Test]
        public void OnProjectileHit_StrongRifleBullets_KillWithinTwoHits()
        {
            soldier.OnProjectileHit(Bullet(7.92f));
            if (!soldier.IsDead)
                soldier.OnProjectileHit(Bullet(7.92f));

            Assert.That(soldier.IsDead, Is.True);
        }

        [Test]
        public void OnProjectileHit_CannonRound_KillsAtOnce()
        {
            soldier.OnProjectileHit(Bullet(75f));

            Assert.That(soldier.IsDead, Is.True);
        }

        [Test]
        public void OnProjectileHit_AfterDeath_IsIgnored()
        {
            soldier.OnProjectileHit(Bullet(75f));

            Assert.That(soldier.OnProjectileHit(Bullet(75f)).Outcome, Is.EqualTo(HitOutcome.Ignored));
        }

        [Test]
        public void Detonate_AtTheSoldiersFeet_Kills()
        {
            Physics2D.SyncTransforms();
            var blast = new BlastSettings { radius = 2.5f, penetrationMin = 30f, penetrationMax = 45f, damage = 120f, caliber = 40f };

            Blast.Detonate(Vector2.zero, blast, null);

            Assert.That(soldier.IsDead, Is.True);
        }

        [Test]
        public void Kill_Soldier_DisablesItsColliders()
        {
            soldier.Kill(Vector2.up);

            Assert.That(soldier.GetComponent<Collider2D>().enabled, Is.False);
        }
    }
}
