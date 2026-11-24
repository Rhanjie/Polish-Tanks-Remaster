using PolishTanks.Weapons;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Minimal pooled object that counts its lifecycle callbacks.</summary>
    public class TestPooled : PooledBehaviour
    {
        public int SpawnedCount { get; private set; }
        public int ReturnedCount { get; private set; }

        protected override void OnSpawned() => SpawnedCount++;

        protected override void OnReturned() => ReturnedCount++;
    }
}
