using System.Collections;
using NUnit.Framework;
using PolishTanks.Prototypes;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    public class ExtrudedBuildingViewTests
    {
        private static readonly Vector2 Size = new Vector2(4f, 4f);

        private BuildingRig rig;
        private SpriteRenderer roof;
        private MeshFilter walls;

        [SetUp]
        public void SetUp()
        {
            rig = new BuildingRig();
            roof = rig.CreateRoof(Vector3.zero);
            walls = rig.CreateWalls();
        }

        [TearDown]
        public void TearDown() => rig.Dispose();

        private Transform CreateTarget(Vector2 position)
        {
            var target = rig.Track(new GameObject("Target"));
            target.transform.position = position;
            return target.transform;
        }

        [UnityTest]
        public IEnumerator Roof_CameraOffsetFromBuilding_LeansAwayFromCamera()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 5f);
            var view = rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            Assert.That(roof.transform.position.x, Is.GreaterThan(10f));
            Assert.That(view.RoofOffset.x, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator Roof_TallerBuilding_LeansFurther()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 2f);
            var view = rig.CreateExtrudedView(building, roof, walls);
            yield return null;
            var lowOffset = view.RoofOffset.magnitude;

            building.Configure(Size, 8f);
            yield return null;

            Assert.That(view.RoofOffset.magnitude, Is.GreaterThan(lowOffset));
        }

        [UnityTest]
        public IEnumerator Roof_ZeroHeight_StaysOnFootprint()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 0f);
            rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            Assert.That(roof.transform.position.x, Is.EqualTo(10f).Within(1e-4f));
        }

        [UnityTest]
        public IEnumerator Roof_Size_MatchesBuilding()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), new Vector2(6f, 3f), 5f);
            rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            Assert.That(roof.size, Is.EqualTo(new Vector2(6f, 3f)));
        }

        [UnityTest]
        public IEnumerator Walls_BuildingOnAxis_ShowsOneWall()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 5f);
            rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            Assert.That(walls.sharedMesh.vertexCount, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator Walls_BuildingOffDiagonal_ShowsTwoWalls()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 10f), Size, 5f);
            rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            Assert.That(walls.sharedMesh.vertexCount, Is.EqualTo(8));
        }

        [UnityTest]
        public IEnumerator Walls_CameraExactlyAboveCenter_ShowsNoWalls()
        {
            rig.CreateMainCamera(new Vector2(10f, 10f));
            var building = rig.CreateBuilding(new Vector2(10f, 10f), Size, 5f);
            rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            Assert.That(walls.sharedMesh.vertexCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Walls_RotatedBuilding_TopEdgeMeetsRoofCorners()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 3f), new Vector2(6f, 2f), 5f);
            walls.transform.SetPositionAndRotation(building.transform.position, Quaternion.Euler(0f, 0f, 90f));
            rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            var half = new Vector2(3f, 1f);
            var roofCenter = (Vector2)roof.transform.position;
            Vector2[] roofCorners =
            {
                roofCenter + (Vector2)(walls.transform.rotation * new Vector2(-half.x, -half.y)),
                roofCenter + (Vector2)(walls.transform.rotation * new Vector2(half.x, -half.y)),
                roofCenter + (Vector2)(walls.transform.rotation * new Vector2(half.x, half.y)),
                roofCenter + (Vector2)(walls.transform.rotation * new Vector2(-half.x, half.y)),
            };
            var vertices = walls.sharedMesh.vertices;
            var worstGap = vertices.Length == 0 ? float.PositiveInfinity : 0f;
            for (var i = 0; i < vertices.Length; i++)
            {
                if (i % 4 < 2)
                    continue;
                var world = (Vector2)walls.transform.TransformPoint(vertices[i]);
                var nearest = float.PositiveInfinity;
                foreach (var corner in roofCorners)
                    nearest = Mathf.Min(nearest, Vector2.Distance(world, corner));
                worstGap = Mathf.Max(worstGap, nearest);
            }

            Assert.That(worstGap, Is.LessThan(1e-3f));
        }

        [UnityTest]
        public IEnumerator IsOccluding_TargetBetweenBaseAndRoof_IsTrue()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 5f);
            var view = rig.CreateExtrudedView(building, roof, walls, CreateTarget(new Vector2(12.5f, 0f)));

            yield return null;

            Assert.That(view.IsOccluding, Is.True);
        }

        [UnityTest]
        public IEnumerator IsOccluding_TargetFarAway_IsFalse()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 5f);
            var view = rig.CreateExtrudedView(building, roof, walls, CreateTarget(new Vector2(-20f, 0f)));

            yield return null;

            Assert.That(view.IsOccluding, Is.False);
        }

        [UnityTest]
        public IEnumerator IsOccluding_NoTarget_IsFalse()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 5f);
            var view = rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            Assert.That(view.IsOccluding, Is.False);
        }

        [UnityTest]
        public IEnumerator Alpha_WhileOccluding_DropsBelowOne()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 5f);
            var view = rig.CreateExtrudedView(building, roof, walls, CreateTarget(new Vector2(12.5f, 0f)));

            for (var i = 0; i < 5; i++)
                yield return null;

            Assert.That(view.Alpha, Is.LessThan(1f));
        }

        [UnityTest]
        public IEnumerator Alpha_AfterTargetLeaves_ReturnsToOne()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 5f);
            var target = CreateTarget(new Vector2(12.5f, 0f));
            var view = rig.CreateExtrudedView(building, roof, walls, target);
            for (var i = 0; i < 5; i++)
                yield return null;

            target.position = new Vector3(-20f, 0f, 0f);
            for (var i = 0; i < 600 && view.Alpha < 1f; i++)
                yield return null;

            Assert.That(view.Alpha, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator Update_WithoutMainCamera_LeavesRoofUntouched()
        {
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 5f);
            rig.CreateExtrudedView(building, roof, walls);

            yield return null;

            Assert.That(roof.transform.position, Is.EqualTo(Vector3.zero));
        }
    }
}
