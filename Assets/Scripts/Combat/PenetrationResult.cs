using UnityEngine;

namespace PolishTanks.Combat
{
    /// <summary>
    /// Outcome of one shell against one plate: which plate, how thick it really was and what the shell managed.
    /// A shell with at least <see cref="ArmorMath.OverpenetrationFactor"/> times the plate's effective armor goes
    /// out the far side too and flies on with what is left after paying for both walls.
    /// </summary>
    public readonly struct PenetrationResult
    {
        public readonly HitSide Side;
        public readonly float EffectiveArmor;
        public readonly float Penetration;

        public PenetrationResult(HitSide side, float effectiveArmor, float penetration)
        {
            Side = side;
            EffectiveArmor = effectiveArmor;
            Penetration = penetration;
        }

        public bool Penetrated => Penetration >= EffectiveArmor;
        public bool Overpenetrated => Penetration >= EffectiveArmor * ArmorMath.OverpenetrationFactor;
        public float ResidualPenetration => Mathf.Max(0f, Penetration - EffectiveArmor * ArmorMath.OverpenetrationFactor);
    }
}
