using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>Extra plates or skirts: more armor and hit points for more mass.</summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Equipment/Armor Kit", fileName = "Armor_")]
    public class ArmorKitModule : EquipmentModule
    {
        [SerializeField, Min(0f)] private float extraFrontArmor;
        [SerializeField, Min(0f)] private float extraSideArmor;
        [SerializeField, Min(0f)] private float extraHitPoints;

        public float ExtraFrontArmor => extraFrontArmor;
        public float ExtraSideArmor => extraSideArmor;
        public float ExtraHitPoints => extraHitPoints;

        public override void Modify(TankStats stats)
        {
            stats.FrontArmor += extraFrontArmor;
            stats.SideArmor += extraSideArmor;
            stats.HitPoints += extraHitPoints;
        }
    }
}
