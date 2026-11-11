using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// Math for faking object height in a top-down view: roofs pushed away from the view center
    /// (perspective parallax) and shadows cast along the sun direction. Offsets are in world space; a rotated
    /// building converts them into its own space before building walls or shadows.
    /// </summary>
    public static class HeightProjection
    {
        public static Vector2 RoofOffset(Vector2 basePosition, Vector2 viewCenter, float height, float perspective) =>
            (basePosition - viewCenter) * (height * perspective);

        public static Vector2 ShadowOffset(Vector2 sunDirection, float height, float shadowLength) =>
            sunDirection.sqrMagnitude > 0f ? sunDirection.normalized * (height * shadowLength) : Vector2.zero;

        public static void ExtrudedRectHull(Rect rect, Vector2 offset, List<Vector2> result)
        {
            ExtrudedQuadHull(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin),
                new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax), offset, result);
        }

        public static void ExtrudedQuadHull(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 offset, List<Vector2> result)
        {
            Corners.Clear();
            Corners.Add(a);
            Corners.Add(b);
            Corners.Add(c);
            Corners.Add(d);
            for (var i = 0; i < 4; i++)
                Corners.Add(Corners[i] + offset);
            ConvexHull(Corners, result);
        }

        public static void ConvexHull(List<Vector2> points, List<Vector2> result)
        {
            result.Clear();
            if (points.Count < 3)
            {
                result.AddRange(points);
                return;
            }

            points.Sort(LexicographicOrder);

            for (var i = 0; i < points.Count; i++)
            {
                while (result.Count >= 2 && Cross(result[result.Count - 2], result[result.Count - 1], points[i]) <= 0f)
                    result.RemoveAt(result.Count - 1);
                result.Add(points[i]);
            }

            var lowerCount = result.Count + 1;
            for (var i = points.Count - 2; i >= 0; i--)
            {
                while (result.Count >= lowerCount && Cross(result[result.Count - 2], result[result.Count - 1], points[i]) <= 0f)
                    result.RemoveAt(result.Count - 1);
                result.Add(points[i]);
            }

            result.RemoveAt(result.Count - 1);
        }

        public static bool IsVisibleWall(Vector2 outwardNormal, Vector2 roofOffset) =>
            Vector2.Dot(outwardNormal, roofOffset) < -0.0001f;

        private static readonly List<Vector2> Corners = new List<Vector2>(8);
        private static readonly IComparer<Vector2> LexicographicOrder = new XThenY();

        private sealed class XThenY : IComparer<Vector2>
        {
            public int Compare(Vector2 a, Vector2 b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y);
        }

        private static float Cross(Vector2 o, Vector2 a, Vector2 b) =>
            (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }
}
