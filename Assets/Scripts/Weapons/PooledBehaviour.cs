using System;
using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// Base for objects handed out by a <see cref="PrefabPool{T}"/>. Call <see cref="ReturnToPool"/>
    /// instead of Destroy when the object is done.
    /// </summary>
    public abstract class PooledBehaviour : MonoBehaviour
    {
        internal Action<PooledBehaviour> ReleaseAction;

        /// <summary>True between being taken from the pool and returned to it.</summary>
        public bool IsSpawned { get; internal set; }

        /// <summary>Gives the object back to its pool. Safe to call more than once.</summary>
        public void ReturnToPool()
        {
            if (!IsSpawned)
                return;
            IsSpawned = false;

            if (ReleaseAction != null)
                ReleaseAction(this);
            else
                Destroy(gameObject); // not from a pool, e.g. placed in a scene by hand
        }

        /// <summary>Called after the object is positioned and activated.</summary>
        protected internal virtual void OnSpawned() { }

        /// <summary>Called right before the object is deactivated and stored.</summary>
        protected internal virtual void OnReturned() { }
    }
}
