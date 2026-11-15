using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>Everything a projectile carries from the gun that fired it. <see cref="LowCover"/> within <see cref="CoverClearance"/> of <see cref="FiredFrom"/> is shot over.</summary>
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
        public Vector2 FiredFrom;
        public float CoverClearance;
        public PrefabPool<PooledEffect> ImpactPool;
        public PrefabPool<PooledEffect> BlockedPool;
        public PrefabPool<PooledEffect> RicochetPool;
        public PrefabPool<PooledEffect> OtherRicochetPool;
    }
}
