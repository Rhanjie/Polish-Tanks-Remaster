namespace PolishTanks.Weapons
{
    /// <summary>One shared pool per prefab for the lifetime of a scene.</summary>
    public interface IPoolService
    {
        PrefabPool<T> For<T>(T prefab, int prewarm = 0) where T : PooledBehaviour;
    }
}
