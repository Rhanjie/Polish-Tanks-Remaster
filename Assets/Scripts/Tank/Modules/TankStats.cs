namespace PolishTanks.Tank.Modules
{
    /// <summary>
    /// The numbers a loadout adds up to. Produced by <see cref="TankStatsCalculator"/>
    /// and pushed into the tank components by <see cref="TankController"/>.
    /// Units: mass in kg, power-to-weight in hp per tonne, dispersion in degrees.
    /// Machine gun fields stay zero when no machine gun is fitted.
    /// </summary>
    public sealed class TankStats
    {
        public float TotalMass;
        public float HorsePower;
        public float PowerToWeight;
        public bool IsOverloaded;
        public TankDriveSettings Drive;

        public TurretSettings Turret;
        public GunSettings Gun;
        public float GunCaliber;
        public float MuzzleVelocity;
        public float GunDamage;
        public float Penetration;
        public float GunDispersion;
        public int AmmoCapacity;

        public bool HasMachineGun;
        public GunSettings MachineGun;
        public float MachineGunFireInterval;
        public float MachineGunDispersion;
        public float MachineGunDamage;
        public int MachineGunBeltSize;
        public float MachineGunBeltChangeTime;

        public float FrontArmor;
        public float SideArmor;
        public float RearArmor;
        public float TurretFrontArmor;
        public float HitPoints;

        public float ViewRange;
        public int Cost;
    }
}
