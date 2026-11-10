using UnityEngine;

namespace PolishTanks.Combat
{
    /// <summary>
    /// Armor rules. The plate is picked from the hit surface normal against the target's forward axis
    /// (front and rear plates cover 45 degrees either side). Effective thickness is nominal armor over the
    /// cosine of the impact angle, capped so a grazing hit counts as at most <see cref="MaxImpactAngle"/>.
    /// </summary>
    public static class ArmorMath
    {
        public const float FrontRearHalfArc = 45f;
        public const float MaxImpactAngle = 75f;

        public static HitSide GetSide(Vector2 targetForward, Vector2 surfaceNormal)
        {
            var angle = Vector2.Angle(targetForward, surfaceNormal);
            if (angle <= FrontRearHalfArc)
                return HitSide.Front;
            if (angle >= 180f - FrontRearHalfArc)
                return HitSide.Rear;
            return HitSide.Side;
        }

        public static float ImpactAngle(Vector2 surfaceNormal, Vector2 shotDirection) =>
            Mathf.Min(Vector2.Angle(surfaceNormal, -shotDirection), MaxImpactAngle);

        public static float EffectiveArmor(float nominal, Vector2 surfaceNormal, Vector2 shotDirection) =>
            nominal / Mathf.Cos(ImpactAngle(surfaceNormal, shotDirection) * Mathf.Deg2Rad);

        public static float RollPenetration(float min, float max, float random01) =>
            Mathf.Lerp(Mathf.Min(min, max), Mathf.Max(min, max), Mathf.Clamp01(random01));

        public static PenetrationResult Resolve(ArmorProfile armor, Vector2 targetForward, Vector2 surfaceNormal,
            Vector2 shotDirection, float penetrationMin, float penetrationMax, float random01)
        {
            var side = GetSide(targetForward, surfaceNormal);
            var effective = EffectiveArmor(armor.For(side), surfaceNormal, shotDirection);
            var roll = RollPenetration(penetrationMin, penetrationMax, random01);
            return new PenetrationResult(side, effective, roll);
        }
    }
}
