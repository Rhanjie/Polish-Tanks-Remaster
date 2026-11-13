using System.Collections;
using NUnit.Framework;
using PolishTanks.Combat;
using PolishTanks.Damage;
using PolishTanks.Rendering;
using PolishTanks.Tank;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>The vision damage view publishes the player tank's broken vision blocks to <see cref="VisionDamageState"/> and clears the effect once the tank is knocked out.</summary>
    public class VisionDamageViewTests
    {
        private const float Timeout = 5f;

        private TankRig rig;
        private WeaponFixture fixture;
        private Container container;
        private TankRig.BuiltTank tank;
        private ModuleDamage damage;
        private GameObject viewObject;

        [SetUp]
        public void SetUp()
        {
            rig = new TankRig();
            fixture = new WeaponFixture();

            var loadout = rig.CreateLoadout();
            tank = rig.BuildTank("Tank", null, Vector2.zero);
            tank.Root.SetActive(false);
            damage = tank.Root.AddComponent<ModuleDamage>();
            TankRig.SetField(damage, "settings", rig.Create<ModuleDamageSettings>(
                ("fireTickInterval", 0.05f), ("fireTickDamage", 0.0001f), ("fireDuration", 30f)));
            fixture.Inject(tank.Root);
            tank.Root.SetActive(true);
            tank.Controller.Apply(loadout);

            container = new ContainerBuilder()
                .RegisterValue(new PlayerTank(tank.Controller))
                .Build();
            viewObject = new GameObject("VisionDamageView");
            viewObject.SetActive(false);
            viewObject.AddComponent<VisionDamageView>();
            GameObjectInjector.InjectRecursive(viewObject, container);
            viewObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(viewObject);
            VisionDamageState.Clear();
            container.Dispose();
            rig.Dispose();
            fixture.Dispose();
        }

        private static float LossOf(int block) => block < 4 ? VisionDamageState.LossA[block] : VisionDamageState.LossB[block - 4];

        [UnityTest]
        public IEnumerator LateUpdate_VisionBlockBroken_PublishesThatBlocksLoss()
        {
            yield return null;
            var broken = damage.Break(VehiclePart.VisionBlock);
            Assume.That(broken.HasValue, Is.True);

            var deadline = Time.time + Timeout;
            while (LossOf(broken.Value.Index) < 1f && Time.time < deadline)
                yield return null;

            Assert.That(LossOf(broken.Value.Index), Is.EqualTo(1f).Within(1e-3f));
        }

        [UnityTest]
        public IEnumerator LateUpdate_TankDestroyedWithBrokenBlock_EffectEventuallyGoesInactive()
        {
            yield return null;
            damage.Break(VehiclePart.VisionBlock);
            var deadline = Time.time + Timeout;
            while (!VisionDamageState.IsActive && Time.time < deadline)
                yield return null;
            Assume.That(VisionDamageState.IsActive, Is.True);

            var damageable = tank.Root.GetComponent<Damageable>();
            damageable.ApplyDamage(damageable.MaxHitPoints + 1f);
            deadline = Time.time + Timeout;
            while (VisionDamageState.IsActive && Time.time < deadline)
                yield return null;

            Assert.That(VisionDamageState.IsActive, Is.False);
        }
    }
}
