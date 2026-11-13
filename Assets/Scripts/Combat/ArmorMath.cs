using UnityEngine;

namespace PolishTanks.Combat
{
    /// <summary>
    /// Armor rules. The plate is picked from the hit surface normal against the target's forward axis
    /// (front and rear plates cover 45 degrees either side). Box-shaped targets snap the normal to their nearest
    /// face first, since a circle cast against a corner reports a normal pointing back along the shot.
    /// Only faces the shot is travelling into count, so a corner graze never picks the plate it is leaving.
    /// Effective thickness is nominal armor over the cosine of the impact angle, capped so a grazing hit
    /// counts as at most <see cref="MaxImpactAngle"/>.
    /// </summary>
    public static class ArmorMath
    {
        public const float FrontRearHalfArc = 45f;
        public const float MaxImpactAngle = 75f;
        public const float OverpenetrationFactor = 2f;

        public static Vector2 SnapToFace(Vector2 localNormal)
        {
            if (localNormal.sqrMagnitude <= 0f)
                return Vector2.up;
            return Mathf.Abs(localNormal.x) > Mathf.Abs(localNormal.y)
                ? new Vector2(Mathf.Sign(localNormal.x), 0f)
                : new Vector2(0f, Mathf.Sign(localNormal.y));
        }

        public static Vector2 SnapToFace(Vector2 localNormal, Vector2 localShotDirection)
        {
            var dominant = SnapToFace(localNormal);
            if (Vector2.Dot(dominant, localShotDirection) < 0f)
                return dominant;

            var other = dominant.x != 0f
                ? new Vector2(0f, localNormal.y != 0f ? Mathf.Sign(localNormal.y) : -Mathf.Sign(localShotDirection.y))
                : new Vector2(localNormal.x != 0f ? Mathf.Sign(localNormal.x) : -Mathf.Sign(localShotDirection.x), 0f);
            return Vector2.Dot(other, localShotDirection) < 0f ? other : dominant;
        }

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
