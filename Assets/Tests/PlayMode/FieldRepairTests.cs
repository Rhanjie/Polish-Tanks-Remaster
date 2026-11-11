using System.Collections;
using NUnit.Framework;
using PolishTanks.CameraRig;
using PolishTanks.Controls;
using PolishTanks.Damage;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>The repair key on a tank wired in code: it starts and stops a field repair, and driving or firing neither moves the tank nor calls the repair off.</summary>
    public class FieldRepairTests
    {
        private TankRig rig;
        private WeaponFixture fixture;
        private Container container;
        private FakePlayerInput input;
        private TankRig.BuiltTank tank;
        private ModuleDamage damage;
        private FieldRepair repair;

        [SetUp]
        public void SetUp()
        {
            rig = new TankRig();
            fixture = new WeaponFixture();
            input = new FakePlayerInput();
            container = new ContainerBuilder()
                .RegisterValue(input, new[] { typeof(IPlayerInput) })
                .RegisterValue(new FakeGameCamera(), new[] { typeof(IGameCamera) })
                .RegisterValue(fixture.Pools, new[] { typeof(IPoolService) })
                .RegisterValue(fixture.CameraShake, new[] { typeof(ICameraShake) })
                .Build();

            var loadout = rig.CreateLoadout();
            TankRig.SetField(loadout.Tracks, "breakChance", 1f);
            tank = rig.BuildTank("Tank", null, Vector2.zero);
            tank.Root.SetActive(false);
            damage = tank.Root.AddComponent<ModuleDamage>();
            TankRig.SetField(damage, "settings", rig.Create<ModuleDamageSettings>(
                ("fireTickInterval", 0.05f), ("fireTickDamage", 0.0001f), ("fireDuration", 30f),
                ("engineRepair", 0.2f), ("trackRepair", 100f)));
            repair = tank.Root.AddComponent<FieldRepair>();
            tank.Root.AddComponent<TankInput>();
            GameObjectInjector.InjectRecursive(tank.Root, container);
            tank.Root.SetActive(true);
            tank.Controller.Apply(loadout);
        }

        [TearDown]
        public void TearDown()
        {
            container.Dispose();
            rig.Dispose();
            fixture.Dispose();
        }

        private IEnumerator PressRepairUntil(bool repairing)
        {
            input.RepairPressedThisFrame = true;
            var deadline = Time.time + 2f;
            while (repair.IsRepairing != repairing && Time.time < deadline)
                yield return null;
            input.RepairPressedThisFrame = false;
        }

        [UnityTest]
        public IEnumerator Update_RepairPressedWithDestroyedEngine_PatchesEngineAndPutsOutFire()
        {
            damage.Break(VehiclePart.Engine);
            var engine = new PartId(VehiclePart.Engine);

            yield return PressRepairUntil(true);
            var deadline = Time.time + 3f;
            while (damage.Model.ConditionOf(engine) != PartCondition.FieldRepaired && Time.time < deadline)
                yield return null;

            Assert.That(damage.Model.ConditionOf(engine), Is.EqualTo(PartCondition.FieldRepaired));
            Assert.That(damage.IsBurning, Is.False);
        }

        [UnityTest]
        public IEnumerator Update_DriveAndFireWhileRepairing_KeepRepairingAndTankStill()
        {
            damage.Break(VehiclePart.LeftTrack);
            yield return PressRepairUntil(true);
            var start = tank.Root.transform.position;

            input.Move = new Vector2(0f, 1f);
            input.FireHeld = true;
            input.FirePressedThisFrame = true;
            for (var i = 0; i < 20; i++)
                yield return new WaitForFixedUpdate();

            Assert.That(repair.IsRepairing, Is.True);
            Assert.That(Vector3.Distance(tank.Root.transform.position, start), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator Update_RepairPressedWhileRepairing_StopsRepair()
        {
            damage.Break(VehiclePart.LeftTrack);
            yield return PressRepairUntil(true);

            yield return PressRepairUntil(false);

            Assert.That(repair.IsRepairing, Is.False);
        }
    }
}
