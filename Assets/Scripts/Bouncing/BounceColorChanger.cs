using System;
using UnityEngine;

namespace PolishTanks.Bouncing
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BounceColorChanger : MonoBehaviour
    {
        [SerializeField]
        private BounceCounter counter;

        [SerializeField, Range(0f, 1f)]
        private float saturation = 0.78f;

        [SerializeField, Range(0f, 1f)]
        private float brightness = 1f;

        [SerializeField, Min(0f), Tooltip("Seconds to blend into the new colour. 0 switches instantly.")]
        private float blendDuration = 0.12f;

        private SpriteRenderer _renderer;
        private BounceColorSequence _sequence;
        private Color _fromColor;
        private Color _targetColor;
        private float _blend = 1f;

        /// <summary>The colour the ball is heading to (or already has). Valid even before this component's Awake.</summary>
        public Color TargetColor
        {
            get
            {
                EnsureInitialized();
                return _targetColor;
            }
        }

        public event Action<Color> ColorChanged;

        private void Awake() => EnsureInitialized();

        // Listeners on other objects (the HUD) may ask for TargetColor before this object's Awake has run.
        private void EnsureInitialized()
        {
            if (_sequence != null)
                return;

            _renderer = GetComponent<SpriteRenderer>();
            if (counter == null)
                counter = GetComponent<BounceCounter>();

            // Start the hue sequence from the colour the ball already has in the scene.
            Color.RGBToHSV(_renderer.color, out var startHue, out _, out _);
            _sequence = new BounceColorSequence(startHue, saturation, brightness);
            _fromColor = _targetColor = _renderer.color;
        }

        private void OnEnable()
        {
            if (counter != null)
                counter.Bounced += OnBounced;
            else
                Debug.LogWarning($"{nameof(BounceColorChanger)} on '{name}' has no {nameof(BounceCounter)}.", this);
        }

        private void OnDisable()
        {
            if (counter != null)
                counter.Bounced -= OnBounced;
        }

        private void Update()
        {
            if (_blend >= 1f)
                return;

            _blend = blendDuration <= 0f ? 1f : Mathf.Min(1f, _blend + Time.deltaTime / blendDuration);
            _renderer.color = Color.Lerp(_fromColor, _targetColor, _blend);
        }

        private void OnBounced(BounceEvent bounce)
        {
            _fromColor = _renderer.color;
            _targetColor = _sequence.ColorFor(bounce.Count);
            _blend = 0f;
            ColorChanged?.Invoke(_targetColor);
        }
    }
}
