using System;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// One thing in the way, as the grid builder sees it: its bounds, its kind and a distance function
    /// (0 inside the shape) used to decide which cells it touches.
    /// </summary>
    public readonly struct Obstacle
    {
        public Obstacle(Rect bounds, ObstacleKind kind, float softCost, Func<Vector2, float> distanceTo)
        {
            Bounds = bounds;
            Kind = kind;
            SoftCost = softCost;
            DistanceTo = distanceTo;
        }

        public Rect Bounds { get; }
        public ObstacleKind Kind { get; }
        public float SoftCost { get; }
        public Func<Vector2, float> DistanceTo { get; }

        public static Obstacle Box(Rect rect, ObstacleKind kind = ObstacleKind.Solid, float softCost = 0f)
        {
            return new Obstacle(rect, kind, softCost, point =>
            {
                var dx = Mathf.Max(rect.xMin - point.x, 0f, point.x - rect.xMax);
                var dy = Mathf.Max(rect.yMin - point.y, 0f, point.y - rect.yMax);
                return Mathf.Sqrt(dx * dx + dy * dy);
            });
        }
    }
}
