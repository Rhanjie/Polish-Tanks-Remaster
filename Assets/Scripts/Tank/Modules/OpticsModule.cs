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

        public override ModuleSlot Slot => ModuleSlot.Optics;

        public float ViewRange => viewRange;
        public float AccuracyMultiplier => accuracyMultiplier;
    }
}
