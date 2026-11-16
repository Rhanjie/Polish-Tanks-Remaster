using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PolishTanks.Ground;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.TestTools;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Tests that <see cref="GroundSurface"/> reports the painted layer under a point and puts vehicle blocking
    /// colliders only over cells whose layer blocks vehicles.
    /// </summary>
    public class GroundSurfaceTests
    {
        private const int WaterLayer = 1;
        private const int BlockingPhysicsLayer = 4;

        private GameObject root;
        private GroundSurface surface;
        private GroundLayerSet layerSet;
        private GroundTile waterTile;

        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        [SetUp]
        public void SetUp()
        {
            layerSet = ScriptableObject.CreateInstance<GroundLayerSet>();
            SetField(layerSet, "layers", new List<GroundLayer>
            {
                new GroundLayer("Grass", 4f, false),
                new GroundLayer("Water", 4f, true),
            });

            waterTile = ScriptableObject.CreateInstance<GroundTile>();
            SetField(waterTile, "layer", WaterLayer);

            root = new GameObject("Ground Test Root");
            root.AddComponent<Grid>().cellSize = Vector3.one;
            var tilemapObject = new GameObject("Ground Tilemap");
            tilemapObject.transform.SetParent(root.transform, false);
            var tilemap = tilemapObject.AddComponent<Tilemap>();
            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                tilemap.SetTile(new Vector3Int(x, y, 0), waterTile);

            var surfaceObject = new GameObject("Ground Surface");
            surfaceObject.SetActive(false);
            surfaceObject.transform.SetParent(root.transform, false);
            surface = surfaceObject.AddComponent<GroundSurface>();
            SetField(surface, "tilemap", tilemap);
            SetField(surface, "layers", layerSet);
            surfaceObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(layerSet);
            Object.DestroyImmediate(waterTile);
        }

        [Test]
        public void LayerAt_PaintedAndUnpaintedPoints_ReturnWaterAndFallback()
        {
            Assert.AreEqual(WaterLayer, surface.LayerAt(new Vector2(1.5f, 1.5f)));
            Assert.AreEqual(0, surface.LayerAt(new Vector2(8.5f, 8.5f)));
        }

        [UnityTest]
        public IEnumerator Blocking_WaterCells_HaveColliderOnBlockingLayerAndGrassHasNone()
        {
            yield return new WaitForFixedUpdate();
            var mask = 1 << BlockingPhysicsLayer;

            Assert.IsNotNull(Physics2D.OverlapPoint(new Vector2(1.5f, 1.5f), mask));
            Assert.IsNull(Physics2D.OverlapPoint(new Vector2(8.5f, 8.5f), mask));
        }
    }
}
