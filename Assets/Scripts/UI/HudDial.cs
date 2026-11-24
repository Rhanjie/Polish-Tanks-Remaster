using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Round face of the tank HUD drawn with Painter2D. The outer arc over the top is the speedometer, with a tick
    /// every <see cref="TickStepKmh"/> km/h; it turns another colour when reversing. Two inner arcs
    /// frame the tank drawing: the main gun's reload on the left, filling upward, and the machine gun belt on the
    /// right, which drains as it fires and refills while a new belt goes in. Colours come from USS custom properties.
    /// </summary>
    [UxmlElement]
    public partial class HudDial : VisualElement
    {
        public const int TickStepKmh = 10;

        private const float SpeedStart = 150f;
        private const float SpeedSweep = 240f;
        private const float GunStart = 115f;
        private const float BeltStart = 65f;
        private const float SideSweep = 135f;
        private const float SpeedWidth = 9f;
        private const float SideWidth = 5f;
        private const float Epsilon = 0.002f;

        private static readonly CustomStyleProperty<Color> FaceProperty = new CustomStyleProperty<Color>("--dial-face");
        private static readonly CustomStyleProperty<Color> RimProperty = new CustomStyleProperty<Color>("--dial-rim");
        private static readonly CustomStyleProperty<Color> TrackProperty = new CustomStyleProperty<Color>("--dial-track");
        private static readonly CustomStyleProperty<Color> TickProperty = new CustomStyleProperty<Color>("--dial-tick");
        private static readonly CustomStyleProperty<Color> SpeedProperty = new CustomStyleProperty<Color>("--dial-speed");
        private static readonly CustomStyleProperty<Color> ReverseProperty = new CustomStyleProperty<Color>("--dial-reverse");
        private static readonly CustomStyleProperty<Color> ReadyProperty = new CustomStyleProperty<Color>("--dial-ready");
        private static readonly CustomStyleProperty<Color> LoadingProperty = new CustomStyleProperty<Color>("--dial-loading");
        private static readonly CustomStyleProperty<Color> BeltProperty = new CustomStyleProperty<Color>("--dial-belt");

        private Color face = new Color(0.06f, 0.07f, 0.05f, 0.8f);
        private Color rim = new Color(0.78f, 0.78f, 0.7f, 0.35f);
        private Color track = new Color(1f, 1f, 1f, 0.12f);
        private Color tick = new Color(0.9f, 0.9f, 0.85f, 0.6f);
        private Color speedColor = new Color(0.55f, 0.85f, 0.95f);
        private Color reverseColor = new Color(0.95f, 0.75f, 0.3f);
        private Color readyColor = new Color(0.45f, 0.85f, 0.4f);
        private Color loadingColor = new Color(0.95f, 0.6f, 0.15f);
        private Color beltColor = new Color(0.85f, 0.85f, 0.75f);

        private float speedFill;
        private bool reversing;
        private int gaugeMaxKmh = 40;
        private float gunFill = 1f;
        private float beltFill = 1f;
        private bool beltChanging;

        public HudDial()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
        }

        public void SetSpeed(float fill, bool reverse, int maxKmh)
        {
            if (Mathf.Abs(fill - speedFill) < Epsilon && reverse == reversing && maxKmh == gaugeMaxKmh)
                return;
            speedFill = fill;
            reversing = reverse;
            gaugeMaxKmh = maxKmh;
            MarkDirtyRepaint();
        }

        public void SetWeapons(float gun, float belt, bool changingBelt)
        {
            if (Mathf.Abs(gun - gunFill) < Epsilon && Mathf.Abs(belt - beltFill) < Epsilon && changingBelt == beltChanging)
                return;
            gunFill = gun;
            beltFill = belt;
            beltChanging = changingBelt;
            MarkDirtyRepaint();
        }

        private void OnStyleResolved(CustomStyleResolvedEvent evt)
        {
            var style = evt.customStyle;
            Read(style, FaceProperty, ref face);
            Read(style, RimProperty, ref rim);
            Read(style, TrackProperty, ref track);
            Read(style, TickProperty, ref tick);
            Read(style, SpeedProperty, ref speedColor);
            Read(style, ReverseProperty, ref reverseColor);
            Read(style, ReadyProperty, ref readyColor);
            Read(style, LoadingProperty, ref loadingColor);
            Read(style, BeltProperty, ref beltColor);
            MarkDirtyRepaint();
        }

        private static void Read(ICustomStyle style, CustomStyleProperty<Color> property, ref Color color)
        {
            if (style.TryGetValue(property, out var value))
                color = value;
        }

        private void Draw(MeshGenerationContext context)
        {
            var size = contentRect.size;
            var radius = Mathf.Min(size.x, size.y) * 0.5f;
            if (radius < 10f)
                return;
            var center = size * 0.5f;
            var painter = context.painter2D;

            painter.fillColor = face;
            painter.BeginPath();
            painter.Arc(center, radius - 1f, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Fill();

            painter.lineWidth = 1.5f;
            painter.strokeColor = rim;
            painter.BeginPath();
            painter.Arc(center, radius - 1f, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Stroke();

            var speedRadius = radius - 14f;
            painter.lineCap = LineCap.Butt;
            Stroke(painter, center, speedRadius, SpeedStart, SpeedStart + SpeedSweep, SpeedWidth, track);
            if (speedFill > 0f)
                Stroke(painter, center, speedRadius, SpeedStart, SpeedStart + SpeedSweep * speedFill, SpeedWidth,
                    reversing ? reverseColor : speedColor);
            DrawTicks(painter, center, radius);

            var sideRadius = radius - 30f;
            Stroke(painter, center, sideRadius, GunStart, GunStart + SideSweep, SideWidth, track);
            if (gunFill > 0f)
                Stroke(painter, center, sideRadius, GunStart, GunStart + SideSweep * gunFill, SideWidth,
                    gunFill >= 1f ? readyColor : loadingColor);

            Stroke(painter, center, sideRadius, BeltStart, BeltStart - SideSweep, SideWidth, track, ArcDirection.CounterClockwise);
            if (beltFill > 0f)
                Stroke(painter, center, sideRadius, BeltStart, BeltStart - SideSweep * beltFill, SideWidth,
                    beltChanging ? loadingColor : beltColor, ArcDirection.CounterClockwise);
        }

        private void DrawTicks(Painter2D painter, Vector2 center, float radius)
        {
            if (gaugeMaxKmh <= 0)
                return;
            painter.strokeColor = tick;
            painter.lineCap = LineCap.Round;
            var steps = gaugeMaxKmh / TickStepKmh;
            for (var i = 0; i <= steps; i++)
            {
                var major = i % 2 == 0;
                var angle = (SpeedStart + SpeedSweep * i / steps) * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                painter.lineWidth = major ? 2.5f : 1.5f;
                painter.BeginPath();
                painter.MoveTo(center + direction * (radius - (major ? 20f : 17f)));
                painter.LineTo(center + direction * (radius - 4f));
                painter.Stroke();
            }
        }

        private static void Stroke(Painter2D painter, Vector2 center, float radius, float from, float to, float width,
            Color color, ArcDirection direction = ArcDirection.Clockwise)
        {
            painter.lineWidth = width;
            painter.strokeColor = color;
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(from), Angle.Degrees(to), direction);
            painter.Stroke();
        }
    }
}
