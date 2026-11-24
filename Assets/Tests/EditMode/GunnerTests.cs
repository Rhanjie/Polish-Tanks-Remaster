using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class GunnerTests
    {
        private static readonly Vector2 Target = new Vector2(20f, 0f);

        private AiTestRig rig;
        private Gunner gunner;

        [SetUp]
        public void SetUp()
        {
            rig = new AiTestRig()
                .WithDoctrine("aimTolerance", 2f)
                .WithDoctrine("shotInterval", 5f)
                .WithDoctrine("burstSeconds", 1f)
                .WithDoctrine("burstPause", 3f);
            gunner = new Gunner(() => 0.5f);
        }

        [TearDown]
        public void TearDown() => rig.Dispose();

        private void Engage(float time, float deltaTime = 0.02f)
        {
            rig.Context.Time = time;
            rig.Context.DeltaTime = deltaTime;
            gunner.Engage(rig.Context, rig.Driver, Target);
        }

        [Test]
        public void Engage_AimErrorBeyondTolerance_DoesNotFire()
        {
            rig.Driver.AimError = 10f;

            Engage(0f);

            Assert.That(rig.Driver.MainGunShots, Is.EqualTo(0));
        }

        [Test]
        public void Engage_NegativeAimErrorWithinTolerance_Fires()
        {
            rig.Driver.AimError = -1.5f;

            Engage(0f);

            Assert.That(rig.Driver.MainGunShots, Is.EqualTo(1));
        }

        [Test]
        public void Engage_BeforeShotIntervalPasses_DoesNotFireAgain()
        {
            Engage(0f);

            Engage(4.9f);

            Assert.That(rig.Driver.MainGunShots, Is.EqualTo(1));
        }

        [Test]
        public void Engage_AfterShotInterval_FiresAgain()
        {
            Engage(0f);

            Engage(5f);

            Assert.That(rig.Driver.MainGunShots, Is.EqualTo(2));
        }

        [Test]
        public void Engage_GunNotReady_FiresAsSoonAsItIs()
        {
            rig.Driver.CanFire = false;
            Engage(0f);
            rig.Driver.CanFire = true;

            Engage(0.1f);

            Assert.That(rig.Driver.MainGunShots, Is.EqualTo(1));
        }

        [Test]
        public void Engage_MachineGunsOnTarget_FollowTheBurstCycle()
        {
            var held = new List<bool>();
            Engage(0f, 0.5f);
            held.Add(rig.Driver.MachineGunsHeld);
            Engage(0.5f, 2f);
            held.Add(rig.Driver.MachineGunsHeld);
            Engage(2.5f, 2f);
            held.Add(rig.Driver.MachineGunsHeld);

            Assert.That(held, Is.EqualTo(new[] { true, false, true }));
        }

        [Test]
        public void Engage_AimErrorBeyondTolerance_MachineGunsStaySilent()
        {
            rig.Driver.AimError = 10f;

            Engage(0f, 0.5f);

            Assert.That(rig.Driver.MachineGunsHeld, Is.False);
        }

        [Test]
        public void Engage_ZeroBurstSecondsAndPause_MachineGunsStaySilent()
        {
            rig.WithDoctrine("burstSeconds", 0f).WithDoctrine("burstPause", 0f);

            Engage(0f, 0.5f);

            Assert.That(rig.Driver.MachineGunsHeld, Is.False);
        }

        [Test]
        public void CeaseFire_WhileFiringBurst_ReleasesMachineGuns()
        {
            Engage(0f, 0.5f);

            gunner.CeaseFire(rig.Driver);

            Assert.That(rig.Driver.MachineGunsHeld, Is.False);
        }
    }
}
