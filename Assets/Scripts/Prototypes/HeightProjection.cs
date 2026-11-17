using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// Math for faking object height in a top-down view: roofs pushed away from the view center
    /// (perspective parallax) and shadows cast along the sun direction. Offsets are in world space; a rotated
    /// building converts them into its own space before building walls or shadows. Building sorting orders go from
    /// <see cref="BuildingSortingBase"/> up, <see cref="BuildingSortingStride"/> per step, higher for buildings nearer the centre.
    /// Each step holds <see cref="BuildingLayers"/> layers of two orders (walls, then roof) for the blocks of one compound.
    /// Blocks lower than <see cref="LowBuildingHeight"/> (low walls, rubble) skip that and draw at
    /// <see cref="LowBuildingSortingOrder"/>, above props such as crates but under tanks, so a gun barrel reaching over them stays visible.
    /// Building surfaces also write depth from <see cref="BuildingDepth"/>: in this oblique top-down view the higher surface
    /// is always the visible one, so the depth test settles overlaps that no draw order can. The depth range sits behind
    /// z = 0, where every other sprite lives, so buildings never hide sprites drawn after them.
    /// </summary>
    public static class HeightProjection
    {
        public const float MaxLeanDistance = 32f;

        public static Vector2 RoofOffset(Vector2 basePosition, Vector2 viewCenter, float height, float perspective) =>
            Vector2.ClampMagnitude(basePosition - viewCenter, MaxLeanDistance) * (height * perspective);

        public const int BuildingSortingBase = 100;
        public const int BuildingSortingSteps = 2000;
        public const float BuildingSortingStepsPerUnit = 10f;
        public const int BuildingLayers = 8;
        public const int BuildingSortingStride = BuildingLayers * 2;

        public const float BuildingDepthFar = 10f;
        public const float BuildingDepthPerUnit = 0.2f;
        public const float BuildingDepthMaxHeight = 45f;

        public static float BuildingDepth(float height) =>
            BuildingDepthFar - Mathf.Clamp(height, 0f, BuildingDepthMaxHeight) * BuildingDepthPerUnit;

        public const float LowBuildingHeight = 1.5f;
        public const int LowBuildingSortingOrder = 6;

        public static bool IsLowBuilding(float height) => height < LowBuildingHeight;

        public static int BuildingSortingOrder(float distanceFromViewCenter, float height, int layer) =>
            IsLowBuilding(height) ? LowBuildingSortingOrder : BuildingSortingOrder(distanceFromViewCenter) + layer * 2;

        public static int BuildingSortingOrder(float distanceFromViewCenter)
        {
            var steps = Mathf.Clamp(Mathf.RoundToInt(distanceFromViewCenter * BuildingSortingStepsPerUnit), 0, BuildingSortingSteps);
            return BuildingSortingBase + (BuildingSortingSteps - steps) * BuildingSortingStride;
        }

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
