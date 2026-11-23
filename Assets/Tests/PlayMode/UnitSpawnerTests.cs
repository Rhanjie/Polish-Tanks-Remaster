using System.Collections;
using NUnit.Framework;
using PolishTanks.Missions;
using PolishTanks.Radio;
using PolishTanks.Spawning;
using Reflex.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// <see cref="UnitSpawner"/> and <see cref="EnemyWave"/>: a spawned unit has its services before it wakes up and is
    /// tracked by the mission, and a wave waits for its objective and each unit's delay before marking units as targets.
    /// </summary>
    public class UnitSpawnerTests
    {
        private Container container;
        private GameObject staging;
        private GameObject template;
        private FakeMission mission;
        private UnitSpawner spawner;
        private GameObject extra;

        [SetUp]
        public void SetUp()
        {
            staging = new GameObject("Staging");
            staging.SetActive(false);
            template = new GameObject("Unit");
            template.AddComponent<InjectionProbe>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var unit in Object.FindObjectsByType<InjectionProbe>(FindObjectsInactive.Include))
                Object.Destroy(unit.gameObject);
            foreach (var wave in Object.FindObjectsByType<EnemyWave>(FindObjectsInactive.Include))
                Object.Destroy(wave.gameObject);
            if (extra != null)
                Object.Destroy(extra);
            Object.Destroy(staging);
            container?.Dispose();
        }

        private void Build(MissionProgress progress)
        {
            mission = new FakeMission(progress);
            container = null;
            spawner = new UnitSpawner(() => container, staging.transform, mission);
            container = new ContainerBuilder()
                .RegisterValue(new RadioChannel())
                .RegisterValue(mission, new[] { typeof(IMission) })
                .RegisterValue(spawner, new[] { typeof(IUnitSpawner) })
                .Build();
        }

        [Test]
        public void Spawn_UnitWithInjectedFields_WakesUpWithItsServicesAndIsTracked()
        {
            Build(null);
            extra = new GameObject("Parent");
            var parent = extra.transform;

            var unit = spawner.Spawn(template, new Vector2(3f, 4f), 90f, parent);

            Assert.That(unit.GetComponent<InjectionProbe>().InjectedBeforeAwake, Is.True);
            Assert.That(unit.activeInHierarchy, Is.True);
            Assert.That(unit.transform.parent, Is.SameAs(parent));
            Assert.That((Vector2)unit.transform.position, Is.EqualTo(new Vector2(3f, 4f)));
            Assert.That(mission.Tracked, Is.EquivalentTo(new[] { unit }));
        }

        [UnityTest]
        public IEnumerator Update_ObjectiveStarts_SpawnsEachUnitAfterItsDelayAsATargetOfTheObjective()
        {
            var progress = new MissionProgress(new[]
            {
                new MissionObjective("drive", MissionObjectiveKind.Reach, "Drive"),
                new MissionObjective("attack", MissionObjectiveKind.Destroy, "Attack", targetCount: 2),
            }, default);
            Build(progress);
            extra = new GameObject("Spawn");
            var spawnPoint = extra.transform;
            var waveObject = new GameObject("Wave");
            waveObject.SetActive(false);
            var wave = waveObject.AddComponent<EnemyWave>();
            TankRig.SetField(wave, "objectiveId", "attack");
            TankRig.SetField(wave, "targetObjectiveId", "attack");
            TankRig.SetField(wave, "units", new System.Collections.Generic.List<EnemyWaveUnit>
            {
                new EnemyWaveUnit(template, spawnPoint, null),
                new EnemyWaveUnit(template, spawnPoint, null, 0.5f),
            });
            Reflex.Injectors.GameObjectInjector.InjectRecursive(waveObject, container);
            waveObject.SetActive(true);
            yield return null;

            var beforeObjective = wave.Spawned.Count;
            progress.Reached("drive");
            yield return null;
            var rightAway = wave.Spawned.Count;
            yield return new WaitForSeconds(0.6f);

            Assert.That(beforeObjective, Is.Zero);
            Assert.That(rightAway, Is.EqualTo(1));
            Assert.That(wave.Spawned, Has.Count.EqualTo(2));
            foreach (var unit in wave.Spawned)
                Assert.That(unit.GetComponent<MissionTarget>().ObjectiveId, Is.EqualTo("attack"));
        }
    }
}
