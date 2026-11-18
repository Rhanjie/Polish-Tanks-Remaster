using System;
using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// One explosion's numbers: how far it reaches, how much armor it can still defeat right at the plate (falling off
    /// towards the edge), the damage a defeated plate lets through, the caliber the armor and module rules see, the push
    /// on loose objects and the effect it leaves.
    /// </summary>
    [Serializable]
    public struct BlastSettings
    {
        [Min(0.1f)] public float radius;
        [Min(0f)] public float penetrationMin;
        [Min(0f)] public float penetrationMax;
        [Min(0f)] public float damage;
        [Tooltip("Caliber in mm the armor and module rules treat the blast as. 20 or more counts as a heavy hit, not small arms.")]
        [Min(0f)] public float caliber;
        [Tooltip("Push on loose props at the centre, fading to nothing at the edge.")]
        [Min(0f)] public float impulse;
        public PooledEffect effect;
    }
}
