using System.Collections;
using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Weapons;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Fires projectiles by hand into <see cref="Damageable"/> targets. Shots travel along +Y into the bottom face,
    /// so an unrotated target (facing +Y) is hit on its rear plate and a target turned by 180 degrees on its front plate.
    /// </summary>
    public class DamageableTests
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

        private Damageable CreateTarget(float hitPoints, ArmorProfile armor, bool removeOnDeath = true, float zRotation = 0f,
            float y = 3f)
        {
            var profile = fixture.Track(ScriptableObject.CreateInstance<DamageableProfile>());
            TankRig.SetField(profile, "hitPoints", hitPoints);
            TankRig.SetField(profile, "armor", armor);
            TankRig.SetField(profile, "removeOnDeath", removeOnDeath);

            var go = fixture.Track(new GameObject("Target"));
            go.SetActive(false);
            go.transform.position = new Vector3(0f, y, 0f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            var damageable = go.AddComponent<Damageable>();
            TankRig.SetField(damageable, "profile", profile);
            fixture.Inject(go);
            go.SetActive(true);
            return damageable;
        }

        private Projectile Shoot(float damage, float penetration)
        {
            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(prefab).Get(Vector2.zero, Quaternion.identity);
            projectile.Launch(new ProjectileShot
            {
                Velocity = new Vector2(0f, 10f),
                Lifetime = 5f,
                Damage = damage,
                PenetrationMin = penetration,
                PenetrationMax = penetration,
            });
            projectile.Step(Step);
            return projectile;
        }

        [Test]
        public void OnProjectileHit_PenetrationAboveArmor_TakesHitPointsEqualToDamage()
        {
            var target = CreateTarget(100f, new ArmorProfile(10f, 10f, 10f));

            Shoot(damage: 30f, penetration: 100f);

            Assert.That(target.HitPoints, Is.EqualTo(70f).Within(1e-4f));
        }

        [Test]
        public void OnProjectileHit_PenetrationBelowArmor_LeavesHitPointsUntouched()
        {
            var target = CreateTarget(100f, new ArmorProfile(50f, 50f, 50f));

            Shoot(damage: 30f, penetration: 5f);

            Assert.That(target.HitPoints, Is.EqualTo(100f));
        }

        [Test]
        public void OnProjectileHit_ShotIntoTurnedAroundTarget_HitsFrontPlate()
        {
            var target = CreateTarget(100f, new ArmorProfile(10f, 10f, 10f), zRotation: 180f);

            Shoot(damage: 1f, penetration: 100f);

            Assert.That(target.LastResult.Side, Is.EqualTo(HitSide.Front));
        }

        [Test]
        public void OnProjectileHit_ShotFromBehindUnrotatedTarget_HitsRearPlate()
        {
            var target = CreateTarget(100f, new ArmorProfile(10f, 10f, 10f));

            Shoot(damage: 1f, penetration: 100f);

            Assert.That(target.LastResult.Side, Is.EqualTo(HitSide.Rear));
        }

        [Test]
        public void Shot_WithTwiceTheArmor_PassesThroughIntoTheTargetBehind()
        {
            var first = CreateTarget(100f, new ArmorProfile(10f, 10f, 10f), removeOnDeath: false);
            var second = CreateTarget(100f, new ArmorProfile(25f, 25f, 25f), removeOnDeath: false, y: 6f);

            var projectile = Shoot(damage: 30f, penetration: 50f);
            projectile.Step(Step);

            Assert.That(first.HitPoints, Is.EqualTo(70f).Within(1e-4f));
            Assert.That(second.LastResult.Penetration, Is.EqualTo(30f).Within(1e-4f));
            Assert.That(second.HitPoints, Is.EqualTo(70f).Within(1e-4f));
        }

        [Test]
        public void Shot_WithLessThanTwiceTheArmor_StopsInTheFirstTarget()
        {
            var first = CreateTarget(100f, new ArmorProfile(10f, 10f, 10f), removeOnDeath: false);
            var second = CreateTarget(100f, new ArmorProfile(1f, 1f, 1f), removeOnDeath: false, y: 6f);

            var projectile = Shoot(damage: 30f, penetration: 19f);
            projectile.Step(Step);

            Assert.That(first.HitPoints, Is.EqualTo(70f).Within(1e-4f));
            Assert.That(projectile.IsSpawned, Is.False);
            Assert.That(second.HitPoints, Is.EqualTo(100f));
        }

        [Test]
        public void OnProjectileHit_OnDeadTarget_IsIgnored()
        {
            var target = CreateTarget(50f, new ArmorProfile(10f, 10f, 10f), removeOnDeath: false);
            target.ApplyDamage(50f);
            var hit = new ProjectileHit(Vector2.zero, Vector2.up, Vector2.down, 10f, 0f, null, 100f, 100f);

            Assert.That(target.OnProjectileHit(hit).Outcome, Is.EqualTo(HitOutcome.Ignored));
        }

        [UnityTest]
        public IEnumerator ApplyDamage_KillingTargetWithRemoveOnDeath_DestroysGameObject()
        {
            var target = CreateTarget(50f, new ArmorProfile(10f, 10f, 10f), removeOnDeath: true);
            var go = target.gameObject;

            target.ApplyDamage(50f);
            yield return null;

            Assert.That(go == null, Is.True);
        }

        [UnityTest]
        public IEnumerator ApplyDamage_KillingTargetWithoutRemoveOnDeath_KeepsGameObject()
        {
            var target = CreateTarget(50f, new ArmorProfile(10f, 10f, 10f), removeOnDeath: false);
            var go = target.gameObject;

            target.ApplyDamage(50f);
            yield return null;

            Assert.That(go == null, Is.False);
        }
    }
}
