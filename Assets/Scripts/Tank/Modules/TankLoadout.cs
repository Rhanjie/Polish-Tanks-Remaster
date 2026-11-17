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
        [SerializeField] private HullMachineGunModule hullMachineGun;
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
        public HullMachineGunModule HullMachineGun => hullMachineGun;
        public OpticsModule Optics => optics;
        public IReadOnlyList<EquipmentModule> Equipment => equipment;

        public bool IsRuntime { get; private set; }

        public TankModule Get(ModuleSlot slot)
        {
            switch (slot)
            {
                case ModuleSlot.Hull: return hull;
                case ModuleSlot.Engine: return engine;
                case ModuleSlot.Transmission: return transmission;
                case ModuleSlot.Tracks: return tracks;
                case ModuleSlot.Turret: return turret;
                case ModuleSlot.MainGun: return mainGun;
                case ModuleSlot.CoaxialMachineGun: return coaxialMachineGun;
                case ModuleSlot.HullMachineGun: return hullMachineGun;
                case ModuleSlot.Optics: return optics;
                default: throw new System.ArgumentException($"{slot} holds a list, read Equipment instead.", nameof(slot));
            }
        }

        internal static TankLoadout CreateRuntime(string displayName, IReadOnlyDictionary<ModuleSlot, TankModule> slots,
            IEnumerable<EquipmentModule> equipment)
        {
            var loadout = CreateInstance<TankLoadout>();
            loadout.name = displayName;
            loadout.displayName = displayName;
            loadout.hideFlags = HideFlags.DontSave;
            loadout.IsRuntime = true;
            loadout.hull = Slot<HullModule>(slots, ModuleSlot.Hull);
            loadout.engine = Slot<EngineModule>(slots, ModuleSlot.Engine);
            loadout.transmission = Slot<TransmissionModule>(slots, ModuleSlot.Transmission);
            loadout.tracks = Slot<TracksModule>(slots, ModuleSlot.Tracks);
            loadout.turret = Slot<TurretModule>(slots, ModuleSlot.Turret);
            loadout.mainGun = Slot<MainGunModule>(slots, ModuleSlot.MainGun);
            loadout.coaxialMachineGun = Slot<CoaxialMachineGunModule>(slots, ModuleSlot.CoaxialMachineGun);
            loadout.hullMachineGun = Slot<HullMachineGunModule>(slots, ModuleSlot.HullMachineGun);
            loadout.optics = Slot<OpticsModule>(slots, ModuleSlot.Optics);
            loadout.equipment = new List<EquipmentModule>(equipment);
            return loadout;
        }

        private static T Slot<T>(IReadOnlyDictionary<ModuleSlot, TankModule> slots, ModuleSlot slot) where T : TankModule
        {
            return slots.TryGetValue(slot, out var module) ? module as T : null;
        }

        public IEnumerable<TankModule> AllModules()
        {
            TankModule[] fixedSlots = { hull, engine, transmission, tracks, turret, mainGun, coaxialMachineGun, hullMachineGun, optics };
            foreach (var module in fixedSlots)
                if (module != null)
                    yield return module;
            foreach (var item in equipment)
                if (item != null)
                    yield return item;
        }
    }
}
