using NUnit.Framework;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Casts sight lines through colliders built in code; [Test] methods run in one frame, so the physics world is synced by hand.</summary>
    public class LineOfSightTests
    {
        private static readonly Vector2 From = Vector2.zero;
        private static readonly Vector2 To = new Vector2(10f, 0f);

        private WeaponFixture fixture;

        [SetUp]
        public void SetUp() => fixture = new WeaponFixture();

        [TearDown]
        public void TearDown() => fixture.Dispose();

        [Test]
        public void IsClear_StaticBoxBetweenThePoints_IsBlocked()
        {
            fixture.CreateStaticBox(new Vector2(5f, 0f), Vector2.one);
            Physics2D.SyncTransforms();

            Assert.That(LineOfSight.IsClear(From, To, null, null), Is.False);
        }

        [Test]
        public void IsClear_DynamicBodyBetweenThePoints_StaysClear()
        {
            fixture.CreateDynamicBox(new Vector2(5f, 0f), Vector2.one);
            Physics2D.SyncTransforms();

            Assert.That(LineOfSight.IsClear(From, To, null, null), Is.True);
        }

        [Test]
        public void IsClear_StaticBoxOffTheLine_StaysClear()
        {
            fixture.CreateStaticBox(new Vector2(5f, 5f), Vector2.one);
            Physics2D.SyncTransforms();

            Assert.That(LineOfSight.IsClear(From, To, null, null), Is.True);
        }
    }
}
