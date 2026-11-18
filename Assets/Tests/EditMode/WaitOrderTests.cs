using NUnit.Framework;
using PolishTanks.AI.Orders;

namespace PolishTanks.Tests.EditMode
{
    public class WaitOrderTests
    {
        private FakeTankDriver driver;

        [SetUp]
        public void SetUp() => driver = new FakeTankDriver();

        private OrderContext Ctx(float time) => new OrderContext(driver, time);

        [Test]
        public void Tick_AfterTheWait_IsDone()
        {
            var wait = new WaitOrder(3f);
            wait.Begin(Ctx(0f));

            Assert.That(wait.Tick(Ctx(3f)), Is.EqualTo(OrderStatus.Done));
        }

        [Test]
        public void Tick_ResumedAfterCombat_WaitsTheFullTimeAgain()
        {
            var wait = new WaitOrder(3f);
            wait.Begin(Ctx(0f));
            wait.Tick(Ctx(2f));
            wait.End(Ctx(2f));
            wait.Begin(Ctx(10f));

            Assert.That(wait.Tick(Ctx(12f)), Is.EqualTo(OrderStatus.Running));
        }
    }
}
