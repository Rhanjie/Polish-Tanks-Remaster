using System;

namespace PolishTanks.Damage
{
    /// <summary>
    /// Which damageable parts a vehicle has and how likely a qualifying hit is to knock each one out.
    /// A tank has all of them; a lighter vehicle may lack tracks, a turret or vision blocks.
    /// </summary>
    [Serializable]
    public struct PartDamageChances
    {
        public bool HasTracks;
        public bool HasTurretRing;
        public int VisionBlocks;

        public float TrackBreak;
        public float EngineBreak;
        public float TurretRingJam;
        public float VisionBlockBreak;
    }
}
