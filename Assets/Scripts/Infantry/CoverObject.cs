using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>Something a soldier can crouch behind (a crate, a barrel, a low wall, a friendly tank), as a circle.</summary>
    public readonly struct CoverObject
    {
        public CoverObject(Vector2 centre, float radius)
        {
            Centre = centre;
            Radius = radius;
        }

        public Vector2 Centre { get; }
        public float Radius { get; }
    }
}
