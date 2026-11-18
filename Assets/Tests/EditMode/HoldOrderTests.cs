using NUnit.Framework;
using PolishTanks.AI.Orders;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class HoldOrderTests
    {
        private static readonly Vector2 Post = new Vector2(5f, 5f);

        private FakeTankDriver driver;
        private HoldOrder hold;

        [SetUp]
        public void SetUp()
        {
            driver = new FakeTankDriver { Position = Post };
            hold = new HoldOrder();
            hold.Begin(new OrderContext(driver, 0f));
        }

        [Test]
        public void Begin_AfterBeingPulledFarFromThePost_DrivesBackToIt()
        {
            driver.Position = Post + new Vector2(HoldOrder.PostTolerance + 3f, 0f);

            hold.Begin(new OrderContext(driver, 10f));

            Assert.That(driver.LastMoveTo, Is.EqualTo(Post));
        }

        [Test]
        public void Begin_WithinToleranceOfThePost_DoesNotDrive()
        {
            driver.Position = Post + new Vector2(HoldOrder.PostTolerance * 0.5f, 0f);

            hold.Begin(new OrderContext(driver, 10f));

            Assert.That(driver.MoveToCalls, Is.Empty);
        }

        [Test]
        public void Begin_SecondCallElsewhere_KeepsTheFirstPost()
        {
            driver.Position = new Vector2(50f, 50f);
            hold.Begin(new OrderContext(driver, 10f));

            Assert.That(hold.Post, Is.EqualTo(Post));
        }
    }
}
