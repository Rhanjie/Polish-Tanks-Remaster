using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Tracks", fileName = "Tracks_")]
    public class TracksModule : TankModule
    {
        [Tooltip("Traction. Scales acceleration and braking.")]
        [SerializeField, Min(0.1f)] private float grip = 1f;

        [SerializeField, Min(0.1f)] private float maxSpeedMultiplier = 1f;

        [Tooltip("Hull turn rate standing still, degrees per second.")]
        [SerializeField, Min(1f)] private float turnRate = 70f;

        [Tooltip("Turn rate at top speed as a fraction of the standing turn rate.")]
        [SerializeField, Range(0.1f, 1f)] private float turnRateAtSpeedRatio = 0.65f;

        [Tooltip("Total tank mass in kg these tracks carry without penalty. Heavier tanks lose speed and acceleration.")]
        [SerializeField, Min(1f)] private float maxLoad = 30000f;

        [Tooltip("Extra rolling resistance on soft ground. Reserved for terrain types.")]
        [SerializeField, Min(0f)] private float terrainResistance = 1f;

        [Tooltip("Chance a penetrating side hit throws the track on that side. Hits that stay in the plate use a fraction of it.")]
        [SerializeField, Range(0f, 1f)] private float breakChance = 0.25f;

        public override ModuleSlot Slot => ModuleSlot.Tracks;

        public float Grip => grip;
        public float MaxSpeedMultiplier => maxSpeedMultiplier;
        public float TurnRate => turnRate;
        public float TurnRateAtSpeedRatio => turnRateAtSpeedRatio;
        public float MaxLoad => maxLoad;
        public float TerrainResistance => terrainResistance;
        public float BreakChance => breakChance;
    }
}
