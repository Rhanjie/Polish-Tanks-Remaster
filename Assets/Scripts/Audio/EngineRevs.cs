using UnityEngine;

namespace PolishTanks.Audio
{
    /// <summary>
    /// Engine load from 0 (idle) to 1 (flat out): the higher of the tank's speed share and what the driver asks for,
    /// so the engine revs up as soon as the throttle or a pivot turn is pressed, before the hull gets moving.
    /// </summary>
    public static class EngineRevs
    {
        public const float SteerLoad = 0.6f;

        public static float Target(float speed, float maxSpeed, float throttle, float steer)
        {
            var speedShare = maxSpeed > 0f ? Mathf.Abs(speed) / maxSpeed : 0f;
            var demand = Mathf.Max(Mathf.Abs(throttle), Mathf.Abs(steer) * SteerLoad);
            return Mathf.Clamp01(Mathf.Max(speedShare, demand));
        }

        public static float Approach(float current, float target, float riseRate, float fallRate, float deltaTime)
        {
            var rate = target > current ? riseRate : fallRate;
            return Mathf.MoveTowards(current, target, rate * deltaTime);
        }
    }
}
