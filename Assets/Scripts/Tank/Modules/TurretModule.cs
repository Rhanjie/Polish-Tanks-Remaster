using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Turret", fileName = "Turret_")]
    public class TurretModule : TankModule
    {
        [Header("Visuals")]
        [Tooltip("Turret sprite with the barrel pointing right (+X), pivot on the turret ring.")]
        [SerializeField] private Sprite sprite;
        [SerializeField, Min(0.1f)] private float visualScale = 1f;
        [Tooltip("Distance from the turret ring to the barrel tip, world units at this turret's scale.")]
        [SerializeField, Min(0f)] private float muzzleDistance = 4f;

        [Header("Traverse")]
        [SerializeField, Min(1f)] private float maxTraverseSpeed = 90f;
        [SerializeField, Min(1f)] private float traverseAcceleration = 240f;
        [SerializeField, Min(0.01f)] private float responseTime = 0.25f;

        [Tooltip("Heaviest gun (kg) this turret turns at full speed. Heavier guns slow the traverse.")]
        [SerializeField, Min(1f)] private float maxGunMass = 2500f;

        [Header("Protection")]
        [SerializeField, Min(0f)] private float frontArmor = 60f;
        [SerializeField, Min(0f)] private float sideArmor = 40f;

        public override ModuleSlot Slot => ModuleSlot.Turret;

        public Sprite Sprite => sprite;
        public float VisualScale => visualScale;
        public float MuzzleDistance => muzzleDistance;
        public float MaxTraverseSpeed => maxTraverseSpeed;
        public float TraverseAcceleration => traverseAcceleration;
        public float ResponseTime => responseTime;
        public float MaxGunMass => maxGunMass;
        public float FrontArmor => frontArmor;
        public float SideArmor => sideArmor;
    }
}
