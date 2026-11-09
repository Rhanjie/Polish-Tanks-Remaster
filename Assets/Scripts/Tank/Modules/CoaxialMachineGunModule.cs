using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Coaxial Machine Gun", fileName = "Mg_")]
    public class CoaxialMachineGunModule : TankModule
    {
        [SerializeField, Min(1f)] private float caliber = 7.92f;
        [SerializeField, Min(1f)] private float roundsPerMinute = 900f;
        [SerializeField, Min(0f)] private float dispersion = 2f;
        [SerializeField, Min(0f)] private float damage = 8f;

        [Tooltip("Rounds per belt before a belt change.")]
        [SerializeField, Min(1)] private int beltSize = 150;

        [Tooltip("Seconds to change a belt.")]
        [SerializeField, Min(0f)] private float beltChangeTime = 4f;

        public override ModuleSlot Slot => ModuleSlot.CoaxialMachineGun;

        public float Caliber => caliber;
        public float RoundsPerMinute => roundsPerMinute;
        public float FireInterval => 60f / roundsPerMinute;
        public float Dispersion => dispersion;
        public float Damage => damage;
        public int BeltSize => beltSize;
        public float BeltChangeTime => beltChangeTime;
    }
}
