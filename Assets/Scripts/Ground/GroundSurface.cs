using UnityEngine;
using UnityEngine.Tilemaps;

namespace PolishTanks.Ground
{
    /// <summary>
    /// Draws a painted ground tilemap as one quad instead of square tiles. The tilemap is turned into a
    /// <see cref="GroundIndexMap"/> uploaded as a point-sampled index texture; the <c>PolishTanks/Ground Surface</c>
    /// shader blends the four nearest cells with a wobbly edge and lets the higher texture (alpha) win, so grass gives way
    /// to dirt in clumps instead of along cell edges. Every texture repeat shows one of its layer's variants, picked at
    /// random from its position, so a large patch of one layer does not look tiled. Cells of layers that block vehicles get merged box colliders on
    /// <see cref="blockingLayer"/> under a hidden child.
    /// Runs in the editor and rebuilds whenever the tilemap changes, so painting with the tile palette shows the final
    /// ground at once. The tilemap's own renderer should stay disabled. Nothing it builds is saved with the scene.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GroundSurface : MonoBehaviour
    {
        private static readonly int IndexTexId = Shader.PropertyToID("_IndexTex");
        private static readonly int GridId = Shader.PropertyToID("_Grid");
        private static readonly int LayersId = Shader.PropertyToID("_Layers");
        private static readonly int LayerScaleId = Shader.PropertyToID("_LayerScale");
        private static readonly int LayerFirstSliceId = Shader.PropertyToID("_LayerFirstSlice");
        private static readonly int LayerVariantsId = Shader.PropertyToID("_LayerVariants");

        [SerializeField] private Tilemap tilemap;
        [SerializeField] private GroundLayerSet layers;

        [Tooltip("Layer of cells with no tile, also used for the margin.")]
        [SerializeField, Min(0)] private int fallbackLayer;

        [Tooltip("Cells of the fallback layer added around the painted area, so the ground runs past the playable edge.")]
        [SerializeField, Min(0)] private int margin = 16;

        [SerializeField] private int sortingOrder = -100;

        [Tooltip("Physics layer of the colliders over cells that block vehicles. Projectiles should ignore it.")]
        [SerializeField] private int blockingLayer = 4;

        private readonly float[] layerScales = new float[GroundLayerSet.MaxLayers];
        private readonly float[] layerFirstSlices = new float[GroundLayerSet.MaxLayers];
        private readonly float[] layerVariants = new float[GroundLayerSet.MaxLayers];
        private MeshRenderer meshRenderer;
        private Mesh mesh;
        private Texture2D indexTexture;
        private GameObject blocking;
        private MaterialPropertyBlock properties;
        private bool rebuildRequested;

        public GroundIndexMap Map { get; private set; }
        public Tilemap Tilemap => tilemap;

        public int LayerAt(Vector2 worldPosition)
        {
            if (Map == null || tilemap == null)
                return fallbackLayer;
            var cell = tilemap.WorldToCell(worldPosition);
            return Map.LayerAt(new Vector2Int(cell.x, cell.y));
        }

        public void Rebuild()
        {
            rebuildRequested = false;
            if (tilemap == null || layers == null)
                return;

            var bounds = tilemap.cellBounds;
            var cells = new RectInt(bounds.xMin - margin, bounds.yMin - margin, bounds.size.x + 2 * margin, bounds.size.y + 2 * margin);
            Map = GroundIndexMap.Build(cells, LayerOfCell, fallbackLayer);

            UploadIndexTexture();
            BuildMesh();
            ApplyProperties();
            BuildBlocking();
        }

        private void OnEnable()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            properties ??= new MaterialPropertyBlock();
            Tilemap.tilemapTileChanged += OnTilesChanged;
            Rebuild();
        }

        private void OnDisable()
        {
            Tilemap.tilemapTileChanged -= OnTilesChanged;
            Release(mesh);
            Release(indexTexture);
            Release(blocking);
            mesh = null;
            indexTexture = null;
            blocking = null;
            Map = null;
        }

        private void OnValidate() => rebuildRequested = true;

        private void Update()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                rebuildRequested = true;
            }
            if (rebuildRequested)
                Rebuild();
        }

        private void OnTilesChanged(Tilemap changed, Tilemap.SyncTile[] tiles)
        {
            if (changed == tilemap)
                rebuildRequested = true;
        }

        private int LayerOfCell(Vector2Int cell) =>
            tilemap.GetTile(new Vector3Int(cell.x, cell.y, 0)) is GroundTile tile && tile.Layer < UsableLayers ? tile.Layer : -1;

        private int UsableLayers => Mathf.Min(layers.Count, GroundLayerSet.MaxLayers);

        private void UploadIndexTexture()
        {
            var cells = Map.Cells;
            if (indexTexture == null || indexTexture.width != cells.width || indexTexture.height != cells.height)
            {
                Release(indexTexture);
                indexTexture = new Texture2D(cells.width, cells.height, TextureFormat.R8, false, true)
                {
                    name = "Ground Index",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontSave,
                };
            }
            indexTexture.SetPixelData(Map.Indices, 0);
            indexTexture.Apply(false);
        }

        private void BuildMesh()
        {
            if (mesh == null)
            {
                mesh = new Mesh { name = "Ground Surface", hideFlags = HideFlags.DontSave };
                GetComponent<MeshFilter>().sharedMesh = mesh;
            }

            var cells = Map.Cells;
            var min = tilemap.CellToWorld(new Vector3Int(cells.xMin, cells.yMin, 0));
            var max = tilemap.CellToWorld(new Vector3Int(cells.xMax, cells.yMax, 0));
            var corners = new[]
            {
                transform.InverseTransformPoint(new Vector3(min.x, min.y, 0f)),
                transform.InverseTransformPoint(new Vector3(max.x, min.y, 0f)),
                transform.InverseTransformPoint(new Vector3(min.x, max.y, 0f)),
                transform.InverseTransformPoint(new Vector3(max.x, max.y, 0f)),
            };
            mesh.Clear();
            mesh.vertices = corners;
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
            mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateBounds();
        }

        private void ApplyProperties()
        {
            var cells = Map.Cells;
            var origin = tilemap.CellToWorld(new Vector3Int(cells.xMin, cells.yMin, 0));
            var cellSize = tilemap.layoutGrid != null ? tilemap.layoutGrid.cellSize.x : 1f;
            for (var i = 0; i < layerScales.Length; i++)
            {
                layerScales[i] = 1f / layers.TextureSize(i);
                layerFirstSlices[i] = layers.FirstSlice(i);
                layerVariants[i] = layers.VariantCount(i);
            }

            meshRenderer.sortingOrder = sortingOrder;
            meshRenderer.GetPropertyBlock(properties);
            properties.SetTexture(IndexTexId, indexTexture);
            if (layers.Textures != null)
                properties.SetTexture(LayersId, layers.Textures);
            properties.SetVector(GridId, new Vector4(origin.x, origin.y, cellSize, 0f));
            properties.SetFloatArray(LayerScaleId, layerScales);
            properties.SetFloatArray(LayerFirstSliceId, layerFirstSlices);
            properties.SetFloatArray(LayerVariantsId, layerVariants);
            meshRenderer.SetPropertyBlock(properties);
        }

        private void BuildBlocking()
        {
            Release(blocking);
            blocking = null;
            var rects = Map.MergedRects(layers.BlocksVehicles);
            if (rects.Count == 0)
                return;

            blocking = new GameObject("Ground Blocking") { hideFlags = HideFlags.HideAndDontSave, layer = blockingLayer };
            blocking.transform.SetParent(transform, false);
            blocking.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var body = blocking.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var composite = blocking.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            foreach (var rect in rects)
            {
                var min = blocking.transform.InverseTransformPoint(tilemap.CellToWorld(new Vector3Int(rect.xMin, rect.yMin, 0)));
                var max = blocking.transform.InverseTransformPoint(tilemap.CellToWorld(new Vector3Int(rect.xMax, rect.yMax, 0)));
                var box = blocking.AddComponent<BoxCollider2D>();
                box.offset = (min + max) * 0.5f;
                box.size = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
                box.compositeOperation = Collider2D.CompositeOperation.Merge;
            }
            composite.GenerateGeometry();
        }

        private static void Release(Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
