using System.Collections.Generic;
using PolishTanks.AI;
using PolishTanks.AI.Orders;
using PolishTanks.Infantry;
using PolishTanks.Missions;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Spawning
{
    /// <summary>
    /// Enemies that roll in when an objective starts. Each unit appears at its spawn point after its delay, facing its
    /// destination, and gets orders to drive there and hold; on the way it fights as soon as it spots the player. Units
    /// can count towards a Destroy objective, whose target count then has to be set in the mission, since they don't
    /// exist when the mission counts its targets. Nothing spawns once the mission is over.
    /// </summary>
    public class EnemyWave : MonoBehaviour
    {
        [Tooltip("The wave starts when this objective starts.")]
        [SerializeField] private string objectiveId;
        [Tooltip("Optional. Destroy objective the units count towards.")]
        [SerializeField] private string targetObjectiveId;
        [SerializeField] private List<EnemyWaveUnit> units = new List<EnemyWaveUnit>();

        [Inject] private IMission mission;
        [Inject] private IUnitSpawner spawner;

        private readonly List<GameObject> spawned = new List<GameObject>();
        private MissionProgress progress;
        private bool[] done;
        private float startTime = -1f;

        public IReadOnlyList<GameObject> Spawned => spawned;
        public bool HasStarted => startTime >= 0f;

        public void Launch()
        {
            if (HasStarted)
                return;
            startTime = Time.time;
            done = new bool[units.Count];
        }

        private void Start()
        {
            progress = mission != null ? mission.Progress : null;
            if (progress == null)
                return;
            progress.ObjectiveChanged += OnObjectiveChanged;
            foreach (var status in progress.Objectives)
                if (status.Objective.Id == objectiveId && status.State == ObjectiveState.Active)
                    Launch();
        }

        private void OnDestroy()
        {
            if (progress != null)
                progress.ObjectiveChanged -= OnObjectiveChanged;
        }

        private void OnObjectiveChanged(ObjectiveStatus status)
        {
            if (status.Objective.Id == objectiveId && status.State == ObjectiveState.Active)
                Launch();
        }

        private void Update()
        {
            if (!HasStarted || spawner == null || (progress != null && progress.Outcome != MissionOutcome.Running))
                return;
            var elapsed = Time.time - startTime;
            for (var i = 0; i < units.Count; i++)
            {
                if (done[i] || elapsed < units[i].Delay)
                    continue;
                done[i] = true;
                Spawn(units[i]);
            }
        }

        private void Spawn(EnemyWaveUnit unit)
        {
            if (unit.Prefab == null || unit.SpawnPoint == null)
                return;
            Vector2 from = unit.SpawnPoint.position;
            var angle = unit.SpawnPoint.eulerAngles.z;
            if (unit.Destination != null)
            {
                var heading = (Vector2)unit.Destination.position - from;
                if (heading.sqrMagnitude > 0.01f)
                    angle = Vector2.SignedAngle(Vector2.up, heading);
            }

            var spawnedUnit = spawner.Spawn(unit.Prefab, from, angle, null, MarkAsTarget);
            if (spawnedUnit == null)
                return;
            spawned.Add(spawnedUnit);

            if (unit.Loadout != null && spawnedUnit.TryGetComponent<TankController>(out var tank))
                tank.Apply(unit.Loadout);
            if (unit.Destination == null)
                return;
            var orders = new Order[] { new MoveToOrder(unit.Destination.position), new HoldOrder() };
            if (spawnedUnit.TryGetComponent<EnemyTankAI>(out var tankAi))
                tankAi.SetOrders(orders, false);
            else if (spawnedUnit.TryGetComponent<EnemyInfantryAI>(out var soldierAi))
                soldierAi.SetOrders(orders, false);
        }

        private void MarkAsTarget(GameObject unit)
        {
            if (!string.IsNullOrEmpty(targetObjectiveId))
                unit.AddComponent<MissionTarget>().Assign(targetObjectiveId);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.95f, 0.3f, 0.2f, 0.8f);
            foreach (var unit in units)
            {
                if (unit == null || unit.SpawnPoint == null)
                    continue;
                Gizmos.DrawWireSphere(unit.SpawnPoint.position, 1.5f);
                if (unit.Destination != null)
                    Gizmos.DrawLine(unit.SpawnPoint.position, unit.Destination.position);
            }
        }
    }
}
