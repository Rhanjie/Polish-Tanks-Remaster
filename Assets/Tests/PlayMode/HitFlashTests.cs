using System.Collections;
using NUnit.Framework;
using PolishTanks.Props;
using PolishTanks.Weapons;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class HitFlashTests
    {
        private static readonly Color BaseColor = Color.white;
        private static readonly Color FlashColor = new Color(1f, 0.2f, 0.2f);

        private WeaponFixture fixture;
        private SpriteRenderer sprite;
        private HitFlash flash;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            var go = fixture.Track(new GameObject("FlashingCrate"));
            go.SetActive(false);
            sprite = go.AddComponent<SpriteRenderer>();
            sprite.color = BaseColor;
            flash = go.AddComponent<HitFlash>();
            TankRig.SetField(flash, "flashColor", FlashColor);
            TankRig.SetField(flash, "duration", 0.05f);
            go.SetActive(true);
        }

        [TearDown]
        public void TearDown() => fixture.Dispose();

        private void Hit()
        {
            var hit = default(ProjectileHit);
            flash.OnProjectileHit(in hit);
        }

        [Test]
        public void OnProjectileHit_SetsFlashColor()
        {
            Hit();

            Assert.That(sprite.color, Is.EqualTo(FlashColor));
            Assert.That(flash.IsFlashing, Is.True);
        }

        [UnityTest]
        public IEnumerator Update_AfterDuration_RestoresBaseColor()
        {
            Hit();

            var frames = 0;
            while (flash.IsFlashing && frames++ < 600)
                yield return null;

            Assert.That(flash.IsFlashing, Is.False);
            Assert.That(sprite.color, Is.EqualTo(BaseColor));
        }

        [UnityTest]
        public IEnumerator Update_WithoutHit_KeepsBaseColor()
        {
            yield return null;
            yield return null;

            Assert.That(sprite.color, Is.EqualTo(BaseColor));
        }

        [UnityTest]
        public IEnumerator OnProjectileHit_SecondHitDuringFlash_RestartsFlash()
        {
            Hit();
            yield return null;

            Hit();

            Assert.That(sprite.color, Is.EqualTo(FlashColor));
        }

        [Test]
        public void OnProjectileHit_WithoutTargetSprite_DoesNotThrow()
        {
            var go = fixture.Track(new GameObject("NoSprite"));
            var bare = go.AddComponent<HitFlash>();
            var hit = default(ProjectileHit);

            Assert.DoesNotThrow(() => bare.OnProjectileHit(in hit));
        }
    }
}
