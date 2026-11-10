using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>A handler's answer to a hit: the outcome and the plate normal it judged it by, used to bounce ricochets.</summary>
    public readonly struct HitResponse
    {
        public readonly HitOutcome Outcome;
        public readonly Vector2 Normal;
        public readonly SurfaceMaterial Material;

        public HitResponse(HitOutcome outcome, Vector2 normal, SurfaceMaterial material = SurfaceMaterial.Other)
        {
            Outcome = outcome;
            Normal = normal;
            Material = material;
        }

        public static HitResponse Ignored => new HitResponse(HitOutcome.Ignored, Vector2.zero);
    }
}
