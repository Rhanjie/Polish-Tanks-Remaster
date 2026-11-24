using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>A machine gun in the turret next to the main gun, so it points wherever the turret does.</summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Coaxial Machine Gun", fileName = "Mg_")]
    public class CoaxialMachineGunModule : MachineGunModule
    {
        public override ModuleSlot Slot => ModuleSlot.CoaxialMachineGun;
    }
}
