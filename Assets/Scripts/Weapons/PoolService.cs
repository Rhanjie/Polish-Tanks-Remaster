using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// <see cref="IPoolService"/> that keeps pooled objects under one parent transform.
    /// Bound per scene, so pools are created with the scene and destroyed when its container is disposed.
    /// </summary>
    public sealed class PoolService : IPoolService, IDisposable
    {
        private readonly Dictionary<PooledBehaviour, object> pools = new Dictionary<PooledBehaviour, object>();
        private readonly Transform root;

        public PoolService(Transform root)
        {
            this.root = root != null ? root : throw new ArgumentNullException(nameof(root));
        }

        public PrefabPool<T> For<T>(T prefab, int prewarm = 0) where T : PooledBehaviour
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (pools.TryGetValue(prefab, out var existing) && existing is PrefabPool<T> pool)
                return pool;

            pool = new PrefabPool<T>(prefab, root, prewarm);
            pools[prefab] = pool;
            return pool;
        }

        public void Dispose()
        {
            pools.Clear();
            if (root != null)
                Object.Destroy(root.gameObject);
        }
    }
}
