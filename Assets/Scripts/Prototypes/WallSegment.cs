using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// A visible piece of a block's outer wall in compound space. It starts above whatever stands next to it
    /// (ground or a neighbouring block's roof, possibly sloped, hence two bottoms) and ends at the block's eaves.
    /// </summary>
    public readonly struct WallSegment
    {
        public readonly Vector2 A;
        public readonly Vector2 B;
        public readonly Vector2 Normal;
        public readonly float BottomA;
        public readonly float BottomB;
        public readonly float Top;

        public WallSegment(Vector2 a, Vector2 b, Vector2 normal, float bottomA, float bottomB, float top)
        {
            A = a;
            B = b;
            Normal = normal;
            BottomA = bottomA;
            BottomB = bottomB;
            Top = top;
        }
    }
}
