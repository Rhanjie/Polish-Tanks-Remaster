using UnityEngine;

namespace PolishTanks.Weapons
{
    public static class BallisticsMath
    {
        /// <summary>Rotates a vector counter-clockwise by the given angle.</summary>
        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        /// <summary>
        /// Deviates an aim direction by up to <paramref name="spreadDegrees"/> either way.
        /// Two uniform randoms are averaged, so shots bunch toward the center.
        /// </summary>
        /// <param name="random1">Uniform random in [0, 1].</param>
        /// <param name="random2">Uniform random in [0, 1].</param>
        public static Vector2 ApplySpread(Vector2 direction, float spreadDegrees, float random1, float random2)
        {
            if (spreadDegrees <= 0f)
                return direction.normalized;
            var t = (Mathf.Clamp01(random1) + Mathf.Clamp01(random2)) - 1f; // -1..1, triangular
            return Rotate(direction.normalized, t * spreadDegrees);
        }

        /// <summary>Muzzle velocity plus the shooter's own motion, so shots from a moving tank don't lag behind.</summary>
        public static Vector2 LaunchVelocity(Vector2 direction, float speed, Vector2 shooterVelocity) =>
            direction.normalized * speed + shooterVelocity;
    }
}
