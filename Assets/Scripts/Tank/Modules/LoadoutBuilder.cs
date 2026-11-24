using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolishTanks.Tank.Modules
{
    /// <summary>
    /// Editable copy of a <see cref="TankLoadout"/> for swapping modules from code: start from a loadout, fit or
    /// remove modules, then <see cref="Build"/> a new runtime loadout. Fitting a hull drops what that hull can't carry
    /// (a hull machine gun without a mount, equipment over its slots), so cycling hulls never leaves an invalid build.
    /// Runtime loadouts are not assets; hand them back with <see cref="Release"/> when they are no longer used.
    /// </summary>
    public sealed class LoadoutBuilder
    {
        public static readonly IReadOnlyList<ModuleSlot> FixedSlots = new[]
        {
            ModuleSlot.Hull, ModuleSlot.Engine, ModuleSlot.Transmission, ModuleSlot.Tracks, ModuleSlot.Turret,
            ModuleSlot.MainGun, ModuleSlot.CoaxialMachineGun, ModuleSlot.HullMachineGun, ModuleSlot.Optics,
        };

        public static readonly IReadOnlyList<ModuleSlot> RequiredSlots = new[]
        {
            ModuleSlot.Hull, ModuleSlot.Engine, ModuleSlot.Transmission, ModuleSlot.Tracks, ModuleSlot.Turret,
            ModuleSlot.MainGun,
        };

        private readonly Dictionary<ModuleSlot, TankModule> slots = new Dictionary<ModuleSlot, TankModule>();
        private readonly List<EquipmentModule> equipment = new List<EquipmentModule>();

        public LoadoutBuilder(TankLoadout source)
        {
            if (source == null)
                return;
            Name = source.DisplayName;
            foreach (var slot in FixedSlots)
            {
                var module = source.Get(slot);
                if (module != null)
                    slots[slot] = module;
            }
            equipment.AddRange(source.Equipment.Where(item => item != null));
        }

        public string Name { get; set; } = "Custom";

        public IReadOnlyList<EquipmentModule> Equipment => equipment;

        public HullModule Hull => Get(ModuleSlot.Hull) as HullModule;

        public static bool IsRequired(ModuleSlot slot) => RequiredSlots.Contains(slot);

        public TankModule Get(ModuleSlot slot)
        {
            if (slot == ModuleSlot.Equipment)
                throw new ArgumentException("Equipment holds a list, read Equipment instead.", nameof(slot));
            return slots.TryGetValue(slot, out var module) ? module : null;
        }

        public bool IsFitted(TankModule module)
        {
            if (module == null)
                return false;
            return module is EquipmentModule item ? equipment.Contains(item) : Get(module.Slot) == module;
        }

        public bool CanFit(TankModule module)
        {
            if (module == null)
                return false;
            if (IsFitted(module))
                return true;
            var hull = Hull;
            switch (module)
            {
                case HullMachineGunModule _:
                    return hull == null || hull.HasHullMachineGunMount;
                case EquipmentModule _:
                    return hull == null || equipment.Count < hull.EquipmentSlots;
                default:
                    return true;
            }
        }

        public LoadoutBuilder Fit(TankModule module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            if (module is EquipmentModule item)
            {
                if (!equipment.Contains(item))
                    equipment.Add(item);
                return this;
            }

            slots[module.Slot] = module;
            if (module is HullModule hull)
                DropWhatHullCannotCarry(hull);
            return this;
        }

        public LoadoutBuilder Remove(ModuleSlot slot)
        {
            if (slot == ModuleSlot.Equipment)
                equipment.Clear();
            else
                slots.Remove(slot);
            return this;
        }

        public LoadoutBuilder Remove(TankModule module)
        {
            if (module is EquipmentModule item)
                equipment.Remove(item);
            else if (IsFitted(module))
                slots.Remove(module.Slot);
            return this;
        }

        public IReadOnlyList<string> Validate()
        {
            var loadout = Build();
            try
            {
                return TankStatsCalculator.Validate(loadout);
            }
            finally
            {
                Release(loadout);
            }
        }

        public TankLoadout Build() => TankLoadout.CreateRuntime(Name, slots, equipment);

        public static void Release(TankLoadout loadout)
        {
            if (loadout == null || !loadout.IsRuntime)
                return;
            if (Application.isPlaying)
                Object.Destroy(loadout);
            else
                Object.DestroyImmediate(loadout);
        }

        private void DropWhatHullCannotCarry(HullModule hull)
        {
            if (!hull.HasHullMachineGunMount)
                slots.Remove(ModuleSlot.HullMachineGun);
            if (equipment.Count > hull.EquipmentSlots)
                equipment.RemoveRange(hull.EquipmentSlots, equipment.Count - hull.EquipmentSlots);
        }
    }
}
