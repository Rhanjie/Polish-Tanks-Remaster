using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Detonates blasts next to a <see cref="Damageable"/> at the origin facing +Y: a blast below it tests the rear plate,
    /// one above it the front plate.
    /// </summary>
    public class BlastTests
    {
        private WeaponFixture fixture;
        private Damageable target;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            var go = fixture.Track(new GameObject("Target"));
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            target = go.AddComponent<Damageable>();
            target.Configure(100f, new ArmorProfile(front: 500f, side: 500f, rear: 10f));
        }

        [TearDown]
        public void TearDown() => fixture.Dispose();

        private static BlastSettings Settings() => new BlastSettings
        {
            radius = 3f,
            penetrationMin = 100f,
            penetrationMax = 100f,
            damage = 30f,
            caliber = 40f,
        };

        private void Detonate(Vector2 centre)
        {
            Physics2D.SyncTransforms();
            Blast.Detonate(centre, Settings(), null);
        }

        [Test]
        public void Detonate_BehindTargetWithThinRearArmor_Penetrates()
        {
            Detonate(new Vector2(0f, -2f));

            Assert.That(target.HitPoints, Is.EqualTo(70f).Within(1e-4f));
        }

        [Test]
        public void Detonate_InFrontOfThickFrontPlate_DoesNotPenetrate()
        {
            Detonate(new Vector2(0f, 2f));

            Assert.That(target.HitPoints, Is.EqualTo(100f));
        }

        [Test]
        public void Detonate_BeyondTheRadius_LeavesTheTargetAlone()
        {
            Detonate(new Vector2(0f, -4f));

            Assert.That(target.HitPoints, Is.EqualTo(100f));
        }
    }
}
