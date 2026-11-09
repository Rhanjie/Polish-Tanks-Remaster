using NUnit.Framework;
using PolishTanks.Prototypes;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    public class BuildingTests
    {
        private BuildingRig rig;

        [SetUp]
        public void SetUp() => rig = new BuildingRig();

        [TearDown]
        public void TearDown() => rig.Dispose();

        [Test]
        public void Configure_NewSize_ResizesCollider()
        {
            var building = rig.CreateBuilding(Vector2.zero, new Vector2(4f, 4f), 3f);

            building.Configure(new Vector2(6f, 2f), 3f);

            Assert.That(building.GetComponent<BoxCollider2D>().size, Is.EqualTo(new Vector2(6f, 2f)));
        }

        [Test]
        public void Configure_Called_ResetsColliderOffset()
        {
            var building = rig.CreateBuilding(Vector2.zero, new Vector2(4f, 4f), 3f);
            building.GetComponent<BoxCollider2D>().offset = new Vector2(1f, 1f);

            building.Configure(new Vector2(4f, 4f), 3f);

            Assert.That(building.GetComponent<BoxCollider2D>().offset, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Footprint_AtPosition_IsCenteredOnIt()
        {
            var building = rig.CreateBuilding(new Vector2(3f, 4f), new Vector2(4f, 2f), 3f);

            Assert.That(building.Footprint.center, Is.EqualTo(new Vector2(3f, 4f)));
            Assert.That(building.Footprint.size, Is.EqualTo(new Vector2(4f, 2f)));
        }

        [Test]
        public void Footprint_AfterMoving_FollowsTransform()
        {
            var building = rig.CreateBuilding(Vector2.zero, new Vector2(2f, 2f), 3f);

            building.transform.position = new Vector3(10f, -5f, 0f);

            Assert.That(building.Footprint.center, Is.EqualTo(new Vector2(10f, -5f)));
        }
    }
}
