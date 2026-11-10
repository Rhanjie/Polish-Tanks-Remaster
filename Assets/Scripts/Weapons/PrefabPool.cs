using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace PolishTanks.Weapons
{
    /// <summary>Reuses instances of one prefab instead of instantiating and destroying them.</summary>
    public sealed class PrefabPool<T> where T : PooledBehaviour
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly ObjectPool<T> pool;

        public PrefabPool(T prefab, Transform parent, int prewarm = 0, int maxSize = 256)
        {
            this.prefab = prefab;
            this.parent = parent;
            pool = new ObjectPool<T>(Create, actionOnGet: null, OnRelease, OnDestroyItem,
                collectionCheck: false, defaultCapacity: Mathf.Max(8, prewarm), maxSize: maxSize);
            Prewarm(prewarm);
        }

        public bool IsAlive => parent != null;

        public int CountAll => pool.CountAll;
        public int CountActive => pool.CountActive;
        public int CountInactive => pool.CountInactive;

        public T Get(Vector3 position, Quaternion rotation)
        {
            var item = pool.Get();
            item.transform.SetPositionAndRotation(position, rotation);
            item.IsSpawned = true;
            item.gameObject.SetActive(true);
            item.OnSpawned();
            return item;
        }

        private void Prewarm(int count)
        {
            if (count <= 0)
                return;
            var items = new List<T>(count);
            for (var i = 0; i < count; i++)
                items.Add(pool.Get());
            foreach (var item in items)
                pool.Release(item);
        }

        private T Create()
        {
            var item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            item.ReleaseAction = released => pool.Release((T)released);
            return item;
        }

        private static void OnRelease(T item)
        {
            if (item == null)
                return;
            item.OnReturned();
            item.IsSpawned = false;
            item.gameObject.SetActive(false);
        }

        private static void OnDestroyItem(T item)
        {
            if (item != null)
                Object.Destroy(item.gameObject);
        }
    }
}
