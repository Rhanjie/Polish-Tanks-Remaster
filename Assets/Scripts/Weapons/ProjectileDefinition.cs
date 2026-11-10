using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>What a weapon fires: the pooled prefab, how long it flies and what it leaves on impact.</summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Projectile", fileName = "Projectile_")]
    public class ProjectileDefinition : ScriptableObject
    {
        [SerializeField] private Projectile prefab;

        [Tooltip("Seconds before an unspent projectile is returned to the pool.")]
        [SerializeField, Min(0.05f)] private float lifetime = 1.2f;

        [Tooltip("Pooled effect spawned where the projectile hits. Optional.")]
        [SerializeField] private PooledEffect impactEffect;

        [Tooltip("Pooled effect spawned when armor stops the projectile. Falls back to the impact effect.")]
        [SerializeField] private PooledEffect blockedEffect;

        [Tooltip("Pooled effect spawned where the projectile glances off, pointing along its new path. Optional.")]
        [SerializeField] private PooledEffect ricochetEffect;

        [Tooltip("Instances created up front so the first shots don't instantiate.")]
        [SerializeField, Min(0)] private int prewarmCount = 8;

        public Projectile Prefab => prefab;
        public float Lifetime => lifetime;
        public PooledEffect ImpactEffect => impactEffect;
        public PooledEffect BlockedEffect => blockedEffect;
        public PooledEffect RicochetEffect => ricochetEffect;
        public int PrewarmCount => prewarmCount;
    }
}
