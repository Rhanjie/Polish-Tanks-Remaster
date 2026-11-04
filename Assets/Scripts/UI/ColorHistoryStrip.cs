using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>Row of rounded swatches with the latest colours, newest on the left, older ones fading out.</summary>
    [UxmlElement]
    public partial class ColorHistoryStrip : VisualElement
    {
        private const float Gap = 4f;
        private const float CornerRadius = 4f;

        private Color[] _colors = new Color[12];
        private int _count;

        [UxmlAttribute, Min(1)]
        public int Capacity
        {
            get => _colors.Length;
            set
            {
                _colors = new Color[Mathf.Max(1, value)];
                _count = 0;
                MarkDirtyRepaint();
            }
        }

        public int Count => _count;

        public ColorHistoryStrip()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        public void Push(Color color)
        {
            for (var i = Mathf.Min(_count, _colors.Length - 1); i > 0; i--)
                _colors[i] = _colors[i - 1];
            _colors[0] = color;
            _count = Mathf.Min(_count + 1, _colors.Length);
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var w = contentRect.width;
            var h = contentRect.height;
            var capacity = _colors.Length;
            var cell = (w - Gap * (capacity - 1)) / capacity;
            if (cell < 1f || h < 1f)
                return;

            var painter = ctx.painter2D;
            for (var i = 0; i < capacity; i++)
            {
                var x = i * (cell + Gap);
                painter.BeginPath();
                TraceRoundedRect(painter, x + 0.5f, 0.5f, cell - 1f, h - 1f, CornerRadius);

                if (i < _count)
                {
                    var color = _colors[i];
                    color.a = Mathf.Lerp(1f, 0.25f, (float)i / capacity);
                    painter.fillColor = color;
                    painter.Fill();
                }
                else
                {
                    painter.lineWidth = 1f;
                    painter.strokeColor = new Color(1f, 1f, 1f, 0.12f);
                    painter.Stroke();
                }
            }

            if (_count > 0)
            {
                // A bright outline marks the current colour.
                painter.lineWidth = 2f;
                painter.strokeColor = Color.white;
                painter.BeginPath();
                TraceRoundedRect(painter, 1f, 1f, cell - 2f, h - 2f, CornerRadius);
                painter.Stroke();
            }
        }

        private static void TraceRoundedRect(Painter2D p, float x, float y, float w, float h, float r)
        {
            r = Mathf.Min(r, Mathf.Min(w, h) * 0.5f);
            p.MoveTo(new Vector2(x + r, y));
            p.LineTo(new Vector2(x + w - r, y));
            p.ArcTo(new Vector2(x + w, y), new Vector2(x + w, y + r), r);
            p.LineTo(new Vector2(x + w, y + h - r));
            p.ArcTo(new Vector2(x + w, y + h), new Vector2(x + w - r, y + h), r);
            p.LineTo(new Vector2(x + r, y + h));
            p.ArcTo(new Vector2(x, y + h), new Vector2(x, y + h - r), r);
            p.LineTo(new Vector2(x, y + r));
            p.ArcTo(new Vector2(x, y), new Vector2(x + r, y), r);
            p.ClosePath();
        }
    }
}
