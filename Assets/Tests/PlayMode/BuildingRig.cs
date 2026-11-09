using System;
using System.Collections.Generic;
using PolishTanks.Prototypes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Builds buildings, views, a main camera and roof sprites in code for the view prototype tests.
    /// Views are created inactive so serialized fields are set before OnEnable runs. Dispose destroys
    /// everything immediately, so no MainCamera-tagged object outlives a test.
    /// </summary>
    internal sealed class BuildingRig : IDisposable
    {
        private readonly List<Object> created = new List<Object>();

        public T Track<T>(T obj) where T : Object
        {
            created.Add(obj);
            return obj;
        }

        public Building CreateBuilding(Vector2 position, Vector2 size, float height)
        {
            var go = Track(new GameObject("Building"));
            go.transform.position = position;
            var building = go.AddComponent<Building>();
            building.Configure(size, height);
            return building;
        }

        public Camera CreateMainCamera(Vector2 position)
        {
            var go = Track(new GameObject("TestMainCamera"));
            go.tag = "MainCamera";
            go.transform.position = new Vector3(position.x, position.y, -10f);
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            return camera;
        }

        public SpriteRenderer CreateRoof(Vector3 position)
        {
            var texture = Track(new Texture2D(1, 1));
            var sprite = Track(Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f)));
            var go = Track(new GameObject("Roof"));
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            return renderer;
        }

        public MeshFilter CreateWalls() => Track(new GameObject("Walls")).AddComponent<MeshFilter>();

        public ExtrudedBuildingView CreateExtrudedView(Building building, SpriteRenderer roof, MeshFilter walls, Transform target = null)
        {
            var go = Track(new GameObject("ExtrudedView"));
            go.SetActive(false);
            var view = go.AddComponent<ExtrudedBuildingView>();
            TankRig.SetField(view, "building", building);
            TankRig.SetField(view, "roof", roof);
            TankRig.SetField(view, "walls", walls);
            if (target != null)
                view.SetOcclusionTarget(target);
            go.SetActive(true);
            return view;
        }

        public void Dispose()
        {
            foreach (var obj in created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            created.Clear();
        }
    }
}
