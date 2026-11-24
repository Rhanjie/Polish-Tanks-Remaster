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
        private TankInput tankInput;
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
            root.AddComponent<TankMovement>();

            var gunObject = new GameObject("Gun");
            gunObject.transform.SetParent(root.transform, false);
            gun = gunObject.AddComponent<TankGun>();
            gun.Settings = new GunSettings { reloadTime = 100f, projectileSpeed = 10f };
            gun.Fired += _ => shots++;

            tankInput = root.AddComponent<TankInput>();

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

        [UnityTest]
        public IEnumerator Update_HeldByMenu_IgnoresThrottle()
        {
            tankInput.Hold(this);
            input.Move = new Vector2(0f, 1f);

            for (var i = 0; i < 10; i++)
                yield return new WaitForFixedUpdate();

            Assert.That(body.linearVelocity.y, Is.Zero);
        }

        [UnityTest]
        public IEnumerator Update_FireHeldThroughRelease_FiresOnlyAfterPressedAgain()
        {
            input.FireHeld = true;
            tankInput.Hold(this);
            tankInput.Release(this);
            yield return null;
            yield return null;
            var shotsWhileStillHeld = shots;

            input.FireHeld = false;
            yield return null;
            input.FireHeld = true;
            input.FirePressedThisFrame = true;
            yield return null;
            yield return null;

            Assert.That(shotsWhileStillHeld, Is.Zero);
            Assert.That(shots, Is.EqualTo(1));
        }
    }
}
