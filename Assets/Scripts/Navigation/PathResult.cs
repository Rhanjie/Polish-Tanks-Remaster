using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>A finished search: its status and the waypoints to drive through, not including the start.</summary>
    public sealed class PathResult
    {
        public static readonly PathResult None = new PathResult(PathStatus.NoPath, new List<Vector2>());

        public PathResult(PathStatus status, List<Vector2> points)
        {
            Status = status;
            Points = points;
        }

        public PathStatus Status { get; }
        public IReadOnlyList<Vector2> Points { get; }
        public bool HasPath => Status != PathStatus.NoPath;

        public float Length(Vector2 from)
        {
            var length = 0f;
            var previous = from;
            foreach (var point in Points)
            {
                length += Vector2.Distance(previous, point);
                previous = point;
            }
            return length;
        }
    }
}
