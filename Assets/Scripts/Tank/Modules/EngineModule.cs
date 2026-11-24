using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Engine", fileName = "Engine_")]
    public class EngineModule : TankModule
    {
        [Tooltip("Engine power in horsepower. Power-to-weight drives top speed and acceleration.")]
        [SerializeField, Min(1f)] private float horsePower = 300f;

        [Tooltip("Turret traverse multiplier from a powered (hydraulic/electric) drive. 1 = no bonus.")]
        [SerializeField, Min(0.1f)] private float traverseDriveMultiplier = 1f;

        [Tooltip("1 = never fails. A penetrating rear hit knocks the engine out with a chance of 1 minus this.")]
        [SerializeField, Range(0f, 1f)] private float reliability = 0.85f;

        [Tooltip("Fuel tank capacity in litres. Reserved for a fuel system.")]
        [SerializeField, Min(0f)] private float fuelCapacity = 400f;

        public override ModuleSlot Slot => ModuleSlot.Engine;

        public float HorsePower => horsePower;
        public float TraverseDriveMultiplier => traverseDriveMultiplier;
        public float Reliability => reliability;
        public float FuelCapacity => fuelCapacity;
    }
}
