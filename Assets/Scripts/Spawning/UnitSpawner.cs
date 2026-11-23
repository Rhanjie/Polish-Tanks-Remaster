using System;
using PolishTanks.Missions;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolishTanks.Spawning
{
    /// <summary>
    /// <see cref="IUnitSpawner"/> for a scene with a Reflex container. The copy is made under an inactive staging
    /// object, so <c>Awake</c> and <c>OnEnable</c> wait until <c>prepare</c> has run and the scene's services are
    /// injected; moving it to its parent then wakes it up. The mission starts tracking it right after.
    /// </summary>
    public sealed class UnitSpawner : IUnitSpawner
    {
        private readonly Func<Container> container;
        private readonly Transform staging;
        private readonly IMission mission;

        public UnitSpawner(Func<Container> container, Transform staging, IMission mission)
        {
            this.container = container;
            this.staging = staging;
            this.mission = mission;
        }

        public GameObject Spawn(GameObject prefab, Vector2 position, float angle, Transform parent = null, Action<GameObject> prepare = null)
        {
            if (prefab == null)
                return null;

            var unit = Object.Instantiate(prefab, position, Quaternion.Euler(0f, 0f, angle), staging);
            unit.name = prefab.name;
            prepare?.Invoke(unit);
            GameObjectInjector.InjectRecursive(unit, container());
            unit.transform.SetParent(parent, true);
            mission?.Track(unit);
            return unit;
        }
    }
}
