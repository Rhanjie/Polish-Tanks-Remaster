namespace PolishTanks.Tank.Modules
{
    /// <summary>Where a module goes on a tank. Each slot except Equipment holds exactly one module.</summary>
    public enum ModuleSlot
    {
        Hull,
        Engine,
        Transmission,
        Tracks,
        Turret,
        MainGun,
        CoaxialMachineGun,
        Optics,
        Equipment,
    }
}
