using PolishTanks.Damage;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Temporary look for thrown tracks until proper art exists. When a track snaps, sparks fly off that side and
    /// the loose end hangs out a little behind the hull, under it, until the track is patched. On the hull the track
    /// is covered by a pulsing red strip while it is out and an orange one once it is patched in the field.
    /// </summary>
    [RequireComponent(typeof(ModuleDamage))]
    public class TrackDamageView : MonoBehaviour
    {
        private const float TrackWidthRatio = 0.2f;
        private const float SegmentLength = 0.25f;
        private const float HullOverlayZ = -0.01f;
        private const int HullOrder = 10;
        private const int UnderHullOrder = 9;
        private const float PulseSpeed = 6f;
        private const float LooseEndLength = 0.45f;
        private const float LooseEndOverhang = 0.2f;
        private const float LooseEndAngle = 6f;

        private static Sprite segments;

        [SerializeField] private PooledEffect snapEffect;
        [SerializeField] private Color destroyedColor = new Color(0.85f, 0.1f, 0.05f, 0.75f);
        [SerializeField] private Color patchedColor = new Color(1f, 0.55f, 0.1f, 0.6f);
        [SerializeField] private Color looseEndColor = new Color(0.2f, 0.19f, 0.17f, 1f);

        [Inject] private IPoolService pools;

        private ModuleDamage damage;
        private TankController controller;
        private BoxCollider2D hullCollider;
        private SpriteRenderer leftOverlay;
        private SpriteRenderer rightOverlay;
        private SpriteRenderer leftLooseEnd;
        private SpriteRenderer rightLooseEnd;

        private void Awake()
        {
            damage = GetComponent<ModuleDamage>();
            hullCollider = GetComponent<BoxCollider2D>();
            controller = GetComponent<TankController>();
            leftOverlay = CreateStrip("LeftTrackDamage", HullOrder);
            rightOverlay = CreateStrip("RightTrackDamage", HullOrder);
            leftLooseEnd = CreateStrip("LeftTrackLooseEnd", UnderHullOrder);
            rightLooseEnd = CreateStrip("RightTrackLooseEnd", UnderHullOrder);
            leftLooseEnd.color = looseEndColor;
            rightLooseEnd.color = looseEndColor;
            damage.PartChanged += OnPartChanged;
            damage.LayoutChanged += Refresh;
            controller.StatsApplied += OnStatsApplied;
        }

        private void OnDestroy()
        {
            if (damage != null)
            {
                damage.PartChanged -= OnPartChanged;
                damage.LayoutChanged -= Refresh;
            }
            if (controller != null)
                controller.StatsApplied -= OnStatsApplied;
        }

        private void Start() => Refresh();

        private void Update()
        {
            var pulse = 0.65f + 0.35f * Mathf.Sin(Time.time * PulseSpeed);
            Show(leftOverlay, leftLooseEnd, VehiclePart.LeftTrack, pulse);
            Show(rightOverlay, rightLooseEnd, VehiclePart.RightTrack, pulse);
        }

        private void OnStatsApplied(Modules.TankStats _) => Refresh();

        private void OnPartChanged(PartId id, PartCondition condition)
        {
            if (id.Part != VehiclePart.LeftTrack && id.Part != VehiclePart.RightTrack)
                return;
            if (condition == PartCondition.Destroyed)
                Sparks(id.Part == VehiclePart.LeftTrack);
        }

        private void Refresh()
        {
            var (width, length, offset) = TrackLayout();
            PlaceOverlay(leftOverlay, -offset, width, length);
            PlaceOverlay(rightOverlay, offset, width, length);
            PlaceLooseEnd(leftLooseEnd, -1f, offset, width, length);
            PlaceLooseEnd(rightLooseEnd, 1f, offset, width, length);
        }

        private void Sparks(bool left)
        {
            if (pools == null || snapEffect == null)
                return;
            var (_, _, offset) = TrackLayout();
            var side = left ? -1f : 1f;
            Vector2 trackCenter = transform.TransformPoint(new Vector3(side * offset, 0f, 0f));
            pools.For(snapEffect, 2).Get(trackCenter, Quaternion.LookRotation(Vector3.forward, transform.right * side));
        }

        private void Show(SpriteRenderer overlay, SpriteRenderer looseEnd, VehiclePart part, float pulse)
        {
            var condition = damage.Model != null ? damage.Model.ConditionOf(new PartId(part)) : PartCondition.Intact;
            overlay.enabled = condition != PartCondition.Intact;
            looseEnd.enabled = condition == PartCondition.Destroyed;
            if (!overlay.enabled)
                return;
            var color = condition == PartCondition.Destroyed ? destroyedColor : patchedColor;
            if (condition == PartCondition.Destroyed)
                color.a *= pulse;
            overlay.color = color;
        }

        private (float width, float length, float offset) TrackLayout()
        {
            var size = hullCollider != null ? hullCollider.size : new Vector2(2.5f, 4f);
            var width = size.x * TrackWidthRatio;
            return (width, size.y, (size.x - width) * 0.5f);
        }

        private static void PlaceOverlay(SpriteRenderer strip, float x, float width, float length)
        {
            strip.transform.localPosition = new Vector3(x, 0f, HullOverlayZ);
            strip.size = new Vector2(width, length);
        }

        private static void PlaceLooseEnd(SpriteRenderer strip, float side, float offset, float width, float length)
        {
            var endLength = length * LooseEndLength;
            var centerY = -length * 0.5f - length * LooseEndOverhang + endLength * 0.5f;
            strip.transform.localPosition = new Vector3(side * offset, centerY, 0f);
            strip.transform.localRotation = Quaternion.Euler(0f, 0f, side * LooseEndAngle);
            strip.size = new Vector2(width, endLength);
        }

        private SpriteRenderer CreateStrip(string stripName, int order)
        {
            var go = new GameObject(stripName);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Segments;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.sortingOrder = order;
            renderer.enabled = false;
            return renderer;
        }

        private static Sprite Segments
        {
            get
            {
                if (segments == null)
                {
                    var texture = new Texture2D(1, 4) { name = "TrackSegments", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
                    texture.SetPixels(new[] { Color.white, Color.white, Color.white, new Color(0.55f, 0.55f, 0.55f, 1f) });
                    texture.Apply();
                    segments = Sprite.Create(texture, new Rect(0f, 0f, 1f, 4f), new Vector2(0.5f, 0.5f), 4f / SegmentLength,
                        0, SpriteMeshType.FullRect);
                }
                return segments;
            }
        }
    }
}
