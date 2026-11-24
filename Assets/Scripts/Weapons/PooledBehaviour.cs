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

        public bool IsSpawned { get; internal set; }

        public void ReturnToPool()
        {
            if (!IsSpawned)
                return;
            IsSpawned = false;

            if (ReleaseAction != null)
                ReleaseAction(this);
            else
                Destroy(gameObject);
        }

        protected internal virtual void OnSpawned() { }

        protected internal virtual void OnReturned() { }
    }
}
