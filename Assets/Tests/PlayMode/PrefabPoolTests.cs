using NUnit.Framework;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    public class PrefabPoolTests
    {
        private WeaponFixture fixture;
        private TestPooled prefab;
        private Transform root;

        [SetUp]
        public void SetUp()
        {
            fixture = new WeaponFixture();
            prefab = fixture.CreatePrefab<TestPooled>();
            root = fixture.Track(new GameObject("PoolRoot")).transform;
        }

        [TearDown]
        public void TearDown() => fixture.Dispose();

        [Test]
        public void Constructor_WithPrewarm_CreatesThatManyInactiveInstances()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root, 3);

            Assert.That(pool.CountInactive, Is.EqualTo(3));
            Assert.That(root.childCount, Is.EqualTo(3));
            Assert.That(root.GetChild(0).gameObject.activeSelf, Is.False);
        }

        [Test]
        public void Constructor_WithoutPrewarm_CreatesNothing()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root);

            Assert.That(pool.CountAll, Is.EqualTo(0));
        }

        [Test]
        public void Get_ActivatesAndPositionsInstance()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root, 1);

            var item = pool.Get(new Vector3(3f, 4f, 0f), Quaternion.identity);

            Assert.That(item.gameObject.activeSelf, Is.True);
            Assert.That(item.transform.position, Is.EqualTo(new Vector3(3f, 4f, 0f)));
        }

        [Test]
        public void Get_MarksInstanceSpawnedAndCallsOnSpawned()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root, 1);

            var item = pool.Get(Vector3.zero, Quaternion.identity);

            Assert.That(item.IsSpawned, Is.True);
            Assert.That(item.SpawnedCount, Is.EqualTo(1));
        }

        [Test]
        public void Get_WithoutPrewarm_CreatesInstanceOnDemand()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root);

            pool.Get(Vector3.zero, Quaternion.identity);

            Assert.That(pool.CountActive, Is.EqualTo(1));
        }

        [Test]
        public void ReturnToPool_DeactivatesInstance()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root, 1);
            var item = pool.Get(Vector3.zero, Quaternion.identity);
            var returnedBefore = item.ReturnedCount;

            item.ReturnToPool();

            Assert.That(item.gameObject.activeSelf, Is.False);
            Assert.That(item.ReturnedCount, Is.EqualTo(returnedBefore + 1));
        }

        [Test]
        public void Get_AfterReturn_ReusesSameInstance()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root);
            var first = pool.Get(Vector3.zero, Quaternion.identity);
            first.ReturnToPool();

            var second = pool.Get(Vector3.one, Quaternion.identity);

            Assert.That(second, Is.SameAs(first));
            Assert.That(pool.CountAll, Is.EqualTo(1));
        }

        [Test]
        public void ReturnToPool_CalledTwice_KeepsCountsConsistent()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root, 1);
            var item = pool.Get(Vector3.zero, Quaternion.identity);
            var returnedBefore = item.ReturnedCount;

            item.ReturnToPool();
            item.ReturnToPool();

            Assert.That(pool.CountInactive, Is.EqualTo(1));
            Assert.That(pool.CountActive, Is.EqualTo(0));
            Assert.That(item.ReturnedCount, Is.EqualTo(returnedBefore + 1));
        }

        [Test]
        public void ReturnToPool_WhenNeverSpawned_DoesNothing()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root, 1);
            var item = root.GetChild(0).GetComponent<TestPooled>();

            item.ReturnToPool();

            Assert.That(pool.CountInactive, Is.EqualTo(1));
        }

        [Test]
        public void IsAlive_AfterParentDestroyed_IsFalse()
        {
            var pool = new PrefabPool<TestPooled>(prefab, root);

            Object.DestroyImmediate(root.gameObject);

            Assert.That(pool.IsAlive, Is.False);
        }

        [Test]
        public void PoolRegistry_For_SamePrefab_ReturnsSamePool()
        {
            var first = PoolRegistry.For(prefab);

            Assert.That(PoolRegistry.For(prefab), Is.SameAs(first));
        }

        [Test]
        public void PoolRegistry_For_AfterPoolsRootDestroyed_BuildsNewPool()
        {
            var first = PoolRegistry.For(prefab);
            Object.DestroyImmediate(GameObject.Find("[Pools]"));

            var second = PoolRegistry.For(prefab);

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.IsAlive, Is.True);
        }
    }
}
