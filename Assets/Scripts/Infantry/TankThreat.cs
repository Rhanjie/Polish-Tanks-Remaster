using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>A tank near a soldier as the soldier judges it: where it is, where it is going and how wide it is.</summary>
    public readonly struct TankThreat
    {
        public const float DefaultHalfWidth = 1.4f;

        public TankThreat(Vector2 position, Vector2 velocity, float halfWidth = DefaultHalfWidth)
        {
            Position = position;
            Velocity = velocity;
            HalfWidth = halfWidth;
        }

        public Vector2 Position { get; }
        public Vector2 Velocity { get; }
        public float HalfWidth { get; }
    }
}
