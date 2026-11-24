using NUnit.Framework;
using PolishTanks.Tank;

namespace PolishTanks.Tests.EditMode
{
    public class MagazineTests
    {
        [Test]
        public void SizeZero_IsUnlimited()
        {
            var mag = new Magazine(0, 3f);

            for (var i = 0; i < 1000; i++)
                mag.Consume();

            Assert.That(mag.CanFire, Is.True);
            Assert.That(mag.IsReloading, Is.False);
        }

        [Test]
        public void EmptyingBelt_StartsReload()
        {
            var mag = new Magazine(3, 2f);

            mag.Consume();
            mag.Consume();
            Assert.That(mag.CanFire, Is.True);
            mag.Consume();

            Assert.That(mag.Rounds, Is.EqualTo(0));
            Assert.That(mag.IsReloading, Is.True);
            Assert.That(mag.CanFire, Is.False);
        }

        [Test]
        public void Reload_RefillsAfterReloadTime()
        {
            var mag = new Magazine(2, 2f);
            mag.Consume();
            mag.Consume();

            mag.Tick(1f);
            Assert.That(mag.ReloadProgress, Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(mag.CanFire, Is.False);

            mag.Tick(1.1f);
            Assert.That(mag.CanFire, Is.True);
            Assert.That(mag.Rounds, Is.EqualTo(2));
        }

        [Test]
        public void ZeroReloadTime_RefillsImmediately()
        {
            var mag = new Magazine(1, 0f);

            mag.Consume();

            Assert.That(mag.CanFire, Is.True);
            Assert.That(mag.Rounds, Is.EqualTo(1));
        }

        [Test]
        public void Configure_RefillsAndCancelsReload()
        {
            var mag = new Magazine(1, 5f);
            mag.Consume();

            mag.Configure(10, 1f);

            Assert.That(mag.Rounds, Is.EqualTo(10));
            Assert.That(mag.IsReloading, Is.False);
        }
    }
}
