using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>A complete tank build: one module per slot plus optional equipment.</summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Tank Loadout", fileName = "Loadout_")]
    public class TankLoadout : ScriptableObject
    {
        [SerializeField] private string displayName;

        [Header("Required")]
        [SerializeField] private HullModule hull;
        [SerializeField] private EngineModule engine;
        [SerializeField] private TransmissionModule transmission;
        [SerializeField] private TracksModule tracks;
        [SerializeField] private TurretModule turret;
        [SerializeField] private MainGunModule mainGun;

        [Header("Optional")]
        [SerializeField] private CoaxialMachineGunModule coaxialMachineGun;
        [SerializeField] private OpticsModule optics;
        [SerializeField] private List<EquipmentModule> equipment = new List<EquipmentModule>();

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public HullModule Hull => hull;
        public EngineModule Engine => engine;
        public TransmissionModule Transmission => transmission;
        public TracksModule Tracks => tracks;
        public TurretModule Turret => turret;
        public MainGunModule MainGun => mainGun;
        public CoaxialMachineGunModule CoaxialMachineGun => coaxialMachineGun;
        public OpticsModule Optics => optics;
        public IReadOnlyList<EquipmentModule> Equipment => equipment;

        public IEnumerable<TankModule> AllModules()
        {
            TankModule[] fixedSlots = { hull, engine, transmission, tracks, turret, mainGun, coaxialMachineGun, optics };
            foreach (var module in fixedSlots)
                if (module != null)
                    yield return module;
            foreach (var item in equipment)
                if (item != null)
                    yield return item;
        }
    }
}
