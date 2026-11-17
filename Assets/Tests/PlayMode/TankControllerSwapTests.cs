using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PolishTanks.Tank;
using PolishTanks.Tank.Modules;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class TankControllerSwapTests
    {
        private TankRig rig;

        [SetUp]
        public void SetUp() => rig = new TankRig();

        [TearDown]
        public void TearDown() => rig.Dispose();

        [Test]
        public void Fit_HeavierEngine_ChangesStats()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var before = tank.Controller.Stats.TotalMass;

            tank.Controller.Fit(rig.Create<EngineModule>(("mass", 3000f), ("horsePower", 600f)));

            Assert.That(tank.Controller.Stats.TotalMass, Is.GreaterThan(before));
        }

        [Test]
        public void Fit_NewModule_RaisesStatsApplied()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var received = new List<TankStats>();
            tank.Controller.StatsApplied += received.Add;

            tank.Controller.Fit(rig.Create<EngineModule>(("mass", 3000f), ("horsePower", 600f)));

            Assert.That(received, Has.Count.EqualTo(1));
        }

        [Test]
        public void Remove_RequiredSlot_ReturnsFalse()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);

            LogAssert.Expect(LogType.Error, new Regex("Engine slot is empty"));

            Assert.That(tank.Controller.Remove(ModuleSlot.Engine), Is.False);
        }

        [Test]
        public void Remove_RequiredSlot_KeepsPreviousLoadout()
        {
            var original = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", original, Vector2.zero);
            LogAssert.Expect(LogType.Error, new Regex("Engine slot is empty"));

            tank.Controller.Remove(ModuleSlot.Engine);

            Assert.That(tank.Controller.Loadout, Is.SameAs(original));
        }
    }
}
