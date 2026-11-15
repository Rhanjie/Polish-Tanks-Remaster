using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.CameraRig
{
    /// <summary>
    /// How hard a hit on the player's own tank shakes the camera. Force grows faster than the calibre, so a heavy
    /// gun lands with a real thump while a light one barely nudges the view. A shell that gets through hits harder,
    /// a ricochet softer, and anything below the minimum calibre (machine-gun fire) does not shake at all.
    /// </summary>
    public static class HitShake
    {
        public const float CaliberExponent = 1.5f;
        public const float RicochetFactor = 0.5f;
        public const float PenetrationFactor = 1.3f;

        public static float Force(float caliber, HitOutcome outcome, float minCaliber, float referenceCaliber, float forceAtReference)
        {
            if (caliber < minCaliber || referenceCaliber <= 0f || outcome == HitOutcome.Ignored)
                return 0f;
            var force = forceAtReference * Mathf.Pow(caliber / referenceCaliber, CaliberExponent);
            return outcome switch
            {
                HitOutcome.Ricochet => force * RicochetFactor,
                HitOutcome.Penetrated => force * PenetrationFactor,
                _ => force,
            };
        }
    }
}
