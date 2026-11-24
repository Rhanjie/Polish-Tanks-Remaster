using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>Better shell stowage: more ammo and/or a faster loader.</summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Equipment/Ammo Rack", fileName = "AmmoRack_")]
    public class AmmoRackModule : EquipmentModule
    {
        [SerializeField, Min(0)] private int extraShells;

        [Tooltip("Multiplies main gun reload time. Below 1 = faster.")]
        [SerializeField, Range(0.5f, 1.5f)] private float reloadTimeMultiplier = 1f;

        public int ExtraShells => extraShells;
        public float ReloadTimeMultiplier => reloadTimeMultiplier;

        public override void Modify(TankStats stats)
        {
            stats.AmmoCapacity += extraShells;
            var gun = stats.Gun;
            gun.reloadTime *= reloadTimeMultiplier;
            stats.Gun = gun;
        }
    }
}
