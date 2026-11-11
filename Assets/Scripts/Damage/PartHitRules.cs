using System;
using System.Collections.Generic;
using PolishTanks.Combat;
using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// Which parts a hit can knock out, by the plate it struck. Side hits threaten the track on that side,
    /// even when the shell does not get through (scaled down). Everything else needs a penetration:
    /// the engine from the rear, vision blocks from the front, the turret ring from any side.
    /// Each part rolls on its own, so one shell can do several kinds of damage.
    /// </summary>
    public static class PartHitRules
    {
        public static void Roll(HitSide side, bool leftSide, bool penetrated, in PartDamageChances chances,
            float nonPenetratingTrackFactor, Func<float> random, List<VehiclePart> result)
        {
            result.Clear();

            if (side == HitSide.Side && chances.HasTracks)
            {
                var trackChance = chances.TrackBreak * (penetrated ? 1f : Mathf.Clamp01(nonPenetratingTrackFactor));
                if (Hits(trackChance, random))
                    result.Add(leftSide ? VehiclePart.LeftTrack : VehiclePart.RightTrack);
            }

            if (!penetrated)
                return;

            if (side == HitSide.Rear && Hits(chances.EngineBreak, random))
                result.Add(VehiclePart.Engine);
            if (side == HitSide.Front && chances.VisionBlocks > 0 && Hits(chances.VisionBlockBreak, random))
                result.Add(VehiclePart.VisionBlock);
            if (chances.HasTurretRing && Hits(chances.TurretRingJam, random))
                result.Add(VehiclePart.TurretRing);
        }

        private static bool Hits(float chance, Func<float> random) => chance > 0f && random() < chance;
    }
}
