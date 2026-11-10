namespace PolishTanks.Combat
{
    /// <summary>Outcome of one shell against one plate: which plate, how thick it really was and what the shell managed.</summary>
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
    }
}
