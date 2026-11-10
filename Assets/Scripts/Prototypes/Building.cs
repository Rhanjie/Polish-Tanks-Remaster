using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// An axis-aligned building footprint with a height. Views (flat or extruded) and the shadow
    /// read from it; the ground collider always matches the footprint.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Building : MonoBehaviour
    {
        [Tooltip("Footprint in world units.")]
        [SerializeField] private Vector2 size = new Vector2(4f, 4f);

        [Tooltip("Height in world units. Drives shadow length and, in 2.5D, how far the roof leans.")]
        [SerializeField, Min(0f)] private float height = 3f;

        public Vector2 Size => size;
        public float Height => height;
        public Rect Footprint => new Rect((Vector2)transform.position - size * 0.5f, size);

        public void Configure(Vector2 newSize, float newHeight)
        {
            size = newSize;
            height = newHeight;
            SyncCollider();
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
