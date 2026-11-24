using NUnit.Framework;
using PolishTanks.CameraRig;
using PolishTanks.Weapons;

namespace PolishTanks.Tests.EditMode
{
    public class HitShakeTests
    {
        [Test]
        public void Force_GrowsFasterThanCalibre()
        {
            var light = HitShake.Force(37.5f, HitOutcome.Blocked, 20f, 75f, 1f);
            var heavy = HitShake.Force(75f, HitOutcome.Blocked, 20f, 75f, 1f);

            Assert.That(heavy, Is.GreaterThan(light * 2f));
        }

        [Test]
        public void Force_MachineGunBullet_DoesNotShake()
        {
            Assert.That(HitShake.Force(7.9f, HitOutcome.Penetrated, 20f, 75f, 1f), Is.Zero);
        }
    }
}
