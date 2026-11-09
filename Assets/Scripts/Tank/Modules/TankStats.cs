namespace PolishTanks.Tank.Modules
{
    /// <summary>
    /// The numbers a loadout adds up to. Produced by <see cref="TankStatsCalculator"/>
    /// and pushed into the tank components by <see cref="TankController"/>.
    /// </summary>
    public sealed class TankStats
    {
        // Mobility
        public float TotalMass;         // kg
        public float HorsePower;
        public float PowerToWeight;     // hp per tonne
        public bool IsOverloaded;
        public TankDriveSettings Drive;

        // Turret and main gun
        public TurretSettings Turret;
        public GunSettings Gun;
        public float GunCaliber;
        public float MuzzleVelocity;
        public float GunDamage;
        public float Penetration;
        public float GunDispersion;     // degrees
        public int AmmoCapacity;

        // Coaxial machine gun (zeroes when none is fitted)
        public bool HasMachineGun;
        public GunSettings MachineGun;
        public float MachineGunFireInterval;
        public float MachineGunDispersion;
        public float MachineGunDamage;
        public int MachineGunBeltSize;
        public float MachineGunBeltChangeTime;

        // Protection
        public float FrontArmor;
        public float SideArmor;
        public float RearArmor;
        public float TurretFrontArmor;
        public float HitPoints;

        // Crew and workshop
        public float ViewRange;
        public int Cost;
    }
}
