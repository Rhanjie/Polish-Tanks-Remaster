using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PolishTanks.Tank;
using PolishTanks.Tank.Modules;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class TankControllerTests
    {
        private TankRig rig;

        [SetUp]
        public void SetUp() => rig = new TankRig();

        [TearDown]
        public void TearDown() => rig.Dispose();

        private static IEnumerator WaitFixedUpdates(int count)
        {
            for (var i = 0; i < count; i++)
                yield return new WaitForFixedUpdate();
        }

        [Test]
        public void Awake_WithLoadoutAssigned_AppliesStats()
        {
            var loadout = rig.CreateLoadout();

            var tank = rig.BuildTank("Tank", loadout, Vector2.zero);

            Assert.That(tank.Controller.Stats, Is.Not.Null);
        }

        [Test]
        public void Awake_WithoutLoadout_LeavesStatsEmpty()
        {
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            Assert.That(tank.Controller.Stats, Is.Null);
        }

        [Test]
        public void Apply_ValidLoadout_ReturnsTrue()
        {
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            Assert.That(tank.Controller.Apply(rig.CreateLoadout()), Is.True);
        }

        [Test]
        public void Apply_ValidLoadout_SetsMovementTopSpeedFromCalculator()
        {
            var loadout = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            tank.Controller.Apply(loadout);

            var expected = TankStatsCalculator.Calculate(loadout).Drive.maxForwardSpeed;
            Assert.That(tank.Movement.Settings.maxForwardSpeed, Is.EqualTo(expected).Within(1e-4f));
        }

        [Test]
        public void Apply_ValidLoadout_SetsBodyMassInTonnes()
        {
            var loadout = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            tank.Controller.Apply(loadout);

            Assert.That(tank.Body.mass, Is.EqualTo(TankStatsCalculator.Calculate(loadout).TotalMass / 1000f).Within(1e-4f));
        }

        [Test]
        public void Apply_ValidLoadout_SetsGunReloadTimeFromMainGun()
        {
            var loadout = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            tank.Controller.Apply(loadout);

            Assert.That(tank.Gun.Settings.reloadTime, Is.EqualTo(2f).Within(1e-4f));
        }

        [Test]
        public void Apply_ValidLoadout_PlacesMuzzleAtTurretMuzzleDistance()
        {
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            tank.Controller.Apply(rig.CreateLoadout(muzzleDistance: 6.5f));

            Assert.That((Vector2)tank.Muzzle.localPosition, Is.EqualTo(new Vector2(0f, 6.5f)));
        }

        [Test]
        public void Apply_ValidLoadout_UpdatesLoadoutProperty()
        {
            var loadout = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            tank.Controller.Apply(loadout);

            Assert.That(tank.Controller.Loadout, Is.SameAs(loadout));
        }

        [Test]
        public void Apply_ValidLoadout_RaisesStatsAppliedWithCalculatedStats()
        {
            var loadout = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", null, Vector2.zero);
            TankStats received = null;
            tank.Controller.StatsApplied += stats => received = stats;

            tank.Controller.Apply(loadout);

            Assert.That(received, Is.Not.Null);
            Assert.That(received.TotalMass, Is.EqualTo(20000f));
        }

        [Test]
        public void Apply_ValidLoadout_RaisesStatsAppliedOnce()
        {
            var tank = rig.BuildTank("Tank", null, Vector2.zero);
            var calls = 0;
            tank.Controller.StatsApplied += _ => calls++;

            tank.Controller.Apply(rig.CreateLoadout());

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Apply_EmptyRequiredSlot_ReturnsFalse()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var broken = rig.CreateLoadout();
            TankRig.SetField(broken, "engine", null);
            LogAssert.Expect(LogType.Error, new Regex("can't apply loadout"));

            Assert.That(tank.Controller.Apply(broken), Is.False);
        }

        [Test]
        public void Apply_EmptyRequiredSlot_LogsErrorNamingTheSlot()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var broken = rig.CreateLoadout();
            TankRig.SetField(broken, "engine", null);
            LogAssert.Expect(LogType.Error, new Regex("Engine slot is empty"));

            tank.Controller.Apply(broken);
        }

        [Test]
        public void Apply_EmptyRequiredSlot_KeepsPreviousMovementSettings()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(300f), Vector2.zero);
            var before = tank.Movement.Settings.maxForwardSpeed;
            var broken = rig.CreateLoadout(900f);
            TankRig.SetField(broken, "hull", null);
            LogAssert.Expect(LogType.Error, new Regex("can't apply loadout"));

            tank.Controller.Apply(broken);

            Assert.That(tank.Movement.Settings.maxForwardSpeed, Is.EqualTo(before));
        }

        [Test]
        public void Apply_EmptyRequiredSlot_KeepsPreviousLoadout()
        {
            var original = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", original, Vector2.zero);
            var broken = rig.CreateLoadout();
            TankRig.SetField(broken, "mainGun", null);
            LogAssert.Expect(LogType.Error, new Regex("can't apply loadout"));

            tank.Controller.Apply(broken);

            Assert.That(tank.Controller.Loadout, Is.SameAs(original));
        }

        [Test]
        public void Apply_EmptyRequiredSlot_DoesNotRaiseStatsApplied()
        {
            var tank = rig.BuildTank("Tank", null, Vector2.zero);
            var calls = 0;
            tank.Controller.StatsApplied += _ => calls++;
            var broken = rig.CreateLoadout();
            TankRig.SetField(broken, "tracks", null);
            LogAssert.Expect(LogType.Error, new Regex("can't apply loadout"));

            tank.Controller.Apply(broken);

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void Apply_Null_ReturnsFalseAndLogsError()
        {
            var tank = rig.BuildTank("Tank", null, Vector2.zero);
            LogAssert.Expect(LogType.Error, new Regex("Loadout is missing"));

            Assert.That(tank.Controller.Apply(null), Is.False);
        }

        [Test]
        public void Apply_SameLoadoutTwice_KeepsSameStatsValues()
        {
            var loadout = rig.CreateLoadout();
            var tank = rig.BuildTank("Tank", null, Vector2.zero);

            tank.Controller.Apply(loadout);
            var first = tank.Movement.Settings.maxForwardSpeed;
            tank.Controller.Apply(loadout);

            Assert.That(tank.Movement.Settings.maxForwardSpeed, Is.EqualTo(first));
        }

        [Test]
        public void Apply_HeavierLoadoutAfterLighter_UpdatesBodyMass()
        {
            var tank = rig.BuildTank("Tank", rig.CreateLoadout(), Vector2.zero);
            var heavy = rig.CreateLoadout();
            TankRig.SetField(heavy.Hull, "mass", 30000f);

            tank.Controller.Apply(heavy);

            Assert.That(tank.Body.mass, Is.EqualTo(36f).Within(1e-4f));
        }

        [UnityTest]
        public IEnumerator Apply_StrongerEngine_DrivesFartherInSameTime()
        {
            // Far apart in X so the two hulls never touch.
            var weak = rig.BuildTank("Weak", rig.CreateLoadout(150f), new Vector2(-100f, 0f));
            var strong = rig.BuildTank("Strong", rig.CreateLoadout(300f), new Vector2(100f, 0f));
            weak.Movement.SetInput(1f, 0f);
            strong.Movement.SetInput(1f, 0f);

            yield return WaitFixedUpdates(100);

            Assert.That(strong.Root.transform.position.y, Is.GreaterThan(weak.Root.transform.position.y + 0.5f));
        }

        [UnityTest]
        public IEnumerator Apply_EngineSwapAtRuntime_DrivesFartherThanBefore()
        {
            var weakLoadout = rig.CreateLoadout(100f);
            var strongLoadout = rig.CreateLoadout(600f);
            var swapped = rig.BuildTank("Swapped", weakLoadout, new Vector2(-100f, 0f));
            var control = rig.BuildTank("Control", weakLoadout, new Vector2(100f, 0f));
            swapped.Controller.Apply(strongLoadout);
            swapped.Movement.SetInput(1f, 0f);
            control.Movement.SetInput(1f, 0f);

            yield return WaitFixedUpdates(100);

            Assert.That(swapped.Root.transform.position.y, Is.GreaterThan(control.Root.transform.position.y + 0.5f));
        }
    }
}
