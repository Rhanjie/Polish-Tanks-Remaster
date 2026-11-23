using System;
using UnityEngine;

namespace PolishTanks.Spawning
{
    /// <summary>
    /// Creates tanks and soldiers while the scene is running. A spawned unit gets its <c>[Inject]</c> fields before any
    /// of its components wakes up and is tracked by the mission, like units placed in the scene.
    /// </summary>
    public interface IUnitSpawner
    {
        GameObject Spawn(GameObject prefab, Vector2 position, float angle, Transform parent = null, Action<GameObject> prepare = null);
    }
}
