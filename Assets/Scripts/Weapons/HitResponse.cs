using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// A handler's answer to a hit: the outcome and the plate normal it judged it by, used to bounce ricochets.
    /// A penetrating answer can also let the shot pass through and fly on with the penetration it has left.
    /// </summary>
    public readonly struct HitResponse
    {
        public readonly HitOutcome Outcome;
        public readonly Vector2 Normal;
        public readonly SurfaceMaterial Material;
        public readonly bool PassesThrough;
        public readonly float ResidualPenetration;

        public HitResponse(HitOutcome outcome, Vector2 normal, SurfaceMaterial material = SurfaceMaterial.Other)
            : this(outcome, normal, material, false, 0f)
        {
        }

        private HitResponse(HitOutcome outcome, Vector2 normal, SurfaceMaterial material, bool passesThrough,
            float residualPenetration)
        {
            Outcome = outcome;
            Normal = normal;
            Material = material;
            PassesThrough = passesThrough;
            ResidualPenetration = residualPenetration;
        }

        public static HitResponse PassThrough(Vector2 normal, SurfaceMaterial material, float residualPenetration) =>
            new HitResponse(HitOutcome.Penetrated, normal, material, true, residualPenetration);

        public static HitResponse Ignored => new HitResponse(HitOutcome.Ignored, Vector2.zero);
    }
}
