namespace PolishTanks.Weapons
{
    /// <summary>Implemented by components that react to being hit (damage, effects). Found via GetComponentInParent on the hit collider.</summary>
    public interface IProjectileHitHandler
    {
        void OnProjectileHit(in ProjectileHit hit);
    }
}
