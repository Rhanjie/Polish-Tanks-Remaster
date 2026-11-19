using NUnit.Framework;
using PolishTanks.Infantry;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Timing of <see cref="InfantryArms"/> with a random source fixed at 0.5, which makes every variation neutral.</summary>
    public class InfantryArmsTests
    {
        private GameObject grenadePrefab;

        [TearDown]
        public void TearDown()
        {
            if (grenadePrefab != null)
                Object.DestroyImmediate(grenadePrefab);
        }

        private RifleSettings Rifle() => new RifleSettings { burstShots = 3, shotInterval = 0.25f, burstPause = 4f };

        private GrenadeSettings Grenades(bool carried = true)
        {
            var settings = new GrenadeSettings { cooldown = 10f, windup = 0.5f };
            if (carried)
            {
                grenadePrefab = new GameObject("GrenadePrefab");
                settings.prefab = grenadePrefab.AddComponent<Grenade>();
            }
            return settings;
        }

        private InfantryArms Arms(bool carriesGrenades = true) => new InfantryArms(Rifle(), Grenades(carriesGrenades), () => 0.5f, 0f);

        private static int ShotsBetween(InfantryArms arms, float from, float to)
        {
            var shots = 0;
            for (var time = from; time < to; time += 0.25f)
                if (arms.TryShoot(time))
                    shots++;
            return shots;
        }

        [Test]
        public void TryShoot_RightAfterABurst_ShootsOnlyTheBurstCountThenPauses()
        {
            Assert.That(ShotsBetween(Arms(), 0f, 4.5f), Is.EqualTo(3));
        }

        [Test]
        public void TryShoot_AfterTheBurstPause_StartsANewBurst()
        {
            var arms = Arms();
            ShotsBetween(arms, 0f, 4.5f);

            Assert.That(ShotsBetween(arms, 4.5f, 6f), Is.EqualTo(3));
        }

        [Test]
        public void TryShoot_BetweenShotsOfABurst_IsRefused()
        {
            var arms = Arms();
            arms.TryShoot(0f);

            Assert.That(arms.TryShoot(0.1f), Is.False);
        }

        [Test]
        public void IsGrenadeReady_RightAfterAThrow_IsFalseUntilTheCooldownPasses()
        {
            var arms = Arms();
            arms.Throw(10f);

            Assert.That(arms.IsGrenadeReady(19.9f), Is.False);
            Assert.That(arms.IsGrenadeReady(20f), Is.True);
        }

        [Test]
        public void IsGrenadeReady_NoGrenadesCarried_IsFalse()
        {
            Assert.That(Arms(carriesGrenades: false).IsGrenadeReady(1000f), Is.False);
        }

        [Test]
        public void IsBusy_DuringTheThrowWindup_IsTrueAndThenFalse()
        {
            var arms = Arms();
            arms.Throw(10f);

            Assert.That(arms.IsBusy(10.25f), Is.True);
            Assert.That(arms.IsBusy(10.5f), Is.False);
        }

        [Test]
        public void IsBusy_RightAfterAShot_IsTrueForTheShotInterval()
        {
            var arms = Arms();
            arms.TryShoot(1f);

            Assert.That(arms.IsBusy(1.1f), Is.True);
        }
    }
}
