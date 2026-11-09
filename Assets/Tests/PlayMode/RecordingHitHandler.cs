using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Remembers the hits it received.</summary>
    public class RecordingHitHandler : MonoBehaviour, IProjectileHitHandler
    {
        public int HitCount { get; private set; }
        public ProjectileHit LastHit { get; private set; }

        public void OnProjectileHit(in ProjectileHit hit)
        {
            HitCount++;
            LastHit = hit;
        }
    }
}
