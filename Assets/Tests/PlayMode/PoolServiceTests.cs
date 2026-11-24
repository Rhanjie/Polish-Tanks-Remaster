using System;
using NUnit.Framework;

namespace PolishTanks.Tests.PlayMode
{
    public class PoolServiceTests
    {
        private WeaponFixture fixture;

        [SetUp]
        public void SetUp() => fixture = new WeaponFixture();

        [TearDown]
        public void TearDown() => fixture.Dispose();

        [Test]
        public void For_DifferentPrefabs_ReturnDifferentPools()
        {
            var first = fixture.CreatePrefab<TestPooled>("A");
            var second = fixture.CreatePrefab<TestPooled>("B");

            Assert.That(fixture.Pools.For(first), Is.Not.SameAs(fixture.Pools.For(second)));
        }

        [Test]
        public void For_NullPrefab_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => fixture.Pools.For<TestPooled>(null));
        }
    }
}
