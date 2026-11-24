using PolishTanks.Damage;
using UnityEngine;

namespace PolishTanks.Rendering
{
    /// <summary>
    /// What the vision damage screen effect should draw this frame: the seed that lays out each vision block's
    /// share of the screen, how much each block is spoiled and how brightly a blinding hit flashes each one. A scene component publishes it and the renderer
    /// feature reads it, because renderer features live in the pipeline asset and cannot be injected. While nothing
    /// is published the effect adds no pass at all.
    /// </summary>
    public static class VisionDamageState
    {
        public static bool IsActive { get; private set; }
        public static float Seed { get; private set; }
        public static int Blocks { get; private set; }
        public static Vector4 LossA { get; private set; }
        public static Vector4 LossB { get; private set; }
        public static Vector4 FlashA { get; private set; }
        public static Vector4 FlashB { get; private set; }
        public static float FireAlarm { get; private set; }

        public static void Publish(float seed, VisionImpairment impairment)
        {
            IsActive = impairment.IsActive;
            Seed = seed;
            Blocks = impairment.Blocks;
            LossA = new Vector4(impairment.Loss(0), impairment.Loss(1), impairment.Loss(2), impairment.Loss(3));
            LossB = new Vector4(impairment.Loss(4), impairment.Loss(5), 0f, 0f);
            FlashA = new Vector4(impairment.Flash(0), impairment.Flash(1), impairment.Flash(2), impairment.Flash(3));
            FlashB = new Vector4(impairment.Flash(4), impairment.Flash(5), 0f, 0f);
            FireAlarm = impairment.FireAlarm;
        }

        public static void Clear() => IsActive = false;
    }
}
