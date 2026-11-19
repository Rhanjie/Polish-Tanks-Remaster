using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>Distance from a point to the stretch a shot covered in one frame.</summary>
    public static class SegmentMath
    {
        public static float DistanceToSegment(Vector2 point, Vector2 from, Vector2 to)
        {
            var segment = to - from;
            var lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= Mathf.Epsilon)
                return Vector2.Distance(point, from);
            var along = Mathf.Clamp01(Vector2.Dot(point - from, segment) / lengthSquared);
            return Vector2.Distance(point, from + segment * along);
        }
    }
}
