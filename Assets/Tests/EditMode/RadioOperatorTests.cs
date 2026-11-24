using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class RadioOperatorTests
    {
        private static readonly Vector2 PlayerAt = new Vector2(50f, 50f);

        private RadioOperator radio;
        private FakeAiAgent sender;

        [SetUp]
        public void SetUp()
        {
            radio = new RadioOperator();
            sender = new FakeAiAgent();
        }

        private static FakeAiAgent At(float x, bool active = true) => new FakeAiAgent { Position = new Vector2(x, 0f), IsActive = active };

        [Test]
        public void TryBroadcast_ListenersInAndOutOfRange_AlertsOnlyThoseInRange()
        {
            var near = At(5f);
            var far = At(50f);

            var reached = radio.TryBroadcast(sender, PlayerAt, 10f, 0f, new IAiAgent[] { near, far });

            Assert.That(reached, Is.EqualTo(1));
            Assert.That(near.Alerts, Is.EqualTo(new[] { PlayerAt }));
            Assert.That(far.Alerts, Is.Empty);
        }

        [Test]
        public void TryBroadcast_ListenerExactlyAtRange_IsReached()
        {
            var edge = At(10f);

            radio.TryBroadcast(sender, PlayerAt, 10f, 0f, new IAiAgent[] { edge });

            Assert.That(edge.Alerts.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryBroadcast_SenderAndInactiveListeners_AreSkipped()
        {
            var inactive = At(2f, active: false);

            var reached = radio.TryBroadcast(sender, PlayerAt, 10f, 0f, new IAiAgent[] { sender, inactive });

            Assert.That(reached, Is.EqualTo(0));
            Assert.That(sender.Alerts, Is.Empty);
            Assert.That(inactive.Alerts, Is.Empty);
        }

        [Test]
        public void TryBroadcast_SecondCallWithinCooldown_ReachesNobody()
        {
            var listeners = new IAiAgent[] { At(5f) };
            radio.TryBroadcast(sender, PlayerAt, 10f, 0f, listeners);

            var reached = radio.TryBroadcast(sender, PlayerAt, 10f, RadioOperator.Cooldown - 0.1f, listeners);

            Assert.That(reached, Is.EqualTo(0));
        }

        [Test]
        public void TryBroadcast_AfterCooldown_ReachesAgain()
        {
            var listeners = new IAiAgent[] { At(5f) };
            radio.TryBroadcast(sender, PlayerAt, 10f, 0f, listeners);

            var reached = radio.TryBroadcast(sender, PlayerAt, 10f, RadioOperator.Cooldown, listeners);

            Assert.That(reached, Is.EqualTo(1));
        }
    }
}
