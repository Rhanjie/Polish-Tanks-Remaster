using NUnit.Framework;
using PolishTanks.AI;
using PolishTanks.Combat;

namespace PolishTanks.Tests.EditMode
{
    public class PenetrationLogTests
    {
        [Test]
        public void PenetrationRate_NoHitsOnThatSide_ReturnsTheUnknownValue()
        {
            var log = new PenetrationLog();
            log.Record(HitSide.Front, true);

            Assert.That(log.PenetrationRate(HitSide.Rear, -1f), Is.EqualTo(-1f));
        }

        [Test]
        public void PenetrationRate_MixedHitsOnOneSide_IsPenetrationsOverHits()
        {
            var log = new PenetrationLog();
            log.Record(HitSide.Front, true);
            log.Record(HitSide.Front, false);
            log.Record(HitSide.Front, false);
            log.Record(HitSide.Front, false);

            Assert.That(log.PenetrationRate(HitSide.Front, -1f), Is.EqualTo(0.25f).Within(1e-5f));
        }
    }
}
