using System;
using UnityEngine;

namespace PolishTanks.Combat
{
    /// <summary>Nominal armor thickness per side, in millimetres.</summary>
    [Serializable]
    public struct ArmorProfile
    {
        [Min(0f)] public float front;
        [Min(0f)] public float side;
        [Min(0f)] public float rear;

        public ArmorProfile(float front, float side, float rear)
        {
            this.front = front;
            this.side = side;
            this.rear = rear;
        }

        public float For(HitSide hitSide) => hitSide switch
        {
            HitSide.Front => front,
            HitSide.Rear => rear,
            _ => side,
        };
    }
}
