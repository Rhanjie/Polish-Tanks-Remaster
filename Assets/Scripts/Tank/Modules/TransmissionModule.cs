using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Transmission", fileName = "Transmission_")]
    public class TransmissionModule : TankModule
    {
        [Tooltip("Number of forward gears. Flavor for the workshop for now.")]
        [SerializeField, Min(1)] private int gearCount = 5;

        [SerializeField, Min(0.1f)] private float accelerationMultiplier = 1f;

        [Tooltip("Reverse top speed as a fraction of forward top speed.")]
        [SerializeField, Range(0.1f, 1f)] private float reverseSpeedRatio = 0.4f;

        [Tooltip("Share of engine power that reaches the tracks.")]
        [SerializeField, Range(0.5f, 1f)] private float efficiency = 0.9f;

        [Tooltip("Hull turn rate multiplier (better steering gear turns faster).")]
        [SerializeField, Min(0.1f)] private float turnRateMultiplier = 1f;

        public override ModuleSlot Slot => ModuleSlot.Transmission;

        public int GearCount => gearCount;
        public float AccelerationMultiplier => accelerationMultiplier;
        public float ReverseSpeedRatio => reverseSpeedRatio;
        public float Efficiency => efficiency;
        public float TurnRateMultiplier => turnRateMultiplier;
    }
}
