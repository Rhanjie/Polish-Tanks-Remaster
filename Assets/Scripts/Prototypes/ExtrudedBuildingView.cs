using System.Collections.Generic;
using PolishTanks.CameraRig;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// 2.5D top-down view of a building in the style of early GTA: the roof leans away from the
    /// camera center in proportion to height, walls are rebuilt every frame between footprint and
    /// roof, and the whole building fades when it hides the tracked target. The building may be rotated: the lean is
    /// worked out in world space and turned into the building's own space for the walls.
    /// Also runs in the editor so the scene shows real sizes; there the lean comes from the main camera.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(200)]
    public class ExtrudedBuildingView : MonoBehaviour
    {
        private static readonly Vector2[] EdgeNormals = { Vector2.down, Vector2.right, Vector2.up, Vector2.left };

        [SerializeField] private Building building;
        [SerializeField] private SpriteRenderer roof;
        [SerializeField] private MeshFilter walls;

        [Tooltip("How strongly height turns into lean. Roughly 1 / camera distance.")]
        [SerializeField, Min(0f)] private float perspective = 0.035f;

        [Tooltip("World units of wall per texture repeat, along the wall and up.")]
        [SerializeField] private Vector2 wallTextureSize = new Vector2(2f, 2f);

        [SerializeField] private Vector2 lightDirection = new Vector2(-0.6f, 0.8f);
        [SerializeField] private Color litWall = Color.white;
        [SerializeField] private Color shadedWall = new Color(0.55f, 0.55f, 0.6f);

        [Header("Occlusion")]
        [Tooltip("Object that makes the building fade when hidden by it. Defaults to the main camera's follow target.")]
        [SerializeField] private Transform occlusionTarget;
        [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.35f;
        [SerializeField, Min(0.01f)] private float fadeSpeed = 6f;

        private readonly List<Vector3> vertices = new List<Vector3>(16);
        private readonly List<Vector2> uvs = new List<Vector2>(16);
        private readonly List<Color> colors = new List<Color>(16);
        private readonly List<int> triangles = new List<int>(24);
        private readonly List<Vector2> hull = new List<Vector2>(8);
        private readonly Vector2[] corners = new Vector2[4];
        private readonly Vector2[] worldCorners = new Vector2[4];
        private Vector2 localRoofOffset;
        private Mesh mesh;
        [Inject] private IGameCamera gameCamera;
        private float alpha = 1f;

        public Vector2 RoofOffset { get; private set; }
        public bool IsOccluding { get; private set; }
        public float Alpha => alpha;

        public void SetOcclusionTarget(Transform target) => occlusionTarget = target;

        private void OnEnable()
        {
            if (building == null)
                building = GetComponentInParent<Building>();
            mesh = new Mesh { name = "Building Walls", hideFlags = HideFlags.DontSave };
            mesh.MarkDynamic();
            if (walls != null)
                walls.sharedMesh = mesh;
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
            if (building == null || roof == null || walls == null)
                return;
            if (!TryGetViewCenter(out var viewCenter))
                return;

            Vector2 basePosition = building.transform.position;
            RoofOffset = HeightProjection.RoofOffset(basePosition, viewCenter, building.Height, perspective);

            var roofPosition = new Vector3(basePosition.x + RoofOffset.x, basePosition.y + RoofOffset.y, roof.transform.position.z);
            if ((roof.transform.position - roofPosition).sqrMagnitude > 1e-6f)
                roof.transform.position = roofPosition;
            if (roof.size != building.Size)
                roof.size = building.Size;
            localRoofOffset = walls.transform.InverseTransformVector(RoofOffset);

            FillCorners();
            UpdateFade();
            BuildWalls();

            var roofColor = roof.color;
            if (!Mathf.Approximately(roofColor.a, alpha))
            {
                roofColor.a = alpha;
                roof.color = roofColor;
            }
        }

        private bool TryGetViewCenter(out Vector2 center)
        {
            if (gameCamera != null)
            {
                center = gameCamera.Position;
                return true;
            }
            var main = Application.isPlaying ? null : Camera.main;
            center = main != null ? (Vector2)main.transform.position : Vector2.zero;
            return main != null;
        }

        private void UpdateFade()
        {
            if (!Application.isPlaying)
            {
                IsOccluding = false;
                alpha = 1f;
                return;
            }
            IsOccluding = occlusionTarget != null && Covers(occlusionTarget.position);
            var target = IsOccluding ? fadedAlpha : 1f;
            alpha = Mathf.MoveTowards(alpha, target, fadeSpeed * Time.deltaTime);
        }

        private bool Covers(Vector2 point)
        {
            HeightProjection.ExtrudedQuadHull(worldCorners[0], worldCorners[1], worldCorners[2], worldCorners[3], RoofOffset, hull);
            for (var i = 0; i < hull.Count; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Count];
                if ((b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x) < 0f)
                    return false;
            }
            return hull.Count >= 3;
        }

        private void FillCorners()
        {
            var half = building.Size * 0.5f;
            corners[0] = new Vector2(-half.x, -half.y);
            corners[1] = new Vector2(half.x, -half.y);
            corners[2] = new Vector2(half.x, half.y);
            corners[3] = new Vector2(-half.x, half.y);
            for (var i = 0; i < 4; i++)
                worldCorners[i] = building.transform.TransformPoint(corners[i]);
        }

        private void BuildWalls()
        {
            vertices.Clear();
            uvs.Clear();
            colors.Clear();
            triangles.Clear();

            var light = lightDirection.normalized;
            for (var edge = 0; edge < 4; edge++)
            {
                var normal = EdgeNormals[edge];
                if (!HeightProjection.IsVisibleWall(normal, localRoofOffset))
                    continue;

                var a = corners[edge];
                var b = corners[(edge + 1) % 4];
                var length = Vector2.Distance(a, b) / wallTextureSize.x;
                var up = building.Height / wallTextureSize.y;
                var shade = Color.Lerp(shadedWall, litWall, Mathf.Clamp01(Vector2.Dot((Vector2)walls.transform.TransformDirection(normal), light) * 0.5f + 0.5f));
                shade.a = alpha;

                var start = vertices.Count;
                vertices.Add(a);
                vertices.Add(b);
                vertices.Add(b + localRoofOffset);
                vertices.Add(a + localRoofOffset);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(length, 0f));
                uvs.Add(new Vector2(length, up));
                uvs.Add(new Vector2(0f, up));
                for (var i = 0; i < 4; i++)
                    colors.Add(shade);
                triangles.Add(start);
                triangles.Add(start + 2);
                triangles.Add(start + 1);
                triangles.Add(start);
                triangles.Add(start + 3);
                triangles.Add(start + 2);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }
    }
}
