using NUnit.Framework;
using PolishTanks.AI.Orders;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class OrderListTests
    {
        private static readonly Vector2 A = new Vector2(10f, 0f);
        private static readonly Vector2 B = new Vector2(20f, 0f);

        private sealed class InstantOrder : Order
        {
            public override void Begin(in OrderContext context)
            {
            }

            public override OrderStatus Tick(in OrderContext context) => OrderStatus.Done;
        }

        private FakeTankDriver driver;

        [SetUp]
        public void SetUp() => driver = new FakeTankDriver();

        private OrderContext Ctx(float time = 0f) => new OrderContext(driver, time);

        private static OrderList ListOf(bool loop, params Order[] orders)
        {
            var list = new OrderList { Loop = loop };
            foreach (var order in orders)
                list.Add(order);
            return list;
        }

        [Test]
        public void Tick_FirstOrderArrived_StartsTheNext()
        {
            var list = ListOf(false, new MoveToOrder(A), new MoveToOrder(B));
            list.Tick(Ctx());

            driver.Arrive();
            list.Tick(Ctx());

            Assert.That(driver.LastMoveTo, Is.EqualTo(B));
        }

        [Test]
        public void Tick_LastOrderDoneWithLoop_StartsOverFromTheFirst()
        {
            var list = ListOf(true, new MoveToOrder(A), new MoveToOrder(B));
            list.Tick(Ctx());
            driver.Arrive();
            list.Tick(Ctx());

            driver.Arrive();
            list.Tick(Ctx());

            Assert.That(driver.LastMoveTo, Is.EqualTo(A));
        }

        [Test]
        public void Tick_LastOrderDoneWithoutLoop_FinishesAndStopsTheTank()
        {
            var list = ListOf(false, new InstantOrder());
            driver.IsMoving = true;

            list.Tick(Ctx());

            Assert.That(list.IsFinished, Is.True);
            Assert.That(driver.IsMoving, Is.False);
        }

        [Test]
        public void Tick_OnlyNullEntries_FinishesInsteadOfLooping()
        {
            var list = ListOf(true, null, null);

            list.Tick(Ctx());

            Assert.That(list.IsFinished, Is.True);
        }

        [Test]
        public void Tick_NullEntryBetweenOrders_IsSkipped()
        {
            var list = ListOf(false, null, new MoveToOrder(A), null);

            list.Tick(Ctx());

            Assert.That(driver.LastMoveTo, Is.EqualTo(A));
        }

        [Test]
        public void Add_AfterListFinished_BeginsTheNewOrderBeforeTickingIt()
        {
            var list = ListOf(false);
            list.Tick(Ctx());

            list.Add(new MoveToOrder(A));
            list.Tick(Ctx());

            Assert.That(driver.MoveToCalls, Is.EqualTo(new[] { A }));
            Assert.That(list.IsFinished, Is.False);
        }

        [Test]
        public void Resume_AfterSuspend_BeginsTheCurrentOrderAgain()
        {
            var list = ListOf(false, new MoveToOrder(A));
            list.Tick(Ctx());
            list.Suspend(Ctx());
            driver.MoveToCalls.Clear();

            list.Resume(Ctx());

            Assert.That(driver.MoveToCalls, Is.EqualTo(new[] { A }));
        }

        [Test]
        public void Suspend_RunningOrder_StopsTheTank()
        {
            var list = ListOf(false, new MoveToOrder(A));
            list.Tick(Ctx());

            list.Suspend(Ctx());

            Assert.That(driver.IsMoving, Is.False);
        }

        [Test]
        public void Set_WhileRunning_ReplacesOrdersFromTheStart()
        {
            var list = ListOf(false, new MoveToOrder(A));
            list.Tick(Ctx());

            list.Set(new Order[] { new MoveToOrder(B) }, false, Ctx());
            list.Tick(Ctx());

            Assert.That(driver.LastMoveTo, Is.EqualTo(B));
        }
    }
}
