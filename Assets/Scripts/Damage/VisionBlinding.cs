using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// Short flashes that blind single vision blocks, for example bullets splashing on the glass. Each block counts
    /// down on its own; a new flash on a blinded block restarts its clock. Visibility here combines the blocks' damage
    /// with the flashes, so an AI crew sees less while it lasts.
    /// </summary>
    public sealed class VisionBlinding
    {
        private readonly float[] remaining = new float[VisionImpairment.MaxBlocks];
        private readonly float[] strength = new float[VisionImpairment.MaxBlocks];

        public bool IsAnyBlinded
        {
            get
            {
                foreach (var seconds in remaining)
                    if (seconds > 0f)
                        return true;
                return false;
            }
        }

        public void Flash(int block, float seconds, float amount)
        {
            if (block < 0 || block >= remaining.Length || seconds <= 0f)
                return;
            remaining[block] = Mathf.Max(remaining[block], seconds);
            strength[block] = Mathf.Clamp01(amount);
        }

        public float Amount(int block) =>
            block >= 0 && block < remaining.Length && remaining[block] > 0f ? strength[block] : 0f;

        public void Tick(float deltaTime)
        {
            for (var i = 0; i < remaining.Length; i++)
                remaining[i] = Mathf.Max(0f, remaining[i] - Mathf.Max(0f, deltaTime));
        }

        public void Clear()
        {
            for (var i = 0; i < remaining.Length; i++)
                remaining[i] = 0f;
        }

        public float Visibility(VehicleDamageModel model)
        {
            if (model == null || model.VisionBlocks == 0)
                return 1f;
            var sum = 0f;
            for (var i = 0; i < model.VisionBlocks; i++)
                sum += Mathf.Min(model.Effectiveness(new PartId(VehiclePart.VisionBlock, i)), 1f - Amount(i));
            return sum / model.VisionBlocks;
        }
    }
}
