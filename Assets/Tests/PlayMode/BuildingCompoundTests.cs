using System.Collections;
using NUnit.Framework;
using PolishTanks.Prototypes;
using UnityEngine;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Views of blocks inside a building compound: shared lean, shared sorting step, shared fading and gable roofs.
    /// </summary>
    public class BuildingCompoundTests
    {
        private static readonly Vector2 Size = new Vector2(4f, 4f);

        private BuildingRig rig;

        [SetUp]
        public void SetUp() => rig = new BuildingRig();

        [TearDown]
        public void TearDown() => rig.Dispose();

        private ExtrudedBuildingView CreateView(Building building, out MeshRenderer wallRenderer, Transform target = null)
        {
            var walls = rig.CreateWalls();
            wallRenderer = walls.gameObject.AddComponent<MeshRenderer>();
            return rig.CreateExtrudedView(building, rig.CreateRoof(Vector3.zero), walls, target);
        }

        [UnityTest]
        public IEnumerator RoofOffset_EqualHeightBlocksInCompound_AreIdentical()
        {
            rig.CreateMainCamera(Vector2.zero);
            var compound = rig.CreateCompound(new Vector2(10f, 0f));
            var left = CreateView(rig.CreateBlock(compound, new Vector2(-2f, 0f), Size, 4f), out _);
            var right = CreateView(rig.CreateBlock(compound, new Vector2(2f, 0f), Size, 4f), out _);

            yield return null;

            Assert.That(left.RoofOffset, Is.EqualTo(right.RoofOffset));
        }

        [UnityTest]
        public IEnumerator RoofOffset_EqualHeightBlocksOutsideCompound_Differ()
        {
            rig.CreateMainCamera(Vector2.zero);
            var left = CreateView(rig.CreateBuilding(new Vector2(8f, 0f), Size, 4f), out _);
            var right = CreateView(rig.CreateBuilding(new Vector2(12f, 0f), Size, 4f), out _);

            yield return null;

            Assert.That(left.RoofOffset, Is.Not.EqualTo(right.RoofOffset));
        }

        [UnityTest]
        public IEnumerator SortingOrder_TallerBlockInCompound_DrawsAboveLowerWithinSameStep()
        {
            rig.CreateMainCamera(Vector2.zero);
            var compound = rig.CreateCompound(new Vector2(10f, 0f));
            CreateView(rig.CreateBlock(compound, new Vector2(-2f, 0f), Size, 2f), out var lowWalls);
            CreateView(rig.CreateBlock(compound, new Vector2(2f, 0f), Size, 5f), out var highWalls);

            yield return null;

            Assert.That(highWalls.sortingOrder - lowWalls.sortingOrder, Is.InRange(1, HeightProjection.BuildingSortingStride - 1));
        }

        [UnityTest]
        public IEnumerator IsOccluding_TargetHiddenByOneBlock_FadesWholeCompound()
        {
            rig.CreateMainCamera(Vector2.zero);
            var target = rig.Track(new GameObject("Target")).transform;
            target.position = new Vector2(4f, 0f);
            var compound = rig.CreateCompound(new Vector2(10f, 0f));
            var covering = CreateView(rig.CreateBlock(compound, new Vector2(-6f, 0f), Size, 4f), out _, target);
            var distant = CreateView(rig.CreateBlock(compound, new Vector2(6f, 0f), Size, 4f), out _, target);

            yield return null;

            Assert.That(covering.IsOccluding && distant.IsOccluding, Is.True);
        }

        [UnityTest]
        public IEnumerator Roof_GableBlock_ReplacesSpriteWithMesh()
        {
            rig.CreateMainCamera(Vector2.zero);
            var building = rig.CreateBuilding(new Vector2(10f, 0f), Size, 3f);
            building.ConfigureRoof(RoofShape.Gable, 2f);
            var roof = rig.CreateRoof(Vector3.zero);
            var gable = rig.CreateGableRoof();
            rig.CreateExtrudedView(building, roof, rig.CreateWalls(), null, gable);

            yield return null;

            Assert.That(!roof.enabled && gable.sharedMesh.vertexCount > 0, Is.True);
        }
    }
}
