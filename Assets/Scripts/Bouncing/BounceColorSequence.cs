using UnityEngine;

namespace PolishTanks.Bouncing
{
    /// <summary>
    /// Deterministic colour per bounce. Hue advances by the golden-ratio conjugate, so consecutive colours
    /// are always far apart on the colour wheel and the sequence never settles into a short repeating cycle.
    /// </summary>
    public sealed class BounceColorSequence
    {
        public const double GoldenRatioConjugate = 0.6180339887498949;

        public BounceColorSequence(float startHue, float saturation = 0.78f, float value = 1f)
        {
            StartHue = Mathf.Repeat(startHue, 1f);
            Saturation = Mathf.Clamp01(saturation);
            Value = Mathf.Clamp01(value);
        }

        public float StartHue { get; }
        public float Saturation { get; }
        public float Value { get; }

        public float HueFor(int bounceIndex)
        {
            // Double precision keeps the hue stable after millions of bounces.
            var hue = (StartHue + bounceIndex * GoldenRatioConjugate) % 1.0;
            if (hue < 0.0)
                hue += 1.0;
            return (float)hue;
        }

        public Color ColorFor(int bounceIndex) => Color.HSVToRGB(HueFor(bounceIndex), Saturation, Value);
    }
}
