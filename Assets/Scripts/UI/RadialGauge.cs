using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>270° arc gauge with tick marks, a glowing value arc and a peak marker.</summary>
    [UxmlElement]
    public partial class RadialGauge : VisualElement
    {
        private const float StartDegrees = 135f;
        private const float SweepDegrees = 270f;
        private const int TickCount = 28;
        private const int MajorTickEvery = 7;

        private float _value;
        private float _peak;
        private float _pulse;
        private Color _accentColor = new Color(1f, 0.3f, 0.2f);

        [UxmlAttribute, Range(0f, 1f)]
        public float Value
        {
            get => _value;
            set { _value = Mathf.Clamp01(value); MarkDirtyRepaint(); }
        }

        [UxmlAttribute, Range(0f, 1f)]
        public float Peak
        {
            get => _peak;
            set { _peak = Mathf.Clamp01(value); MarkDirtyRepaint(); }
        }

        /// <summary>Extra glow right after an impact; the HUD fades it back to 0.</summary>
        [UxmlAttribute, Range(0f, 1f)]
        public float Pulse
        {
            get => _pulse;
            set { _pulse = Mathf.Clamp01(value); MarkDirtyRepaint(); }
        }

        [UxmlAttribute]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; MarkDirtyRepaint(); }
        }

        [UxmlAttribute]
        public float Thickness { get; set; } = 9f;

        public RadialGauge()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var w = contentRect.width;
            var h = contentRect.height;
            if (w < 1f || h < 1f)
                return;

            var painter = ctx.painter2D;
            var center = new Vector2(w * 0.5f, h * 0.5f);
            var radius = Mathf.Min(w, h) * 0.5f - 14f;
            if (radius <= Thickness)
                return;

            // Ticks just outside the track; every 7th one is a major tick.
            painter.lineCap = LineCap.Butt;
            painter.lineWidth = 1.5f;
            painter.strokeColor = new Color(1f, 1f, 1f, 0.22f);
            painter.BeginPath();
            for (var i = 0; i <= TickCount; i++)
            {
                var major = i % MajorTickEvery == 0;
                var dir = Direction(StartDegrees + SweepDegrees * i / TickCount);
                painter.MoveTo(center + dir * (radius + Thickness * 0.5f + 3f));
                painter.LineTo(center + dir * (radius + Thickness * 0.5f + (major ? 11f : 6f)));
            }
            painter.Stroke();

            painter.lineWidth = 1f;
            painter.strokeColor = new Color(1f, 1f, 1f, 0.07f);
            painter.BeginPath();
            painter.Arc(center, radius - Thickness - 6f, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Stroke();

            DrawArc(painter, center, radius, 1f, Thickness, new Color(1f, 1f, 1f, 0.08f), LineCap.Butt);

            if (_value > 0.001f)
            {
                var glow = _accentColor;
                glow.a = Mathf.Lerp(0.14f, 0.45f, _pulse);
                DrawArc(painter, center, radius, _value, Thickness * Mathf.Lerp(2f, 3.4f, _pulse), glow, LineCap.Round);
                DrawArc(painter, center, radius, _value, Thickness, _accentColor, LineCap.Round);
            }

            if (_peak > 0.001f)
            {
                var dir = Direction(StartDegrees + SweepDegrees * _peak);
                painter.lineCap = LineCap.Round;
                painter.lineWidth = 2.5f;
                painter.strokeColor = Color.white;
                painter.BeginPath();
                painter.MoveTo(center + dir * (radius - Thickness));
                painter.LineTo(center + dir * (radius + Thickness));
                painter.Stroke();
            }
        }

        private static void DrawArc(Painter2D painter, Vector2 center, float radius, float fraction, float width, Color color, LineCap cap)
        {
            painter.lineCap = cap;
            painter.lineWidth = width;
            painter.strokeColor = color;
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(StartDegrees), Angle.Degrees(StartDegrees + SweepDegrees * fraction));
            painter.Stroke();
        }

        // UI space has y pointing down, so increasing angles run clockwise on screen.
        private static Vector2 Direction(float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}
