using System.Collections;
using NUnit.Framework;
using PolishTanks.Damage;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>A tank leaving tread marks has its left track thrown by a shell into the side.</summary>
    public class TrackMarksTests
    {
        private TankRig rig;
        private WeaponFixture fixture;

        [SetUp]
        public void SetUp()
        {
            rig = new TankRig();
            fixture = new WeaponFixture();
        }

        [TearDown]
        public void TearDown()
        {
            rig.Dispose();
            fixture.Dispose();
        }

        [UnityTest]
        public IEnumerator ThrownTrack_StopsMarkingOnlyItsOwnSide()
        {
            var loadout = rig.CreateLoadout();
            TankRig.SetField(loadout.Tracks, "breakChance", 1f);
            TankRig.SetField(loadout.Turret, "ringJamChance", 0f);
            TankRig.SetField(loadout.Engine, "reliability", 1f);
            var tank = rig.BuildTank("Tank", null, Vector2.zero);
            tank.Root.SetActive(false);
            var damage = tank.Root.AddComponent<ModuleDamage>();
            TankRig.SetField(damage, "settings", rig.Create<ModuleDamageSettings>());
            var marks = tank.Root.AddComponent<TrackMarks>();
            fixture.Inject(tank.Root);
            tank.Root.SetActive(true);
            tank.Controller.Apply(loadout);
            yield return null;

            Assert.That(marks.Left.emitting && marks.Right.emitting, Is.True);

            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(fixture.CreateProjectilePrefab()).Get(new Vector2(-3f, 0f), Quaternion.identity);
            projectile.Launch(new ProjectileShot
            {
                Velocity = new Vector2(10f, 0f),
                Lifetime = 5f,
                Damage = 1f,
                PenetrationMin = 1000f,
                PenetrationMax = 1000f,
                Caliber = 75f,
            });
            projectile.Step(0.5f);
            yield return null;

            Assert.That(damage.Model.ConditionOf(new PartId(VehiclePart.LeftTrack)), Is.EqualTo(PartCondition.Destroyed));
            Assert.That(marks.Left.emitting, Is.False);
            Assert.That(marks.Right.emitting, Is.True);
        }
    }
}
