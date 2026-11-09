using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Hull", fileName = "Hull_")]
    public class HullModule : TankModule
    {
        [Header("Visuals")]
        [SerializeField] private Sprite sprite;
        [SerializeField, Min(0.1f)] private float visualScale = 1f;
        [Tooltip("Collider size in world units, at this hull's scale.")]
        [SerializeField] private Vector2 colliderSize = new Vector2(2.6f, 4.1f);
        [Tooltip("Turret ring position in hull space.")]
        [SerializeField] private Vector2 turretMount = new Vector2(0f, 0.15f);

        [Header("Protection")]
        [SerializeField, Min(0f)] private float frontArmor = 50f;
        [SerializeField, Min(0f)] private float sideArmor = 30f;
        [SerializeField, Min(0f)] private float rearArmor = 20f;
        [SerializeField, Min(1f)] private float hitPoints = 500f;

        [Header("Workshop")]
        [Tooltip("How many equipment modules fit on this hull.")]
        [SerializeField, Min(0)] private int equipmentSlots = 2;

        public override ModuleSlot Slot => ModuleSlot.Hull;

        public Sprite Sprite => sprite;
        public float VisualScale => visualScale;
        public Vector2 ColliderSize => colliderSize;
        public Vector2 TurretMount => turretMount;
        public float FrontArmor => frontArmor;
        public float SideArmor => sideArmor;
        public float RearArmor => rearArmor;
        public float HitPoints => hitPoints;
        public int EquipmentSlots => equipmentSlots;
    }
}
