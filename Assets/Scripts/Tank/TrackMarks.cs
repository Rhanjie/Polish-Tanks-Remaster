using PolishTanks.Damage;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Leaves two tread marks on the ground behind the tank, one per track, that fade out over time. A snapped
    /// track drags nothing, so its side stops marking until the crew patches it.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class TrackMarks : MonoBehaviour
    {
        private const float TrackWidthRatio = 0.2f;

        [SerializeField] private Material material;
        [SerializeField] private Color color = new Color(0.09f, 0.075f, 0.06f, 0.85f);
        [Tooltip("Seconds a mark stays on the ground before it has faded completely.")]
        [SerializeField, Min(0.1f)] private float fadeTime = 25f;
        [Tooltip("World length of one repeat of the tread texture.")]
        [SerializeField, Min(0.01f)] private float treadLength = 0.5f;
        [SerializeField] private int sortingOrder = -90;

        private ModuleDamage damage;
        private BoxCollider2D hull;
        private TrailRenderer left;
        private TrailRenderer right;

        public TrailRenderer Left => left;
        public TrailRenderer Right => right;

        public static bool Marks(PartCondition trackCondition) => trackCondition != PartCondition.Destroyed;

        private void Awake()
        {
            damage = GetComponent<ModuleDamage>();
            hull = GetComponent<BoxCollider2D>();
            left = CreateTrail("LeftTrackMarks");
            right = CreateTrail("RightTrackMarks");
        }

        private void LateUpdate()
        {
            var width = hull.size.x * TrackWidthRatio;
            var offset = (hull.size.x - width) * 0.5f;
            Place(left, -offset, width);
            Place(right, offset, width);
            left.emitting = Marks(ConditionOf(VehiclePart.LeftTrack));
            right.emitting = Marks(ConditionOf(VehiclePart.RightTrack));
        }

        private PartCondition ConditionOf(VehiclePart part) =>
            damage != null && damage.Model != null ? damage.Model.ConditionOf(new PartId(part)) : PartCondition.Intact;

        private void Place(TrailRenderer trail, float x, float width)
        {
            trail.transform.localPosition = new Vector3(x, 0f, 0f);
            trail.widthMultiplier = width;
            trail.textureScale = new Vector2(1f / treadLength, 1f);
        }

        private TrailRenderer CreateTrail(string trailName)
        {
            var go = new GameObject(trailName);
            go.transform.SetParent(transform, false);
            var trail = go.AddComponent<TrailRenderer>();
            trail.sharedMaterial = material;
            trail.time = fadeTime;
            trail.minVertexDistance = 0.15f;
            trail.textureMode = LineTextureMode.Tile;
            trail.alignment = LineAlignment.TransformZ;
            trail.numCapVertices = 0;
            trail.sortingOrder = sortingOrder;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.colorGradient = new Gradient
            {
                colorKeys = new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(color.a, 0.6f), new GradientAlphaKey(0f, 1f),
                },
            };
            return trail;
        }
    }
}
