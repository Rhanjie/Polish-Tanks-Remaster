using PolishTanks.Prototypes;
using UnityEngine;

namespace PolishTanks.Rendering
{
    /// <summary>
    /// Casts a flat ground shadow of a sprite: a dark copy of it pushed away from <see cref="SunLight"/> by how high
    /// the object stands. The copy follows the sprite every frame, so a hull swapped by a module or a turret that
    /// turns keeps a matching shadow. The shadow object is created at run time and never saved with the scene.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class DropShadow : MonoBehaviour
    {
        [Tooltip("How high the object stands; the shadow moves this far away from the sun, in world units.")]
        [SerializeField, Min(0f)] private float height = 0.35f;
        [SerializeField] private Color color = new Color(0f, 0f, 0f, 0.35f);
        [SerializeField] private int sortingOrder = -50;

        private SpriteRenderer source;
        private SpriteRenderer shadow;

        public SpriteRenderer Shadow => shadow;

        public static Vector2 Offset(float height) => HeightProjection.ShadowOffset(SunLight.Direction, height, 1f);

        private void OnEnable()
        {
            source = GetComponent<SpriteRenderer>();
            var go = new GameObject("DropShadow") { hideFlags = HideFlags.HideAndDontSave };
            go.transform.SetParent(transform, false);
            shadow = go.AddComponent<SpriteRenderer>();
            Follow();
        }

        private void OnDisable()
        {
            if (shadow == null)
                return;
            if (Application.isPlaying)
                Destroy(shadow.gameObject);
            else
                DestroyImmediate(shadow.gameObject);
            shadow = null;
        }

        private void LateUpdate() => Follow();

        private void Follow()
        {
            if (shadow == null)
                return;
            shadow.enabled = source.enabled && source.sprite != null;
            shadow.sprite = source.sprite;
            shadow.drawMode = source.drawMode;
            if (source.drawMode != SpriteDrawMode.Simple)
                shadow.size = source.size;
            shadow.flipX = source.flipX;
            shadow.flipY = source.flipY;
            shadow.sortingLayerID = source.sortingLayerID;
            shadow.sortingOrder = sortingOrder;
            shadow.sharedMaterial = source.sharedMaterial;
            shadow.color = new Color(color.r, color.g, color.b, color.a * source.color.a);
            shadow.transform.position = transform.position + (Vector3)Offset(height);
        }
    }
}
