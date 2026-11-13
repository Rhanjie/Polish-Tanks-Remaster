using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// Ground shadow of a building: the footprint swept along the sun direction by its height.
    /// Rebuilt only when the building, its rotation or the sun settings change. The sun direction is in world space.
    /// Runs in the editor too, so the scene view shows it while laying out a map.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BuildingShadow : MonoBehaviour
    {
        [SerializeField] private Building building;
        [SerializeField] private Vector2 sunDirection = Rendering.SunLight.Direction;
        [SerializeField, Min(0f)] private float shadowLength = 0.45f;
        [SerializeField] private Color color = new Color(0f, 0f, 0f, 0.35f);

        private readonly List<Vector2> hull = new List<Vector2>(8);
        private readonly List<Vector3> vertices = new List<Vector3>(8);
        private readonly List<Color> colors = new List<Color>(8);
        private readonly List<int> triangles = new List<int>(18);
        private Mesh mesh;
        private Vector2 builtSize;
        private float builtHeight = -1f;
        private Vector2 builtSun;
        private float builtLength = -1f;
        private Color builtColor;
        private Quaternion builtRotation;

        private void OnEnable()
        {
            if (building == null)
                building = GetComponentInParent<Building>();
            mesh = new Mesh { name = "Building Shadow", hideFlags = HideFlags.DontSave };
            mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = mesh;
            Rebuild();
        }

        private void OnDisable()
        {
            if (mesh == null)
                return;
            if (Application.isPlaying)
                Destroy(mesh);
            else
                DestroyImmediate(mesh);
        }

        private void LateUpdate()
        {
            if (building != null && IsOutdated())
                Rebuild();
        }

        private bool IsOutdated()
        {
            return building.Size != builtSize
                || !Mathf.Approximately(building.Height, builtHeight)
                || sunDirection != builtSun
                || !Mathf.Approximately(shadowLength, builtLength)
                || color != builtColor
                || transform.rotation != builtRotation;
        }

        private void Rebuild()
        {
            if (building == null || mesh == null)
                return;

            var local = new Rect(-building.Size * 0.5f, building.Size);
            Vector2 offset = transform.InverseTransformVector(HeightProjection.ShadowOffset(sunDirection, building.Height, shadowLength));
            HeightProjection.ExtrudedRectHull(local, offset, hull);

            vertices.Clear();
            colors.Clear();
            triangles.Clear();
            foreach (var p in hull)
            {
                vertices.Add(p);
                colors.Add(color);
            }
            for (var i = 1; i < hull.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();

            builtSize = building.Size;
            builtHeight = building.Height;
            builtSun = sunDirection;
            builtLength = shadowLength;
            builtColor = color;
            builtRotation = transform.rotation;
        }
    }
}
