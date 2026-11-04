using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>Line chart of recent values with a gradient area fill. The newest value sits on the right edge.</summary>
    [UxmlElement]
    public partial class Sparkline : VisualElement
    {
        private const int GridLines = 3;
        private const int FillBands = 5;

        private float[] _values = new float[0];
        private int _count;
        private float _maxValue = 1f;
        private Color _accentColor = new Color(1f, 0.3f, 0.2f);

        [UxmlAttribute, Min(2)]
        public int Capacity { get; set; } = 32;

        [UxmlAttribute]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; MarkDirtyRepaint(); }
        }

        public Sparkline()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        /// <summary>Copies the values (oldest first). Allocates only when the history outgrows the buffer.</summary>
        public void SetValues(IReadOnlyList<float> values, float maxValue)
        {
            if (_values.Length < values.Count)
                _values = new float[values.Count];
            _count = values.Count;
            for (var i = 0; i < _count; i++)
                _values[i] = values[i];
            _maxValue = Mathf.Max(maxValue, 0.0001f);
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var w = contentRect.width;
            var h = contentRect.height;
            if (w < 1f || h < 1f)
                return;

            var painter = ctx.painter2D;

            painter.lineWidth = 1f;
            painter.strokeColor = new Color(1f, 1f, 1f, 0.06f);
            painter.BeginPath();
            for (var i = 0; i <= GridLines; i++)
            {
                var y = Mathf.Round(h * i / GridLines) + 0.5f;
                painter.MoveTo(new Vector2(0f, y));
                painter.LineTo(new Vector2(w, y));
            }
            painter.Stroke();

            if (_count < 2)
                return;

            var step = w / (Mathf.Max(Capacity, _count) - 1);
            var first = Point(0, w, h, step);

            // Fading area fill built from stacked translucent copies of the area, each squashed further
            // towards the baseline. FillGradient ignored alpha here, which rendered a solid block.
            var fill = _accentColor;
            fill.a = 0.07f;
            painter.fillColor = fill;
            for (var band = 0; band < FillBands; band++)
            {
                var squash = 1f - (float)band / FillBands;
                painter.BeginPath();
                painter.MoveTo(new Vector2(first.x, h));
                for (var i = 0; i < _count; i++)
                {
                    var p = Point(i, w, h, step);
                    painter.LineTo(new Vector2(p.x, h - (h - p.y) * squash));
                }
                painter.LineTo(new Vector2(w, h));
                painter.ClosePath();
                painter.Fill();
            }

            painter.lineWidth = 2f;
            painter.lineJoin = LineJoin.Round;
            painter.lineCap = LineCap.Round;
            painter.strokeColor = _accentColor;
            painter.BeginPath();
            painter.MoveTo(first);
            for (var i = 1; i < _count; i++)
                painter.LineTo(Point(i, w, h, step));
            painter.Stroke();

            var last = Point(_count - 1, w, h, step);
            painter.fillColor = Color.white;
            painter.BeginPath();
            painter.Arc(last, 3.5f, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Fill();
        }

        private Vector2 Point(int index, float w, float h, float step)
        {
            var x = w - (_count - 1 - index) * step;
            var normalized = Mathf.Clamp01(_values[index] / _maxValue);
            // A few pixels of headroom keep the stroke and the end dot from being clipped.
            var y = h - 4f - normalized * (h - 10f);
            return new Vector2(x, y);
        }
    }
}
