using NUnit.Framework;
using PolishTanks.Prototypes;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    public class BuildingShadowTests
    {
        private BuildingRig rig;

        [SetUp]
        public void SetUp() => rig = new BuildingRig();

        [TearDown]
        public void TearDown() => rig.Dispose();

        private Mesh CreateShadowMesh(float height)
        {
            var building = rig.CreateBuilding(Vector2.zero, new Vector2(4f, 4f), height);
            var go = rig.Track(new GameObject("Shadow"));
            go.transform.SetParent(building.transform, false);
            go.AddComponent<BuildingShadow>();
            return go.GetComponent<MeshFilter>().sharedMesh;
        }

        [Test]
        public void Mesh_TallBuilding_HasAtLeastFourVertices()
        {
            var mesh = CreateShadowMesh(3f);

            Assert.That(mesh.vertexCount, Is.GreaterThanOrEqualTo(4));
        }

        [Test]
        public void Bounds_TallBuilding_ShiftTowardSun()
        {
            var mesh = CreateShadowMesh(3f);

            Assert.That(mesh.bounds.center.x, Is.GreaterThan(0f));
            Assert.That(mesh.bounds.center.y, Is.LessThan(0f));
        }

        [Test]
        public void Bounds_ZeroHeight_StayCenteredOnFootprint()
        {
            var mesh = CreateShadowMesh(0f);

            Assert.That(mesh.bounds.center.magnitude, Is.EqualTo(0f).Within(1e-4f));
        }
    }
}
