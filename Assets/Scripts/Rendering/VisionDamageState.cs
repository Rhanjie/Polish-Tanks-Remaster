using PolishTanks.Damage;
using UnityEngine;

namespace PolishTanks.Rendering
{
    /// <summary>
    /// What the vision damage screen effect should draw this frame: where the player's tank stands, the seed that
    /// places each vision block's damaged patches on the screen and how much each block is spoiled. A scene component publishes it and the
    /// renderer feature reads it, because renderer features live in the pipeline asset and cannot be injected.
    /// While nothing is published the effect adds no pass at all.
    /// </summary>
    public static class VisionDamageState
    {
        public static bool IsActive { get; private set; }
        public static Vector2 Position { get; private set; }
        public static float Seed { get; private set; }
        public static float ClearRadius { get; private set; }
        public static int Blocks { get; private set; }
        public static Vector4 LossA { get; private set; }
        public static Vector4 LossB { get; private set; }
        public static float FireAlarm { get; private set; }

        public static void Publish(Vector2 position, float clearRadius, float seed, VisionImpairment impairment)
        {
            IsActive = impairment.IsActive;
            Position = position;
            Seed = seed;
            ClearRadius = clearRadius;
            Blocks = impairment.Blocks;
            LossA = new Vector4(impairment.Loss(0), impairment.Loss(1), impairment.Loss(2), impairment.Loss(3));
            LossB = new Vector4(impairment.Loss(4), impairment.Loss(5), 0f, 0f);
            FireAlarm = impairment.FireAlarm;
        }

        public static void Clear() => IsActive = false;
    }
}
