using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Remembers the hits it received.</summary>
    public class RecordingHitHandler : MonoBehaviour, IProjectileHitHandler
    {
        public int HitCount { get; private set; }
        public ProjectileHit LastHit { get; private set; }

        public HitOutcome Outcome { get; set; } = HitOutcome.Ignored;

        public HitResponse OnProjectileHit(in ProjectileHit hit)
        {
            HitCount++;
            LastHit = hit;
            return new HitResponse(Outcome, hit.Normal);
        }
    }
}
