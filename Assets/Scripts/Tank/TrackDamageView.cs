using System.Collections.Generic;
using PolishTanks.Damage;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Temporary look for thrown tracks until proper art exists. When a track snaps, sparks fly off that side and
    /// a segmented strip is left lying on the ground behind that side, as if it unrolled, fading after a while. On the hull the track
    /// is covered by a pulsing red strip while it is out and an orange one once it is patched in the field.
    /// </summary>
    [RequireComponent(typeof(ModuleDamage))]
    public class TrackDamageView : MonoBehaviour
    {
        private const float TrackWidthRatio = 0.2f;
        private const float SegmentLength = 0.25f;
        private const float HullOverlayZ = -0.01f;
        private const int HullOrder = 10;
        private const int GroundOrder = 9;
        private const float PulseSpeed = 6f;

        private static Sprite segments;

        [SerializeField] private PooledEffect snapEffect;
        [SerializeField] private Color destroyedColor = new Color(0.85f, 0.1f, 0.05f, 0.75f);
        [SerializeField] private Color patchedColor = new Color(1f, 0.55f, 0.1f, 0.6f);
        [SerializeField] private Color groundColor = new Color(0.2f, 0.19f, 0.17f, 1f);
        [SerializeField, Min(1f)] private float groundLifetime = 30f;
        [SerializeField, Min(0.1f)] private float groundFade = 5f;

        [Inject] private IPoolService pools;

        private readonly List<(SpriteRenderer renderer, float bornAt)> thrown = new List<(SpriteRenderer, float)>();
        private ModuleDamage damage;
        private TankController controller;
        private BoxCollider2D hullCollider;
        private SpriteRenderer leftOverlay;
        private SpriteRenderer rightOverlay;

        private void Awake()
        {
            damage = GetComponent<ModuleDamage>();
            hullCollider = GetComponent<BoxCollider2D>();
            leftOverlay = CreateStrip("LeftTrackDamage", transform, HullOrder);
            rightOverlay = CreateStrip("RightTrackDamage", transform, HullOrder);
            damage.PartChanged += OnPartChanged;
            damage.LayoutChanged += Refresh;
            controller = GetComponent<TankController>();
            controller.StatsApplied += OnStatsApplied;
        }

        private void OnStatsApplied(Modules.TankStats _) => Refresh();

        private void OnDestroy()
        {
            if (damage != null)
            {
                damage.PartChanged -= OnPartChanged;
                damage.LayoutChanged -= Refresh;
            }
            if (controller != null)
                controller.StatsApplied -= OnStatsApplied;
            foreach (var (renderer, _) in thrown)
                if (renderer != null)
                    Destroy(renderer.gameObject);
            thrown.Clear();
        }

        private void Start() => Refresh();

        private void Update()
        {
            var pulse = 0.65f + 0.35f * Mathf.Sin(Time.time * PulseSpeed);
            Tint(leftOverlay, VehiclePart.LeftTrack, pulse);
            Tint(rightOverlay, VehiclePart.RightTrack, pulse);
            FadeThrownTracks();
        }

        private void OnPartChanged(PartId id, PartCondition condition)
        {
            if (id.Part != VehiclePart.LeftTrack && id.Part != VehiclePart.RightTrack)
                return;
            Refresh();
            if (condition == PartCondition.Destroyed)
                Snap(id.Part == VehiclePart.LeftTrack);
        }

        private void Refresh()
        {
            var (width, length, offset) = TrackLayout();
            Place(leftOverlay, -offset, width, length);
            Place(rightOverlay, offset, width, length);
        }

        private void Snap(bool left)
        {
            var (width, length, offset) = TrackLayout();
            var side = left ? -1f : 1f;
            Vector2 outward = transform.right * side;
            Vector2 trackCenter = transform.TransformPoint(new Vector3(side * offset, 0f, 0f));

            if (pools != null && snapEffect != null)
                pools.For(snapEffect, 2).Get(trackCenter, Quaternion.LookRotation(Vector3.forward, outward));

            var strip = CreateStrip("ThrownTrack", null, GroundOrder);
            Vector2 backward = -transform.up;
            strip.transform.SetPositionAndRotation(trackCenter + outward * (width * 0.6f) + backward * (length * 0.75f),
                transform.rotation * Quaternion.Euler(0f, 0f, Random.Range(-8f, 8f)));
            strip.size = new Vector2(width, length * 0.85f);
            strip.color = groundColor;
            thrown.Add((strip, Time.time));
        }

        private void FadeThrownTracks()
        {
            for (var i = thrown.Count - 1; i >= 0; i--)
            {
                var (renderer, bornAt) = thrown[i];
                var left = groundLifetime - (Time.time - bornAt);
                if (renderer == null || left <= 0f)
                {
                    if (renderer != null)
                        Destroy(renderer.gameObject);
                    thrown.RemoveAt(i);
                    continue;
                }
                var color = groundColor;
                color.a *= Mathf.Clamp01(left / groundFade);
                renderer.color = color;
            }
        }

        private void Tint(SpriteRenderer overlay, VehiclePart part, float pulse)
        {
            var condition = damage.Model != null ? damage.Model.ConditionOf(new PartId(part)) : PartCondition.Intact;
            overlay.enabled = condition != PartCondition.Intact;
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

        private static void Place(SpriteRenderer strip, float x, float width, float length)
        {
            strip.transform.localPosition = new Vector3(x, 0f, HullOverlayZ);
            strip.size = new Vector2(width, length);
        }

        private static SpriteRenderer CreateStrip(string stripName, Transform parent, int order)
        {
            var go = new GameObject(stripName);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Segments;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.sortingOrder = order;
            renderer.enabled = parent == null;
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
