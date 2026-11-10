using UnityEngine;

namespace PolishTanks.Weapons
{
    public readonly struct ProjectileHit
    {
        public readonly Vector2 Point;
        public readonly Vector2 Normal;
        public readonly Vector2 Direction;
        public readonly float Damage;
        public readonly float Impulse;
        public readonly Rigidbody2D Shooter;
        public readonly float PenetrationMin;
        public readonly float PenetrationMax;

        public ProjectileHit(Vector2 point, Vector2 normal, Vector2 direction, float damage, float impulse, Rigidbody2D shooter,
            float penetrationMin = 0f, float penetrationMax = 0f)
        {
            Point = point;
            Normal = normal;
            Direction = direction;
            Damage = damage;
            Impulse = impulse;
            Shooter = shooter;
            PenetrationMin = penetrationMin;
            PenetrationMax = penetrationMax;
        }
    }
}
