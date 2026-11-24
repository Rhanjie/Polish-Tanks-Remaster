using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// Triangular wall under a gable roof on a short side of the block, in compound space: the base runs
    /// from A to B at eave height and the apex sits above the middle at ridge height.
    /// </summary>
    public readonly struct GableEnd
    {
        public readonly Vector2 A;
        public readonly Vector2 B;
        public readonly Vector2 Normal;
        public readonly float Base;
        public readonly float Apex;

        public GableEnd(Vector2 a, Vector2 b, Vector2 normal, float baseHeight, float apex)
        {
            A = a;
            B = b;
            Normal = normal;
            Base = baseHeight;
            Apex = apex;
        }
    }
}
