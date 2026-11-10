using UnityEngine;

namespace PolishTanks.Weapons
{
    public static class BallisticsMath
    {
        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        public static Vector2 ApplySpread(Vector2 direction, float spreadDegrees, float random1, float random2)
        {
            if (spreadDegrees <= 0f)
                return direction.normalized;
            var t = (Mathf.Clamp01(random1) + Mathf.Clamp01(random2)) - 1f;
            return Rotate(direction.normalized, t * spreadDegrees);
        }

        public static Vector2 LaunchVelocity(Vector2 direction, float speed, Vector2 shooterVelocity) =>
            direction.normalized * speed + shooterVelocity;
    }
}
