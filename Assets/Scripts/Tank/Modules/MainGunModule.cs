using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    [CreateAssetMenu(menuName = "Polish Tanks/Modules/Main Gun", fileName = "Gun_")]
    public class MainGunModule : TankModule
    {
        [SerializeField, Min(1f)] private float caliber = 75f;
        [SerializeField, Min(0.05f)] private float reloadTime = 2.5f;

        [Tooltip("Muzzle velocity in m/s. Projectiles scale it to world units.")]
        [SerializeField, Min(1f)] private float muzzleVelocity = 600f;

        [SerializeField, Min(0f)] private float damage = 120f;

        [Tooltip("Armor penetration in mm.")]
        [SerializeField, Min(0f)] private float penetration = 80f;

        [Tooltip("Random spread of a shot, degrees.")]
        [SerializeField, Min(0f)] private float dispersion = 0.4f;

        [Tooltip("Shells carried before resupply.")]
        [SerializeField, Min(1)] private int ammoCapacity = 60;

        [Tooltip("Muzzle flash size multiplier.")]
        [SerializeField, Min(0.1f)] private float muzzleFlashScale = 1f;

        public override ModuleSlot Slot => ModuleSlot.MainGun;

        public float Caliber => caliber;
        public float ReloadTime => reloadTime;
        public float MuzzleVelocity => muzzleVelocity;
        public float Damage => damage;
        public float Penetration => penetration;
        public float Dispersion => dispersion;
        public int AmmoCapacity => ammoCapacity;
        public float MuzzleFlashScale => muzzleFlashScale;
    }
}
