using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Background
{
    /// <summary>
    /// Base for components that generate their mesh from Inspector settings. The mesh is runtime-only
    /// (never saved into the scene) and rebuilt on enable and after every Inspector change, also in Edit Mode.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class ProceduralMeshBehaviour : MonoBehaviour
    {
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int> _triangles = new List<int>();
        private readonly List<Color> _colors = new List<Color>();
        private Mesh _mesh;
        private bool _dirty;

        public Mesh GeneratedMesh => _mesh;

        protected abstract void Build(List<Vector3> vertices, List<int> triangles, List<Color> colors);

        protected virtual void OnEnable() => Rebuild();

        protected virtual void OnDisable() => ReleaseMesh();

        // Mesh changes are not allowed during OnValidate, so the rebuild waits for the next Update.
        protected virtual void OnValidate() => _dirty = true;

        protected virtual void Update()
        {
            if (_dirty)
                Rebuild();
        }

        public void Rebuild()
        {
            _dirty = false;
            if (_mesh == null)
                _mesh = new Mesh { name = $"{GetType().Name} ({name})", hideFlags = HideFlags.HideAndDontSave };

            _vertices.Clear();
            _triangles.Clear();
            _colors.Clear();
            Build(_vertices, _triangles, _colors);

            // Vertex colours reach the shader untouched, unlike SpriteRenderer.color, so in a Linear project
            // the sRGB values picked in the Inspector have to be converted here or everything looks washed out.
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                for (var i = 0; i < _colors.Count; i++)
                {
                    var c = _colors[i];
                    _colors[i] = new Color(Mathf.GammaToLinearSpace(c.r), Mathf.GammaToLinearSpace(c.g), Mathf.GammaToLinearSpace(c.b), c.a);
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        protected static void FillColor(List<Color> colors, int vertexCount, Color color)
        {
            while (colors.Count < vertexCount)
                colors.Add(color);
        }

        private void ReleaseMesh()
        {
            if (_mesh == null)
                return;

            var filter = GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh == _mesh)
                filter.sharedMesh = null;

            if (Application.isPlaying)
                Destroy(_mesh);
            else
                DestroyImmediate(_mesh);
            _mesh = null;
        }
    }
}
