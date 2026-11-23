using System;
using PolishTanks.Tank.Modules;
using UnityEngine;

namespace PolishTanks.Spawning
{
    /// <summary>One unit of an <see cref="EnemyWave"/>: what rolls in, from where, where it heads and how late.</summary>
    [Serializable]
    public class EnemyWaveUnit
    {
        [SerializeField] private GameObject prefab;
        [Tooltip("Optional. Replaces the prefab's loadout.")]
        [SerializeField] private TankLoadout loadout;
        [SerializeField] private Transform spawnPoint;
        [Tooltip("Where the unit drives to and then holds. Empty: it keeps the prefab's orders.")]
        [SerializeField] private Transform destination;
        [Tooltip("Seconds after the wave starts.")]
        [SerializeField, Min(0f)] private float delay;

        public EnemyWaveUnit()
        {
        }

        public EnemyWaveUnit(GameObject prefab, Transform spawnPoint, Transform destination, float delay = 0f, TankLoadout loadout = null)
        {
            this.prefab = prefab;
            this.spawnPoint = spawnPoint;
            this.destination = destination;
            this.delay = delay;
            this.loadout = loadout;
        }

        public GameObject Prefab => prefab;
        public TankLoadout Loadout => loadout;
        public Transform SpawnPoint => spawnPoint;
        public Transform Destination => destination;
        public float Delay => delay;
    }
}
