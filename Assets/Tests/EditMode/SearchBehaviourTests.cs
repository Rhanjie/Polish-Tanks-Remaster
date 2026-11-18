using NUnit.Framework;
using PolishTanks.AI.Behaviours;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class SearchBehaviourTests
    {
        private static readonly Vector2 Lead = new Vector2(10f, 0f);

        private AiTestRig rig;

        [SetUp]
        public void SetUp()
        {
            rig = new AiTestRig();
            rig.Target.Report(Lead, 0f);
        }

        [TearDown]
        public void TearDown() => rig.Dispose();

        [Test]
        public void Score_ScriptedCombat_IsZero()
        {
            rig.Context.ScriptedCombat = true;

            Assert.That(new SearchBehaviour().Score(rig.Context), Is.EqualTo(0f));
        }

        [Test]
        public void Score_NothingKnownAboutTarget_IsZero()
        {
            rig.Target.Clear();

            Assert.That(new SearchBehaviour().Score(rig.Context), Is.EqualTo(0f));
        }

        [Test]
        public void Score_TargetRemembered_IsPositive()
        {
            Assert.That(new SearchBehaviour().Score(rig.Context), Is.GreaterThan(0f));
        }

        [Test]
        public void Enter_SearchLostTargetOn_DrivesToLastKnownPosition()
        {
            rig.WithDoctrine("searchLostTarget", true);

            new SearchBehaviour().Enter(rig.Context);

            Assert.That(rig.Driver.LastMoveTo, Is.EqualTo(Lead));
        }

        [Test]
        public void Enter_SearchLostTargetOff_StaysPut()
        {
            rig.WithDoctrine("searchLostTarget", false);

            new SearchBehaviour().Enter(rig.Context);

            Assert.That(rig.Driver.MoveToCalls, Is.Empty);
        }

        [Test]
        public void Tick_SearchLostTargetOff_AimsTurretAtLastKnownPosition()
        {
            rig.WithDoctrine("searchLostTarget", false);
            var search = new SearchBehaviour();
            search.Enter(rig.Context);

            search.Tick(rig.Context);

            Assert.That(rig.Driver.AimCalls, Does.Contain(Lead));
        }

        [Test]
        public void Tick_LeadMovesPastNewLeadDistance_ReplansToTheNewPosition()
        {
            rig.WithDoctrine("searchLostTarget", true);
            var search = new SearchBehaviour();
            search.Enter(rig.Context);
            var moved = Lead + new Vector2(SearchBehaviour.NewLeadDistance + 1f, 0f);
            rig.Target.Report(moved, 1f);

            search.Tick(rig.Context);

            Assert.That(rig.Driver.LastMoveTo, Is.EqualTo(moved));
        }

        [Test]
        public void Tick_LeadMovesLessThanNewLeadDistance_KeepsTheRoute()
        {
            rig.WithDoctrine("searchLostTarget", true);
            var search = new SearchBehaviour();
            search.Enter(rig.Context);
            rig.Target.Report(Lead + new Vector2(SearchBehaviour.NewLeadDistance - 1f, 0f), 1f);

            search.Tick(rig.Context);

            Assert.That(rig.Driver.MoveToCalls.Count, Is.EqualTo(1));
        }
    }
}
