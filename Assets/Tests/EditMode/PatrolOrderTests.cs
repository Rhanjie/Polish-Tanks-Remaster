using NUnit.Framework;
using PolishTanks.AI.Orders;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class PatrolOrderTests
    {
        private static readonly Vector2 P0 = new Vector2(0f, 0f);
        private static readonly Vector2 P1 = new Vector2(10f, 0f);
        private static readonly Vector2 P2 = new Vector2(20f, 0f);
        private const float Pause = 2f;

        private FakeTankDriver driver;

        [SetUp]
        public void SetUp() => driver = new FakeTankDriver();

        private OrderContext Ctx(float time) => new OrderContext(driver, time);

        private PatrolOrder BeginPatrol()
        {
            var patrol = new PatrolOrder(new[] { P0, P1, P2 }, Pause, true);
            patrol.Begin(Ctx(0f));
            return patrol;
        }

        [Test]
        public void Begin_FirstTime_DrivesToTheFirstPoint()
        {
            BeginPatrol();

            Assert.That(driver.LastMoveTo, Is.EqualTo(P0));
        }

        [Test]
        public void Tick_ArrivedAtPoint_StandsStillForThePause()
        {
            var patrol = BeginPatrol();
            driver.Arrive();
            patrol.Tick(Ctx(1f));

            patrol.Tick(Ctx(2.9f));

            Assert.That(driver.MoveToCalls.Count, Is.EqualTo(1));
        }

        [Test]
        public void Tick_PauseOver_DrivesToTheNextPoint()
        {
            var patrol = BeginPatrol();
            driver.Arrive();
            patrol.Tick(Ctx(1f));

            patrol.Tick(Ctx(3f));

            Assert.That(driver.LastMoveTo, Is.EqualTo(P1));
        }

        [Test]
        public void Tick_PathToPointFailed_SkipsThePauseAndTriesTheNextPoint()
        {
            var patrol = BeginPatrol();
            driver.FailPath();
            patrol.Tick(Ctx(1f));

            patrol.Tick(Ctx(1f));

            Assert.That(driver.LastMoveTo, Is.EqualTo(P1));
        }

        [Test]
        public void Begin_AfterSuspendMidRoute_HeadsForThePointItWasGoingTo()
        {
            var patrol = BeginPatrol();
            driver.Arrive();
            patrol.Tick(Ctx(1f));
            patrol.Tick(Ctx(3f));
            patrol.End(Ctx(4f));

            patrol.Begin(Ctx(10f));

            Assert.That(driver.LastMoveTo, Is.EqualTo(P1));
        }

        [Test]
        public void Tick_LastPointReachedWithoutLoop_IsDoneAfterThePause()
        {
            var patrol = new PatrolOrder(new[] { P0 }, Pause, false);
            patrol.Begin(Ctx(0f));
            driver.Arrive();
            patrol.Tick(Ctx(1f));

            Assert.That(patrol.Tick(Ctx(3f)), Is.EqualTo(OrderStatus.Done));
        }

        [Test]
        public void Tick_LastPointReachedWithLoop_ReturnsToTheFirstPoint()
        {
            var patrol = new PatrolOrder(new[] { P0, P1 }, Pause, true);
            patrol.Begin(Ctx(0f));
            driver.Arrive();
            patrol.Tick(Ctx(1f));
            patrol.Tick(Ctx(3f));
            driver.Arrive();
            patrol.Tick(Ctx(4f));

            patrol.Tick(Ctx(6f));

            Assert.That(driver.LastMoveTo, Is.EqualTo(P0));
        }

        [Test]
        public void Tick_NoPoints_IsDone()
        {
            var patrol = new PatrolOrder(new Vector2[0], Pause, true);
            patrol.Begin(Ctx(0f));

            Assert.That(patrol.Tick(Ctx(0f)), Is.EqualTo(OrderStatus.Done));
        }
    }
}
