using System.Collections;
using NUnit.Framework;
using PolishTanks.Missions;
using PolishTanks.Radio;
using PolishTanks.Tank;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary><see cref="RadioZone"/>: it waits for its objective, then plays its message once however often the tank drives in.</summary>
    public class RadioZoneTests
    {
        private TankRig rig;
        private Container container;
        private RadioChannel radio;
        private MissionProgress progress;
        private TankRig.BuiltTank tank;
        private GameObject zoneObject;

        [SetUp]
        public void SetUp()
        {
            rig = new TankRig();
            radio = new RadioChannel(1000f);
            progress = new MissionProgress(new[]
            {
                new MissionObjective("drive", MissionObjectiveKind.Reach, "Drive"),
                new MissionObjective("fight", MissionObjectiveKind.Destroy, "Fight"),
            }, default);
            tank = rig.BuildTank("Tank", null, Vector2.zero);
            container = new ContainerBuilder()
                .RegisterValue(radio)
                .RegisterValue(new PlayerTank(tank.Controller))
                .RegisterValue(new FakeMission(progress), new[] { typeof(IMission) })
                .Build();

            zoneObject = new GameObject("Zone");
            zoneObject.SetActive(false);
            var zone = zoneObject.AddComponent<RadioZone>();
            var sequence = ScriptableObject.CreateInstance<RadioSequence>();
            TankRig.SetField(sequence, "messages", new System.Collections.Generic.List<RadioMessage> { new RadioMessage(null, "Contact") });
            TankRig.SetField(zone, "sequence", sequence);
            TankRig.SetField(zone, "objectiveId", "fight");
            GameObjectInjector.InjectRecursive(zoneObject, container);
            zoneObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(zoneObject);
            container.Dispose();
            rig.Dispose();
        }

        private void MoveTank(Vector2 position)
        {
            tank.Root.GetComponent<Rigidbody2D>().position = position;
            tank.Root.transform.position = position;
        }

        [UnityTest]
        public IEnumerator Update_TankInsideBeforeAndAfterItsObjectiveStarts_PlaysOnlyOnceTheObjectiveIsOn()
        {
            yield return null;
            var beforeObjective = radio.Current;

            progress.Reached("drive");
            yield return null;
            var firstMessage = radio.Current;
            radio.Confirm();
            radio.Confirm();
            MoveTank(new Vector2(20f, 0f));
            yield return null;
            MoveTank(Vector2.zero);
            yield return null;

            Assert.That(beforeObjective, Is.Null);
            Assert.That(firstMessage, Is.Not.Null);
            Assert.That(firstMessage.Text, Is.EqualTo("Contact"));
            Assert.That(radio.Current, Is.Null);
        }
    }
}
