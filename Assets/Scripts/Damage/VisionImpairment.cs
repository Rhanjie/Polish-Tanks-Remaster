using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// How badly the crew's view is spoiled, block by block, eased over time for the screen effect. A block's loss
    /// is 1 when destroyed, 0.5 when field-repaired and 0 when intact. Losses rise fast (glass shatters at once) and fall slower (the crew clears the view piece by piece).
    /// The fire alarm eases the same way while the engine burns.
    /// </summary>
    public sealed class VisionImpairment
    {
        public const int MaxBlocks = 6;

        private const float Epsilon = 0.001f;

        private readonly float[] loss = new float[MaxBlocks];
        private readonly float[] target = new float[MaxBlocks];
        private float fireTarget;

        public int Blocks { get; private set; }
        public float FireAlarm { get; private set; }

        public bool IsActive
        {
            get
            {
                if (FireAlarm > Epsilon)
                    return true;
                for (var i = 0; i < Blocks; i++)
                    if (loss[i] > Epsilon)
                        return true;
                return false;
            }
        }

        public float Loss(int block) => block >= 0 && block < MaxBlocks ? loss[block] : 0f;

        public void Aim(VehicleDamageModel model, bool burning)
        {
            Blocks = model == null ? 0 : Mathf.Min(model.VisionBlocks, MaxBlocks);
            for (var i = 0; i < MaxBlocks; i++)
                target[i] = i < Blocks
                    ? 1f - model.Effectiveness(new PartId(VehiclePart.VisionBlock, i))
                    : 0f;
            fireTarget = burning ? 1f : 0f;
        }

        public void Clear()
        {
            for (var i = 0; i < MaxBlocks; i++)
                target[i] = 0f;
            fireTarget = 0f;
        }

        public void Step(float deltaTime, float worsenRate, float recoverRate)
        {
            for (var i = 0; i < MaxBlocks; i++)
                loss[i] = Approach(loss[i], target[i], deltaTime, worsenRate, recoverRate);
            FireAlarm = Approach(FireAlarm, fireTarget, deltaTime, worsenRate, recoverRate);
        }

        private static float Approach(float current, float goal, float deltaTime, float worsenRate, float recoverRate)
        {
            var rate = goal > current ? worsenRate : recoverRate;
            return Mathf.MoveTowards(current, goal, rate * deltaTime);
        }
    }
}
