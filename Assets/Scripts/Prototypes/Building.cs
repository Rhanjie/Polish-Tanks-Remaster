using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// An axis-aligned building footprint with an eave height and a roof form. Views (flat or extruded) and the
    /// shadow read from it; the ground collider always matches the footprint. Several blocks under one
    /// <see cref="BuildingCompound"/> form a single building with a shared lean.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Building : MonoBehaviour
    {
        [Tooltip("Footprint in world units.")]
        [SerializeField] private Vector2 size = new Vector2(4f, 4f);

        [Tooltip("Height in world units. Drives shadow length and, in 2.5D, how far the roof leans.")]
        [SerializeField, Min(0f)] private float height = 3f;

        [Tooltip("Flat roofs use the roof sprite; gable roofs are built as a sloped mesh with the ridge along the longer side.")]
        [SerializeField] private RoofShape roof = RoofShape.Flat;

        [Tooltip("How far the gable ridge rises above the eaves, in world units.")]
        [SerializeField, Min(0f)] private float ridgeHeight = 2f;

        public Vector2 Size => size;
        public float Height => height;
        public RoofShape Roof => roof;
        public float RidgeHeight => roof == RoofShape.Gable ? ridgeHeight : 0f;
        public float TopHeight => height + RidgeHeight;
        public float ShadowHeight => height + RidgeHeight * 0.5f;
        public Rect Footprint => new Rect((Vector2)transform.position - size * 0.5f, size);

        public void Configure(Vector2 newSize, float newHeight)
        {
            size = newSize;
            height = newHeight;
            SyncCollider();
        }

        public void ConfigureRoof(RoofShape newRoof, float newRidgeHeight)
        {
            roof = newRoof;
            ridgeHeight = Mathf.Max(0f, newRidgeHeight);
        }

        private void Reset() => SyncCollider();

        private void OnValidate() => SyncCollider();

        private void SyncCollider()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null)
                return;
            box.size = size;
            box.offset = Vector2.zero;
        }
    }
}
