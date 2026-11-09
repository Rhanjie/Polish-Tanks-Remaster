using System.Collections;
using NUnit.Framework;
using PolishTanks.Prototypes;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class FlatBuildingViewTests
    {
        private BuildingRig rig;
        private Building building;
        private SpriteRenderer roof;

        [SetUp]
        public void SetUp()
        {
            rig = new BuildingRig();
            building = rig.CreateBuilding(new Vector2(7f, -3f), new Vector2(6f, 3f), 4f);
            roof = rig.CreateRoof(new Vector3(100f, 100f, 0f));
            var go = rig.Track(new GameObject("FlatView"));
            go.SetActive(false);
            var view = go.AddComponent<FlatBuildingView>();
            TankRig.SetField(view, "building", building);
            TankRig.SetField(view, "roof", roof);
            go.SetActive(true);
        }

        [TearDown]
        public void TearDown() => rig.Dispose();

        [UnityTest]
        public IEnumerator Start_Runs_PlacesRoofOnFootprint()
        {
            yield return null;

            Assert.That((Vector2)roof.transform.position, Is.EqualTo(building.Footprint.center));
        }

        [UnityTest]
        public IEnumerator Start_Runs_RoofSizeEqualsBuildingSize()
        {
            yield return null;

            Assert.That(roof.size, Is.EqualTo(building.Size));
        }
    }
}
