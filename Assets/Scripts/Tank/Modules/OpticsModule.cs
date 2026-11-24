using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Optics", fileName = "Optics_")]
    public class OpticsModule : TankModule
    {
        [Tooltip("How far the crew sees: the follow camera's orthographic size.")]
        [SerializeField, Min(1f)] private float viewRange = 9f;

        [Tooltip("Multiplies weapon dispersion. Below 1 = more accurate.")]
        [SerializeField, Min(0.1f)] private float accuracyMultiplier = 1f;

        [Tooltip("Vision blocks the crew looks through. Each one lost takes an equal share of visibility.")]
        [SerializeField, Range(1, 6)] private int visionBlocks = 4;

        [Tooltip("Chance a penetrating front hit breaks one vision block.")]
        [SerializeField, Range(0f, 1f)] private float blockBreakChance = 0.3f;

        public override ModuleSlot Slot => ModuleSlot.Optics;

        public float ViewRange => viewRange;
        public float AccuracyMultiplier => accuracyMultiplier;
        public int VisionBlocks => visionBlocks;
        public float BlockBreakChance => blockBreakChance;
    }
}
