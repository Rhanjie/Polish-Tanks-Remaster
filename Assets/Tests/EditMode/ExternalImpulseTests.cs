using NUnit.Framework;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class ExternalImpulseTests
    {
        [Test]
        public void Add_ThroughCenter_OnlyMovesLinearly()
        {
            var kick = new ExternalImpulse();

            kick.Add(new Vector2(0f, -20f), Vector2.zero, 10f, 5f);

            Assert.That(kick.Velocity, Is.EqualTo(new Vector2(0f, -2f)));
            Assert.That(kick.AngularVelocity, Is.EqualTo(0f));
        }

        [Test]
        public void Add_OffCenter_Spins()
        {
            var kick = new ExternalImpulse();

            // Push right on a point above the center: clockwise (negative) spin.
            kick.Add(new Vector2(1f, 0f), new Vector2(0f, 2f), 10f, 4f);

            Assert.That(kick.AngularVelocity, Is.EqualTo(-0.5f * Mathf.Rad2Deg).Within(1e-3f));
        }

        [Test]
        public void Add_HeavierBody_MovesLess()
        {
            var light = new ExternalImpulse();
            var heavy = new ExternalImpulse();

            light.Add(Vector2.right * 10f, Vector2.zero, 10f, 1f);
            heavy.Add(Vector2.right * 10f, Vector2.zero, 50f, 1f);

            Assert.That(heavy.Velocity.x, Is.LessThan(light.Velocity.x));
        }

        [Test]
        public void Add_ZeroMassAndInertia_IsIgnored()
        {
            var kick = new ExternalImpulse();

            kick.Add(Vector2.one, Vector2.one, 0f, 0f);

            Assert.That(kick.Velocity, Is.EqualTo(Vector2.zero));
            Assert.That(kick.AngularVelocity, Is.EqualTo(0f));
        }

        [Test]
        public void AbsorbLoss_AlongShove_CancelsIt()
        {
            var kick = new ExternalImpulse();
            kick.Add(new Vector2(0f, -3f), Vector2.zero, 1f, 1f);

            // A wall behind the tank took away the whole backward shove.
            kick.AbsorbLoss(new Vector2(0f, -3f));

            Assert.That(kick.Velocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void AbsorbLoss_Partial_ShortensShove()
        {
            var kick = new ExternalImpulse();
            kick.Add(new Vector2(4f, 0f), Vector2.zero, 1f, 1f);

            kick.AbsorbLoss(new Vector2(1f, 5f));

            Assert.That(kick.Velocity.x, Is.EqualTo(3f).Within(1e-5f));
            Assert.That(kick.Velocity.y, Is.EqualTo(0f).Within(1e-5f));
        }

        [Test]
        public void AbsorbLoss_AgainstShove_IsIgnored()
        {
            var kick = new ExternalImpulse();
            kick.Add(new Vector2(2f, 0f), Vector2.zero, 1f, 1f);

            kick.AbsorbLoss(new Vector2(-5f, 0f));

            Assert.That(kick.Velocity.x, Is.EqualTo(2f));
        }

        [Test]
        public void Step_DecaysToZero()
        {
            var kick = new ExternalImpulse();
            kick.Add(Vector2.right * 10f, new Vector2(0f, 1f), 1f, 1f);

            for (var i = 0; i < 300; i++)
                kick.Step(0.02f, 4f, 6f);

            Assert.That(kick.Velocity, Is.EqualTo(Vector2.zero));
            Assert.That(kick.AngularVelocity, Is.EqualTo(0f));
        }

        [Test]
        public void Step_DecaysExponentially()
        {
            var kick = new ExternalImpulse();
            kick.Add(Vector2.right * 10f, Vector2.zero, 1f, 1f);

            kick.Step(0.25f, 4f, 0f);

            Assert.That(kick.Velocity.x, Is.EqualTo(10f * Mathf.Exp(-1f)).Within(1e-4f));
        }
    }
}
