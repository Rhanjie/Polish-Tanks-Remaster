using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// A circle drawn on the ground with a <see cref="LineRenderer"/>, pulsing its width and alpha so it reads from a
    /// distance. Shared by the circles the player drives into.
    /// </summary>
    public sealed class ZoneRing
    {
        private const int Segments = 64;

        private readonly LineRenderer line;
        private readonly float width;
        private readonly float pulsePeriod;

        public ZoneRing(LineRenderer line, float radius, float width, float pulsePeriod)
        {
            this.line = line;
            this.width = width;
            this.pulsePeriod = pulsePeriod;
            if (line == null)
                return;
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = Segments;
            for (var i = 0; i < Segments; i++)
            {
                var angle = i * Mathf.PI * 2f / Segments;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }
            line.enabled = false;
        }

        public void Show(bool visible, Color color, float time)
        {
            if (line == null)
                return;
            line.enabled = visible;
            if (!visible)
                return;
            var pulse = 0.5f + 0.5f * Mathf.Sin(time * Mathf.PI * 2f / pulsePeriod);
            line.widthMultiplier = width * (0.8f + 0.4f * pulse);
            var shown = color;
            shown.a *= 0.65f + 0.35f * pulse;
            line.startColor = line.endColor = shown;
        }
    }
}
