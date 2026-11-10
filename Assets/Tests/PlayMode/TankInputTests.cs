using System.Collections;
using NUnit.Framework;
using PolishTanks.CameraRig;
using PolishTanks.Controls;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary><see cref="TankInput"/> driven by a fake <see cref="IPlayerInput"/> injected through a Reflex container.</summary>
    public class TankInputTests
    {
        private WeaponFixture fixture;
        private Container container;
        private FakePlayerInput input;
        private Rigidbody2D body;
        private TankGun gun;
        private int shots;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            input = new FakePlayerInput();
            container = new ContainerBuilder()
                .RegisterValue(input, new[] { typeof(IPlayerInput) })
                .RegisterValue(new FakeGameCamera(), new[] { typeof(IGameCamera) })
                .RegisterValue(fixture.Pools, new[] { typeof(IPoolService) })
                .RegisterValue(fixture.CameraShake, new[] { typeof(ICameraShake) })
                .Build();

            var root = fixture.Track(new GameObject("TankHull"));
            root.SetActive(false);
            body = root.AddComponent<Rigidbody2D>();
            var movement = root.AddComponent<TankMovement>();

            var gunObject = new GameObject("Gun");
            gunObject.transform.SetParent(root.transform, false);
            gun = gunObject.AddComponent<TankGun>();
            gun.Settings = new GunSettings { reloadTime = 100f, projectileSpeed = 10f };
            gun.Fired += _ => shots++;

            var tankInput = root.AddComponent<TankInput>();
            TankRig.SetField(tankInput, "movement", movement);
            TankRig.SetField(tankInput, "gun", gun);

            GameObjectInjector.InjectRecursive(root, container);
            root.SetActive(true);
            shots = 0;
        }

        [TearDown]
        public void TearDown()
        {
            container.Dispose();
            fixture.Dispose();
        }

        [UnityTest]
        public IEnumerator Update_ForwardThrottle_MovesHullAlongItsUpAxis()
        {
            input.Move = new Vector2(0f, 1f);

            for (var i = 0; i < 10; i++)
                yield return new WaitForFixedUpdate();

            Assert.That(body.linearVelocity.y, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator Update_SteerInput_RotatesHull()
        {
            input.Move = new Vector2(0.5f, 1f);

            for (var i = 0; i < 10; i++)
                yield return new WaitForFixedUpdate();

            Assert.That(body.angularVelocity, Is.Not.Zero);
        }

        [UnityTest]
        public IEnumerator Update_FirePressed_FiresGun()
        {
            input.FirePressedThisFrame = true;
            yield return null;
            yield return null;

            Assert.That(shots, Is.EqualTo(1));
        }
    }
}
