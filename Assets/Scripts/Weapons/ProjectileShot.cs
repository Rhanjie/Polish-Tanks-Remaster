using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>Everything a projectile carries from the gun that fired it.</summary>
    public struct ProjectileShot
    {
        public Vector2 Velocity;
        public float Lifetime;
        public Rigidbody2D Shooter;
        public float Impulse;
        public float Damage;
        public float PenetrationMin;
        public float PenetrationMax;
        public float Caliber;
        public bool Ricocheted;
        public PrefabPool<PooledEffect> ImpactPool;
        public PrefabPool<PooledEffect> BlockedPool;
        public PrefabPool<PooledEffect> RicochetPool;
        public PrefabPool<PooledEffect> OtherRicochetPool;
    }
}
