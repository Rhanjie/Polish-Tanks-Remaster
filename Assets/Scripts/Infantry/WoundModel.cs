using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// How much a soldier can take, as health from 1 down to 0. A cannon round (20 mm and up) kills outright. A full-power
    /// rifle round (<see cref="StrongRifleCaliber"/> and up, the tank machine guns) kills in one or two hits, a lighter
    /// round in two or three. A blast kills close to its centre and only wounds near its edge.
    /// </summary>
    public sealed class WoundModel
    {
        public const float HeavyCaliber = 20f;
        public const float StrongRifleCaliber = 7.9f;
        public const float SevereWound = 1f;
        public const float StrongRifleWound = 0.5f;
        public const float LightRifleWound = 0.5f;
        public const float GrazeWound = 0.34f;
        public const float BlastCentreWound = 1.2f;
        public const float BlastEdgeWound = 0.3f;

        public float Health { get; private set; } = 1f;
        public bool IsDead => Health <= 0f;

        public static float RifleWound(float caliber, float random01)
        {
            if (caliber >= HeavyCaliber)
                return SevereWound;
            if (caliber >= StrongRifleCaliber)
                return random01 < 0.5f ? SevereWound : StrongRifleWound;
            return random01 < 0.5f ? LightRifleWound : GrazeWound;
        }

        public static float BlastWound(float distance, float radius) =>
            radius <= 0f || distance > radius ? 0f : Mathf.Lerp(BlastCentreWound, BlastEdgeWound, distance / radius);

        public bool TakeHit(float caliber, float random01) => Take(RifleWound(caliber, random01));

        public bool TakeBlast(float distance, float radius) => Take(BlastWound(distance, radius));

        public void Kill() => Health = 0f;

        private bool Take(float wound)
        {
            if (IsDead || wound <= 0f)
                return false;
            Health -= wound;
            return IsDead;
        }
    }
}
