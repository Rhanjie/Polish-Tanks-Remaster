using System.Collections;
using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Damage;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Part damage on a tank wired in code: a shell into the side throws a track, a broken engine burns and stops the tank,
    /// a jammed turret ring keeps the turret still and a field repair puts the fire out.
    /// </summary>
    public class ModuleDamageTests
    {
        private TankRig rig;
        private WeaponFixture fixture;
        private Projectile prefab;
        private TankRig.BuiltTank tank;
        private ModuleDamage damage;

        [SetUp]
        public void SetUp()
        {
            rig = new TankRig();
            fixture = new WeaponFixture();
            prefab = fixture.CreateProjectilePrefab();
            BuildTank();
        }

        [TearDown]
        public void TearDown()
        {
            rig.Dispose();
            fixture.Dispose();
        }

        private void BuildTank()
        {
            var loadout = rig.CreateLoadout();
            TankRig.SetField(loadout.Tracks, "breakChance", 1f);
            TankRig.SetField(loadout.Turret, "ringJamChance", 0f);
            TankRig.SetField(loadout.Engine, "reliability", 1f);

            tank = rig.BuildTank("Tank", null, Vector2.zero);
            tank.Root.SetActive(false);
            damage = tank.Root.AddComponent<ModuleDamage>();
            TankRig.SetField(damage, "settings", rig.Create<ModuleDamageSettings>(
                ("fireTickInterval", 0.05f), ("fireTickDamage", 0.005f), ("fireDuration", 5f)));
            fixture.Inject(tank.Root);
            tank.Root.SetActive(true);
            tank.Controller.Apply(loadout);
        }

        private static IEnumerator WaitFixedUpdates(int count)
        {
            for (var i = 0; i < count; i++)
                yield return new WaitForFixedUpdate();
        }

        private void ShootLeftSide(float caliber = 75f)
        {
            Physics2D.SyncTransforms();
            var projectile = fixture.Pools.For(prefab).Get(new Vector2(-3f, 0f), Quaternion.identity);
            projectile.Launch(new ProjectileShot
            {
                Velocity = new Vector2(10f, 0f),
                Lifetime = 5f,
                Damage = 1f,
                PenetrationMin = 1000f,
                PenetrationMax = 1000f,
                Caliber = caliber,
            });
            projectile.Step(0.5f);
        }

        [UnityTest]
        public IEnumerator OnHit_PenetratingLeftSideHitWithCertainTrackBreak_TankNoLongerDrivesNormally()
        {
            ShootLeftSide();
            tank.Movement.SetInput(1f, 0f);

            yield return WaitFixedUpdates(50);

            Assert.That(tank.Movement.CurrentSpeed, Is.LessThan(tank.Movement.Settings.maxForwardSpeed * 0.2f));
        }

        [Test]
        public void OnHit_PenetratingLeftSideHitWithCertainTrackBreak_ThrowsLeftTrack()
        {
            ShootLeftSide();

            Assert.That(damage.Model.ConditionOf(new PartId(VehiclePart.LeftTrack)), Is.EqualTo(PartCondition.Destroyed));
        }

        [Test]
        public void OnHit_MachineGunBulletIntoSide_LeavesTrackIntact()
        {
            ShootLeftSide(7.92f);

            Assert.That(damage.Model.ConditionOf(new PartId(VehiclePart.LeftTrack)), Is.EqualTo(PartCondition.Intact));
        }

        [UnityTest]
        public IEnumerator Break_Engine_StartsFireThatCostsHitPoints()
        {
            var start = tank.Root.GetComponent<Damageable>().HitPoints;

            damage.Break(VehiclePart.Engine);
            var deadline = Time.time + 2f;
            while (tank.Root.GetComponent<Damageable>().HitPoints >= start && Time.time < deadline)
                yield return null;

            Assert.That(tank.Root.GetComponent<Damageable>().HitPoints, Is.LessThan(start));
        }

        [Test]
        public void Break_Engine_SetsTankOnFire()
        {
            damage.Break(VehiclePart.Engine);

            Assert.That(damage.IsBurning, Is.True);
        }

        [UnityTest]
        public IEnumerator Break_Engine_TankWillNotDrive()
        {
            damage.Break(VehiclePart.Engine);
            tank.Movement.SetInput(1f, 0f);

            yield return WaitFixedUpdates(50);

            Assert.That(tank.Movement.CurrentSpeed, Is.EqualTo(0f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator Break_TurretRing_TurretDoesNotRotateTowardAimPoint()
        {
            damage.Break(VehiclePart.TurretRing);
            tank.Aimer.SetAimPoint(new Vector2(-10f, 0f));

            for (var i = 0; i < 30; i++)
                yield return null;

            Assert.That(Mathf.DeltaAngle(0f, tank.Turret.localEulerAngles.z), Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void FieldRepair_BrokenEngine_ExtinguishesFire()
        {
            damage.Break(VehiclePart.Engine);

            damage.FieldRepair(new PartId(VehiclePart.Engine));

            Assert.That(damage.IsBurning, Is.False);
        }
    }
}
