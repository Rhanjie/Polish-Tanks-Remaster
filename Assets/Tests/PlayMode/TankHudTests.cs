#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using PolishTanks.CameraRig;
using PolishTanks.Controls;
using PolishTanks.Damage;
using PolishTanks.Tank;
using PolishTanks.UI;
using PolishTanks.Weapons;
using Reflex.Core;
using Reflex.Injectors;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>The tank HUD on a UIDocument built in code (editor only, assets loaded through AssetDatabase) follows the damage of the injected player tank.</summary>
    public class TankHudTests
    {
        private TankRig rig;
        private WeaponFixture fixture;
        private Container container;
        private TankRig.BuiltTank tank;
        private ModuleDamage damage;
        private GameObject hudObject;
        private UIDocument document;

        [SetUp]
        public void SetUp()
        {
            rig = new TankRig();
            fixture = new WeaponFixture();

            var loadout = rig.CreateLoadout();
            TankRig.SetField(loadout.Tracks, "breakChance", 1f);
            tank = rig.BuildTank("Tank", null, Vector2.zero);
            tank.Root.SetActive(false);
            damage = tank.Root.AddComponent<ModuleDamage>();
            TankRig.SetField(damage, "settings", rig.Create<ModuleDamageSettings>(
                ("fireTickInterval", 0.05f), ("fireTickDamage", 0.0001f), ("fireDuration", 30f),
                ("engineRepair", 0.2f), ("trackRepair", 100f)));
            var input = new FakePlayerInput();
            tank.Root.AddComponent<FieldRepair>();
            tank.Root.AddComponent<TankInput>();

            container = new ContainerBuilder()
                .RegisterValue(input, new[] { typeof(IPlayerInput) })
                .RegisterValue(new FakeGameCamera(), new[] { typeof(IGameCamera) })
                .RegisterValue(fixture.Pools, new[] { typeof(IPoolService) })
                .RegisterValue(fixture.CameraShake, new[] { typeof(ICameraShake) })
                .RegisterValue(new PlayerTank(tank.Controller))
                .Build();
            GameObjectInjector.InjectRecursive(tank.Root, container);
            tank.Root.SetActive(true);
            tank.Controller.Apply(loadout);

            hudObject = new GameObject("TankHud");
            hudObject.SetActive(false);
            document = hudObject.AddComponent<UIDocument>();
            document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/HudPanelSettings.asset");
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/TankHud.uxml");
            hudObject.AddComponent<TankHud>();
            GameObjectInjector.InjectRecursive(hudObject, container);
            hudObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(hudObject);
            container.Dispose();
            rig.Dispose();
            fixture.Dispose();
        }

        [UnityTest]
        public IEnumerator PartChanged_LeftTrackBroken_MarksTrackDestroyed()
        {
            yield return null;
            var root = document.rootVisualElement;
            Assume.That(root.Q("leftTrack").ClassListContains(TankHudPresenter.IntactClass), Is.True);

            damage.Break(VehiclePart.LeftTrack);
            yield return null;

            Assert.That(root.Q("leftTrack").ClassListContains(TankHudPresenter.DestroyedClass), Is.True);
        }
    }
}
#endif
