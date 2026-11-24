using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// How long a hit knocks a vehicle off its controls: the velocity change it caused times
    /// <see cref="SecondsPerSpeed"/>, capped at <see cref="MaxSeconds"/>. Small knocks below
    /// <see cref="MinSeconds"/> are ignored, so heavy tanks shrug off light guns.
    /// </summary>
    public static class HitStagger
    {
        public const float SecondsPerSpeed = 1f;
        public const float MinSeconds = 0.1f;
        public const float MaxSeconds = 1.5f;

        public static float Duration(float impulse, float mass)
        {
            if (mass <= 0f || impulse <= 0f)
                return 0f;
            var seconds = impulse / mass * SecondsPerSpeed;
            return seconds < MinSeconds ? 0f : Mathf.Min(seconds, MaxSeconds);
        }
    }
}
