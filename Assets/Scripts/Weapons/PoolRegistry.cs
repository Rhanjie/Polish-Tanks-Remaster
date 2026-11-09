using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// One shared pool per prefab, created on first use under a scene object.
    /// The pools die with the scene and are rebuilt on the next request.
    /// </summary>
    public static class PoolRegistry
    {
        private static readonly Dictionary<PooledBehaviour, object> Pools = new Dictionary<PooledBehaviour, object>();
        private static Transform root;

        public static PrefabPool<T> For<T>(T prefab, int prewarm = 0) where T : PooledBehaviour
        {
            if (Pools.TryGetValue(prefab, out var existing) && existing is PrefabPool<T> pool && pool.IsAlive)
                return pool;

            if (root == null)
                root = new GameObject("[Pools]").transform;

            pool = new PrefabPool<T>(prefab, root, prewarm);
            Pools[prefab] = pool;
            return pool;
        }

        // Static state survives Play Mode restarts when domain reload is off.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Pools.Clear();
            root = null;
        }
    }
}
