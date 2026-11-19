using NUnit.Framework;
using PolishTanks.Infantry;

namespace PolishTanks.Tests.EditMode
{
    public class WoundModelTests
    {
        private const float Low = 0.1f;
        private const float High = 0.9f;

        [Test]
        public void TakeHit_CannonRound_KillsInOneHitWhateverTheRoll()
        {
            Assert.That(new WoundModel().TakeHit(WoundModel.HeavyCaliber, High), Is.True);
        }

        [Test]
        public void TakeHit_StrongRifleWithSevereRoll_KillsInOneHit()
        {
            Assert.That(new WoundModel().TakeHit(WoundModel.StrongRifleCaliber, Low), Is.True);
        }

        [Test]
        public void TakeHit_StrongRifleWithMildRoll_NeedsASecondHit()
        {
            var wounds = new WoundModel();

            var firstKills = wounds.TakeHit(7.92f, High);
            var secondKills = wounds.TakeHit(7.92f, High);

            Assert.That(firstKills, Is.False);
            Assert.That(secondKills, Is.True);
        }

        [Test]
        public void TakeHit_LightRifleWithBestRolls_KillsInTwoHits()
        {
            var wounds = new WoundModel();

            wounds.TakeHit(7.62f, Low);

            Assert.That(wounds.TakeHit(7.62f, Low), Is.True);
        }

        [Test]
        public void TakeHit_LightRifleWithWorstRolls_SurvivesTwoHitsAndDiesOnTheThird()
        {
            var wounds = new WoundModel();
            wounds.TakeHit(7.62f, High);

            var survivesSecond = !wounds.TakeHit(7.62f, High);

            Assert.That(survivesSecond, Is.True);
            Assert.That(wounds.TakeHit(7.62f, High), Is.True);
        }

        [Test]
        public void TakeHit_AlreadyDead_ReportsNoNewKill()
        {
            var wounds = new WoundModel();
            wounds.TakeHit(WoundModel.HeavyCaliber, Low);

            Assert.That(wounds.TakeHit(WoundModel.HeavyCaliber, Low), Is.False);
        }

        [Test]
        public void BlastWound_AtTheCentre_IsLethal()
        {
            Assert.That(WoundModel.BlastWound(0f, 2.5f), Is.GreaterThanOrEqualTo(1f));
        }

        [Test]
        public void BlastWound_AtTheEdge_IsAMildWound()
        {
            Assert.That(WoundModel.BlastWound(2.5f, 2.5f), Is.EqualTo(WoundModel.BlastEdgeWound).Within(1e-4f));
        }

        [Test]
        public void BlastWound_OutsideTheRadius_IsZero()
        {
            Assert.That(WoundModel.BlastWound(2.6f, 2.5f), Is.EqualTo(0f));
        }

        [Test]
        public void TakeBlast_OutsideTheRadius_LeavesHealthUntouched()
        {
            var wounds = new WoundModel();

            wounds.TakeBlast(3f, 2.5f);

            Assert.That(wounds.Health, Is.EqualTo(1f));
        }

        [Test]
        public void TakeBlast_NearTheEdgeTwice_StillSurvives()
        {
            var wounds = new WoundModel();
            wounds.TakeBlast(2.5f, 2.5f);

            Assert.That(wounds.TakeBlast(2.5f, 2.5f), Is.False);
        }
    }
}
