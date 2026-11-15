using System.Collections.Generic;
using PolishTanks.CameraRig;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// 2.5D top-down view of a building block in the style of early GTA: the roof leans away from the
    /// camera center in proportion to height, walls are rebuilt every frame between footprint and
    /// roof, and the whole building fades when it hides the tracked target. The building may be rotated: the lean is
    /// worked out in world space and turned into local space for the meshes.
    /// Inside a <see cref="BuildingCompound"/> the lean comes from the compound's anchor, walls are the segments the
    /// compound left visible, the sorting step is shared and the whole compound fades together. A standalone block
    /// leans from its own centre and lays out its walls in the walls object's space.
    /// Flat roofs use the roof sprite; gable roofs replace it with a sloped mesh whose ridge leans further than the eaves.
    /// Also runs in the editor so the scene shows real sizes; there the lean comes from the main camera.
    /// Buildings closer to the view centre lean over the ones further out, so walls and roof get a sorting order
    /// from <see cref="HeightProjection.BuildingSortingOrder"/> every frame instead of a fixed one. Every vertex and the roof
    /// sprite also get a depth from <see cref="HeightProjection.BuildingDepth"/>, so where a wall and a roof overlap the
    /// higher one wins even inside one compound.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(200)]
    public class ExtrudedBuildingView : MonoBehaviour
    {
        [SerializeField] private Building building;
        [SerializeField] private SpriteRenderer roof;
        [SerializeField] private MeshFilter walls;

        [Tooltip("Mesh used instead of the roof sprite when the building has a gable roof.")]
        [SerializeField] private MeshFilter gableRoof;

        [Tooltip("How strongly height turns into lean. Roughly 1 / camera distance.")]
        [SerializeField, Min(0f)] private float perspective = 0.035f;

        [Tooltip("World units of wall per texture repeat, along the wall and up. The height is one storey.")]
        [SerializeField] private Vector2 wallTextureSize = new Vector2(2f, 2f);

        [SerializeField] private Vector2 lightDirection = new Vector2(-0.6f, 0.8f);
        [SerializeField] private Color litWall = Color.white;
        [SerializeField] private Color shadedWall = new Color(0.55f, 0.55f, 0.6f);

        [Tooltip("Wall brightness where it meets the ground, a cheap contact shadow.")]
        [SerializeField, Range(0f, 1f)] private float groundShade = 0.72f;

        [Header("Gable roof")]
        [Tooltip("World units of roof per texture repeat, along the ridge and down the slope.")]
        [SerializeField] private Vector2 roofTextureSize = new Vector2(2f, 2f);
        [SerializeField, Min(0f)] private float eaveOverhang = 0.25f;
        [SerializeField, Min(0f)] private float ridgeCapWidth = 0.2f;
        [SerializeField] private Color litRoof = Color.white;
        [SerializeField] private Color shadedRoof = new Color(0.68f, 0.66f, 0.66f);
        [SerializeField] private Color ridgeCap = new Color(0.55f, 0.5f, 0.48f);

        [Header("Occlusion")]
        [Tooltip("Object that makes the building fade when hidden by it. Defaults to the main camera's follow target.")]
        [SerializeField] private Transform occlusionTarget;
        [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.35f;
        [SerializeField, Min(0.01f)] private float fadeSpeed = 6f;

        private readonly List<Vector3> vertices = new List<Vector3>(32);
        private readonly List<Vector2> uvs = new List<Vector2>(32);
        private readonly List<Color> colors = new List<Color>(32);
        private readonly List<int> triangles = new List<int>(48);
        private readonly List<Vector2> hull = new List<Vector2>(8);
        private readonly List<BlockShape> soloShapes = new List<BlockShape>(1);
        private readonly List<WallSegment> soloWalls = new List<WallSegment>(4);
        private readonly List<GableEnd> soloGableEnds = new List<GableEnd>(2);
        private readonly Vector2[] worldCorners = new Vector2[4];
        private BuildingCompound compound;
        private MeshRenderer wallRenderer;
        private MeshRenderer gableRenderer;
        private Mesh mesh;
        private Mesh roofMesh;
        private Vector2 leanPerUnit;
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
            compound = building != null ? building.GetComponentInParent<BuildingCompound>() : null;
            mesh = CreateMesh("Building Walls");
            if (walls != null)
                walls.sharedMesh = mesh;
            if (gableRoof != null)
            {
                roofMesh = CreateMesh("Building Gable Roof");
                gableRoof.sharedMesh = roofMesh;
                gableRenderer = gableRoof.GetComponent<MeshRenderer>();
            }
        }

        private void OnDisable()
        {
            DestroyMesh(mesh);
            DestroyMesh(roofMesh);
            mesh = null;
            roofMesh = null;
        }

        private void OnTransformParentChanged()
        {
            compound = building != null ? building.GetComponentInParent<BuildingCompound>() : null;
        }

        private static Mesh CreateMesh(string meshName)
        {
            var created = new Mesh { name = meshName, hideFlags = HideFlags.DontSave };
            created.MarkDynamic();
            return created;
        }

        private static void DestroyMesh(Mesh target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }

        private void LateUpdate()
        {
            if (building == null || roof == null || walls == null)
                return;
            if (!TryGetViewCenter(out var viewCenter))
                return;

            if (!Application.isPlaying)
                compound = building.GetComponentInParent<BuildingCompound>();
            var index = compound != null ? compound.IndexOf(building) : -1;
            var inCompound = index >= 0;
            Vector2 basePosition = building.transform.position;
            var anchor = inCompound ? compound.Anchor : basePosition;
            leanPerUnit = HeightProjection.RoofOffset(anchor, viewCenter, 1f, perspective);
            RoofOffset = leanPerUnit * building.Height;

            var roofPosition = new Vector3(basePosition.x + RoofOffset.x, basePosition.y + RoofOffset.y, HeightProjection.BuildingDepth(building.Height));
            if ((roof.transform.position - roofPosition).sqrMagnitude > 1e-6f)
                roof.transform.position = roofPosition;
            if (roof.size != building.Size)
                roof.size = building.Size;

            Transform space;
            BlockShape shape;
            IReadOnlyList<WallSegment> segments;
            IReadOnlyList<GableEnd> gableEnds;
            var ridgeExtensions = Vector2.zero;
            if (inCompound)
            {
                ridgeExtensions = compound.RidgeExtensionsOf(index);
                space = compound.transform;
                shape = compound.ShapeOf(index);
                segments = compound.WallsOf(index);
                gableEnds = compound.GableEndsOf(index);
            }
            else
            {
                space = walls.transform;
                shape = UpdateSoloLayout();
                segments = soloWalls;
                gableEnds = soloGableEnds;
            }

            var order = ApplySortingOrder(viewCenter, inCompound, inCompound ? compound.LayerOf(index) : 0);
            UpdateFade();
            BuildWalls(space, shape, segments, gableEnds);
            UpdateRoof(space, shape, ridgeExtensions, order + 1);
        }

        private BlockShape UpdateSoloLayout()
        {
            var shape = new BlockShape(new Rect(-building.Size * 0.5f, building.Size), building.Height, building.Roof, building.RidgeHeight);
            soloShapes.Clear();
            soloShapes.Add(shape);
            CompoundLayout.WallSegments(soloShapes, 0, soloWalls);
            CompoundLayout.GableEnds(soloShapes, 0, soloGableEnds);
            return shape;
        }

        private int ApplySortingOrder(Vector2 viewCenter, bool inCompound, int layer)
        {
            var distance = DistanceToFootprint(building, viewCenter);
            if (inCompound)
                foreach (var block in compound.Blocks)
                    distance = Mathf.Min(distance, DistanceToFootprint(block, viewCenter));

            var order = HeightProjection.BuildingSortingOrder(distance) + layer * 2;
            if (wallRenderer == null)
                wallRenderer = walls.GetComponent<MeshRenderer>();
            if (wallRenderer != null && wallRenderer.sortingOrder != order)
                wallRenderer.sortingOrder = order;
            if (roof.sortingOrder != order + 1)
                roof.sortingOrder = order + 1;
            return order;
        }

        private static float DistanceToFootprint(Building block, Vector2 point)
        {
            Vector2 local = block.transform.InverseTransformPoint(point);
            var half = block.Size * 0.5f;
            var nearest = new Vector2(Mathf.Clamp(local.x, -half.x, half.x), Mathf.Clamp(local.y, -half.y, half.y));
            return Vector2.Distance(point, block.transform.TransformPoint(nearest));
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
            IsOccluding = occlusionTarget != null && CoversTarget(occlusionTarget.position);
            var target = IsOccluding ? fadedAlpha : 1f;
            alpha = Mathf.MoveTowards(alpha, target, fadeSpeed * Time.deltaTime);
        }

        private bool CoversTarget(Vector2 point)
        {
            if (compound == null || compound.IndexOf(building) < 0)
                return Covers(building, point);
            foreach (var block in compound.Blocks)
                if (Covers(block, point))
                    return true;
            return false;
        }

        private bool Covers(Building block, Vector2 point)
        {
            var half = block.Size * 0.5f;
            var t = block.transform;
            worldCorners[0] = t.TransformPoint(new Vector3(-half.x, -half.y));
            worldCorners[1] = t.TransformPoint(new Vector3(half.x, -half.y));
            worldCorners[2] = t.TransformPoint(new Vector3(half.x, half.y));
            worldCorners[3] = t.TransformPoint(new Vector3(-half.x, half.y));
            HeightProjection.ExtrudedQuadHull(worldCorners[0], worldCorners[1], worldCorners[2], worldCorners[3], leanPerUnit * block.TopHeight, hull);
            for (var i = 0; i < hull.Count; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Count];
                if ((b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x) < 0f)
                    return false;
            }
            return hull.Count >= 3;
        }

        private void BuildWalls(Transform space, BlockShape shape, IReadOnlyList<WallSegment> segments, IReadOnlyList<GableEnd> gableEnds)
        {
            ClearBuffers();
            var target = walls.transform;
            var light = lightDirection.normalized;

            foreach (var segment in segments)
            {
                Vector2 normal = space.TransformDirection(segment.Normal);
                if (!HeightProjection.IsVisibleWall(normal, leanPerUnit))
                    continue;

                var shade = WallShade(normal, light);
                var groundA = segment.BottomA <= 0.001f ? groundShade : 1f;
                var groundB = segment.BottomB <= 0.001f ? groundShade : 1f;
                var u0 = WallU(shape, segment.Normal, segment.A);
                var u1 = WallU(shape, segment.Normal, segment.B);

                var start = vertices.Count;
                vertices.Add(ToLocal(target, space, segment.A, segment.BottomA));
                vertices.Add(ToLocal(target, space, segment.B, segment.BottomB));
                vertices.Add(ToLocal(target, space, segment.B, segment.Top));
                vertices.Add(ToLocal(target, space, segment.A, segment.Top));
                uvs.Add(new Vector2(u0, segment.BottomA / wallTextureSize.y));
                uvs.Add(new Vector2(u1, segment.BottomB / wallTextureSize.y));
                uvs.Add(new Vector2(u1, segment.Top / wallTextureSize.y));
                uvs.Add(new Vector2(u0, segment.Top / wallTextureSize.y));
                colors.Add(Darken(shade, groundA));
                colors.Add(Darken(shade, groundB));
                colors.Add(shade);
                colors.Add(shade);
                AddQuad(start);
            }

            foreach (var end in gableEnds)
            {
                Vector2 normal = space.TransformDirection(end.Normal);
                if (!HeightProjection.IsVisibleWall(normal, leanPerUnit))
                    continue;

                var shade = WallShade(normal, light);
                var middle = (end.A + end.B) * 0.5f;
                var start = vertices.Count;
                vertices.Add(ToLocal(target, space, end.A, end.Base));
                vertices.Add(ToLocal(target, space, end.B, end.Base));
                vertices.Add(ToLocal(target, space, middle, end.Apex));
                uvs.Add(new Vector2(WallU(shape, end.Normal, end.A), end.Base / wallTextureSize.y));
                uvs.Add(new Vector2(WallU(shape, end.Normal, end.B), end.Base / wallTextureSize.y));
                uvs.Add(new Vector2(WallU(shape, end.Normal, middle), end.Apex / wallTextureSize.y));
                colors.Add(shade);
                colors.Add(shade);
                colors.Add(shade);
                triangles.Add(start);
                triangles.Add(start + 2);
                triangles.Add(start + 1);
            }

            ApplyBuffers(mesh);
        }

        private void UpdateRoof(Transform space, BlockShape shape, Vector2 ridgeExtensions, int roofOrder)
        {
            var useGable = shape.HasGable && gableRoof != null && roofMesh != null;
            if (roof.enabled == useGable)
                roof.enabled = !useGable;
            if (gableRenderer != null && gableRenderer.enabled != useGable)
                gableRenderer.enabled = useGable;

            if (useGable)
            {
                if (gableRenderer != null && gableRenderer.sortingOrder != roofOrder)
                    gableRenderer.sortingOrder = roofOrder;
                BuildGableRoof(space, shape, ridgeExtensions);
                return;
            }

            var roofColor = roof.color;
            if (!Mathf.Approximately(roofColor.a, alpha))
            {
                roofColor.a = alpha;
                roof.color = roofColor;
            }
        }

        private void BuildGableRoof(Transform space, BlockShape shape, Vector2 ridgeExtensions)
        {
            ClearBuffers();
            var target = gableRoof.transform;
            var light = lightDirection.normalized;
            var rect = shape.Rect;
            var along = shape.RidgeAlongX ? Vector2.right : Vector2.up;
            var across = shape.RidgeAlongX ? Vector2.up : Vector2.right;
            var halfLength = (shape.RidgeAlongX ? rect.width : rect.height) * 0.5f;
            var halfDepth = (shape.RidgeAlongX ? rect.height : rect.width) * 0.5f + eaveOverhang;
            var center = rect.center;
            var eave = shape.Height;
            var ridge = shape.TopHeight;
            var eaveStart = halfLength + (ridgeExtensions.x > 0f ? 0f : eaveOverhang);
            var eaveEnd = halfLength + (ridgeExtensions.y > 0f ? 0f : eaveOverhang);
            var ridgeStart = halfLength + (ridgeExtensions.x > 0f ? ridgeExtensions.x : eaveOverhang);
            var ridgeEnd = halfLength + (ridgeExtensions.y > 0f ? ridgeExtensions.y : eaveOverhang);
            var origin = Vector2.Dot(center, along) - Mathf.Max(eaveStart, ridgeStart);
            var ridgeA = center - along * ridgeStart;
            var ridgeB = center + along * ridgeEnd;

            for (var side = -1; side <= 1; side += 2)
            {
                Vector2 normal = space.TransformDirection(across * side);
                var shade = Color.Lerp(shadedRoof, litRoof, Mathf.Clamp01(Vector2.Dot(normal, light) * 0.5f + 0.5f));
                shade.a = alpha;

                var eaveA = center - along * eaveStart + across * (halfDepth * side);
                var eaveB = center + along * eaveEnd + across * (halfDepth * side);
                var depth = halfDepth / roofTextureSize.y;

                var start = vertices.Count;
                vertices.Add(ToLocal(target, space, eaveA, eave));
                vertices.Add(ToLocal(target, space, eaveB, eave));
                vertices.Add(ToLocal(target, space, ridgeB, ridge));
                vertices.Add(ToLocal(target, space, ridgeA, ridge));
                uvs.Add(new Vector2(RoofU(eaveA, along, origin), 0f));
                uvs.Add(new Vector2(RoofU(eaveB, along, origin), 0f));
                uvs.Add(new Vector2(RoofU(ridgeB, along, origin), depth));
                uvs.Add(new Vector2(RoofU(ridgeA, along, origin), depth));
                for (var i = 0; i < 4; i++)
                    colors.Add(shade);
                AddQuad(start);
            }

            if (ridgeCapWidth > 0f)
            {
                var cap = ridgeCap;
                cap.a = alpha;
                var half = across * (ridgeCapWidth * 0.5f);
                var start = vertices.Count;
                vertices.Add(ToLocal(target, space, ridgeA - half, ridge));
                vertices.Add(ToLocal(target, space, ridgeB - half, ridge));
                vertices.Add(ToLocal(target, space, ridgeB + half, ridge));
                vertices.Add(ToLocal(target, space, ridgeA + half, ridge));
                var u0 = RoofU(ridgeA, along, origin);
                var u1 = RoofU(ridgeB, along, origin);
                uvs.Add(new Vector2(u0, 0f));
                uvs.Add(new Vector2(u1, 0f));
                uvs.Add(new Vector2(u1, 0.1f));
                uvs.Add(new Vector2(u0, 0.1f));
                for (var i = 0; i < 4; i++)
                    colors.Add(cap);
                AddQuad(start);
            }

            ApplyBuffers(roofMesh);
        }

        private float RoofU(Vector2 point, Vector2 along, float origin) => (Vector2.Dot(point, along) - origin) / roofTextureSize.x;

        private Color WallShade(Vector2 worldNormal, Vector2 light)
        {
            var shade = Color.Lerp(shadedWall, litWall, Mathf.Clamp01(Vector2.Dot(worldNormal, light) * 0.5f + 0.5f));
            shade.a = alpha;
            return shade;
        }

        private static Color Darken(Color color, float factor) =>
            new Color(color.r * factor, color.g * factor, color.b * factor, color.a);

        private float WallU(BlockShape shape, Vector2 normal, Vector2 point)
        {
            var edge = normal.y < -0.5f ? 0 : normal.x > 0.5f ? 1 : normal.y > 0.5f ? 2 : 3;
            CompoundLayout.GetEdge(shape.Rect, edge, out var a, out var b, out _);
            var length = Vector2.Distance(a, b);
            var direction = length > 0f ? (b - a) / length : Vector2.zero;
            var margin = (length % wallTextureSize.x) * 0.5f;
            return (Vector2.Dot(point - a, direction) - margin) / wallTextureSize.x;
        }

        private Vector3 ToLocal(Transform target, Transform space, Vector2 point, float height)
        {
            var flat = (Vector2)space.TransformPoint(point) + leanPerUnit * height;
            return target.InverseTransformPoint(new Vector3(flat.x, flat.y, HeightProjection.BuildingDepth(height)));
        }

        private void AddQuad(int start)
        {
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 1);
            triangles.Add(start);
            triangles.Add(start + 3);
            triangles.Add(start + 2);
        }

        private void ClearBuffers()
        {
            vertices.Clear();
            uvs.Clear();
            colors.Clear();
            triangles.Clear();
        }

        private void ApplyBuffers(Mesh target)
        {
            target.Clear();
            target.SetVertices(vertices);
            target.SetUVs(0, uvs);
            target.SetColors(colors);
            target.SetTriangles(triangles, 0);
            target.RecalculateBounds();
        }
    }
}
