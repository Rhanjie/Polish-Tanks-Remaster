using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>
    /// A machine gun in a ball mount on the front plate, by the driver. It swings only a little to each side of the
    /// hull's nose, so it covers what the tank is driving at rather than where the turret looks.
    /// Fits only hulls with a mount for it.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Hull Machine Gun", fileName = "MgHull_")]
    public class HullMachineGunModule : MachineGunModule
    {
        [Tooltip("How far the ball mount swings to each side of the hull's nose, degrees.")]
        [SerializeField, Range(0f, 45f)] private float arc = 15f;

        [Tooltip("How fast the gunner swings the mount, degrees per second.")]
        [SerializeField, Min(1f)] private float traverseSpeed = 90f;

        public override ModuleSlot Slot => ModuleSlot.HullMachineGun;

        public float Arc => arc;
        public float TraverseSpeed => traverseSpeed;
    }
}
