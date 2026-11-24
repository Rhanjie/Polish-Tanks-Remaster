using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Tank.Modules;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Tank health and armor wiring: stats flow into <see cref="Damageable"/> and a kill turns the tank into a wreck.</summary>
    public class TankDestructionTests
    {
        private TankRig rig;

        [SetUp]
        public void SetUp() => rig = new TankRig();

        [TearDown]
        public void TearDown() => rig.Dispose();

        [Test]
        public void Apply_ValidLoadout_SetsMaxHitPointsFromStats()
        {
            var loadout = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            tank.Controller.Apply(loadout);

            var damageable = tank.Root.GetComponent<Damageable>();
            Assert.That(damageable.MaxHitPoints, Is.EqualTo(TankStatsCalculator.Calculate(loadout).HitPoints).Within(1e-4f));
        }

        [Test]
        public void Apply_ValidLoadout_SetsArmorFromStats()
        {
            var loadout = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            tank.Controller.Apply(loadout);

            var stats = TankStatsCalculator.Calculate(loadout);
            var damageable = tank.Root.GetComponent<Damageable>();
            Assert.That(damageable.Armor, Is.EqualTo(new ArmorProfile(stats.FrontArmor, stats.SideArmor, stats.RearArmor)));
        }

        [Test]
        public void ApplyDamage_BeyondHitPoints_MarksTankDestroyed()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var damageable = tank.Root.GetComponent<Damageable>();

            damageable.ApplyDamage(damageable.MaxHitPoints + 1f);

            Assert.That(tank.Controller.IsDestroyed, Is.True);
        }

        [Test]
        public void ApplyDamage_BeyondHitPoints_DisablesMovement()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var damageable = tank.Root.GetComponent<Damageable>();

            damageable.ApplyDamage(damageable.MaxHitPoints + 1f);

            Assert.That(tank.Movement.enabled, Is.False);
        }

        [Test]
        public void ApplyDamage_BeyondHitPoints_DisablesGun()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var damageable = tank.Root.GetComponent<Damageable>();

            damageable.ApplyDamage(damageable.MaxHitPoints + 1f);

            Assert.That(tank.Gun.enabled, Is.False);
        }

        [Test]
        public void ApplyDamage_BelowHitPoints_KeepsTankAlive()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var damageable = tank.Root.GetComponent<Damageable>();

            damageable.ApplyDamage(damageable.MaxHitPoints - 1f);

            Assert.That(tank.Controller.IsDestroyed, Is.False);
        }
    }
}
