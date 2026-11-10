using UnityEngine;

namespace PolishTanks.Combat
{
    /// <summary>
    /// Ricochet rules. Near-square hits never bounce: they penetrate or break up on the plate.
    /// From <see cref="StartAngle"/> to <see cref="FullAngle"/> (degrees from the plate normal) the chance rises
    /// to certain. A shell at least <see cref="OvermatchRatio"/> times the plate thickness punches in instead.
    /// The bounce mirrors off the plate, skims flatter along it and scatters a little.
    /// </summary>
    public static class RicochetMath
    {
        public const float StartAngle = 55f;
        public const float FullAngle = 70f;
        public const float OvermatchRatio = 3f;
        public const float RetainedPower = 0.3f;
        public const float RetainedSpeed = 0.6f;
        public const float SkimFactor = 0.35f;
        public const float ScatterDegrees = 10f;
        public const float MinLeaveDot = 0.05f;

        public static bool IsOvermatch(float caliber, float plateThickness) =>
            plateThickness <= 0f || caliber >= plateThickness * OvermatchRatio;

        public static float Chance(float impactAngle, bool overmatch)
        {
            if (overmatch)
                return 0f;
            return Mathf.InverseLerp(StartAngle, FullAngle, impactAngle);
        }

        public static bool Rolls(float impactAngle, bool overmatch, float random01) =>
            Chance(impactAngle, overmatch) > Mathf.Clamp01(random01);

        public static Vector2 Deflect(Vector2 direction, Vector2 surfaceNormal, float random01)
        {
            var normal = surfaceNormal.normalized;
            var reflected = Vector2.Reflect(direction.normalized, normal);
            var along = reflected - Vector2.Dot(reflected, normal) * normal;
            var skimmed = along.sqrMagnitude > 0f
                ? Vector2.Lerp(reflected, along.normalized, SkimFactor).normalized
                : reflected;

            var scattered = Rotate(skimmed, (Mathf.Clamp01(random01) * 2f - 1f) * ScatterDegrees);
            if (Vector2.Dot(scattered, normal) > MinLeaveDot)
                return scattered;
            if (Vector2.Dot(skimmed, normal) > MinLeaveDot)
                return skimmed;
            return along.sqrMagnitude > 0f ? (along.normalized + normal * MinLeaveDot * 2f).normalized : normal;
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
