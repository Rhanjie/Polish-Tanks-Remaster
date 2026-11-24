using System.Collections.Generic;
using PolishTanks.Damage;
using PolishTanks.Infantry;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// Runs the scene's <see cref="MissionDefinition"/> through a <see cref="MissionProgress"/>. On start it counts the
    /// <see cref="MissionTarget"/>s of each Destroy objective and starts tracking every enemy tank and soldier in the
    /// scene, so their losses pay respect; units spawned later are added with <see cref="Track"/>. Losing the player's
    /// tank loses the mission. The scene installer binds it as <see cref="IMission"/>.
    /// </summary>
    public class MissionDirector : MonoBehaviour, IMission
    {
        [SerializeField] private MissionDefinition definition;

        [Inject] private PlayerTank playerTank;

        private readonly HashSet<GameObject> tracked = new HashSet<GameObject>();
        private readonly HashSet<(ModuleDamage, PartId)> brokenParts = new HashSet<(ModuleDamage, PartId)>();
        private TankController player;

        public MissionDefinition Definition => definition;

        public MissionProgress Progress { get; private set; }

        public void Track(GameObject unit)
        {
            if (unit == null || Progress == null || !tracked.Add(unit))
                return;

            var tank = unit.GetComponent<TankController>();
            if (tank != null)
            {
                if (tank == player)
                    return;
                tank.Destroyed += OnTankDestroyed;
                var damage = unit.GetComponent<ModuleDamage>();
                if (damage != null)
                    damage.PartChanged += (part, condition) => OnPartChanged(tank, damage, part, condition);
                return;
            }

            var soldier = unit.GetComponent<InfantryHealth>();
            if (soldier != null)
                soldier.Died += OnSoldierDied;
        }

        private void Awake()
        {
            if (definition != null)
                Progress = new MissionProgress(definition.Objectives, definition.Respect);
        }

        private void Start()
        {
            if (Progress == null)
                return;

            if (playerTank != null)
            {
                player = playerTank.Controller;
                player.Destroyed += OnPlayerDestroyed;
            }

            var targetCounts = new Dictionary<string, int>();
            foreach (var target in FindObjectsByType<MissionTarget>(FindObjectsInactive.Include))
                if (!string.IsNullOrEmpty(target.ObjectiveId))
                    targetCounts[target.ObjectiveId] = targetCounts.TryGetValue(target.ObjectiveId, out var count) ? count + 1 : 1;
            foreach (var pair in targetCounts)
                Progress.SetSceneTargets(pair.Key, pair.Value);

            foreach (var tank in FindObjectsByType<TankController>())
                Track(tank.gameObject);
            foreach (var soldier in FindObjectsByType<InfantryHealth>())
                Track(soldier.gameObject);
        }

        private void OnDestroy()
        {
            if (player != null)
                player.Destroyed -= OnPlayerDestroyed;
        }

        private void Update() => Progress?.Tick(Time.deltaTime);

        private void OnPlayerDestroyed(TankController _) => Progress.Fail();

        private void OnTankDestroyed(TankController tank)
        {
            tank.Destroyed -= OnTankDestroyed;
            Progress.EnemyLost(EnemyLoss.Tank);
        }

        private void OnPartChanged(TankController tank, ModuleDamage damage, PartId part, PartCondition condition)
        {
            if (condition == PartCondition.Destroyed && !tank.IsDestroyed && brokenParts.Add((damage, part)))
                Progress.EnemyLost(EnemyLoss.Module);
        }

        private void OnSoldierDied(InfantryHealth soldier)
        {
            soldier.Died -= OnSoldierDied;
            Progress.EnemyLost(EnemyLoss.Infantry);
        }
    }
}
