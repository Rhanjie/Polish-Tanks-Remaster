using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>
    /// Every module and loadout in the project, so runtime code (the debug swap panel, playtest scripts) can list and
    /// find them without the AssetDatabase. The editor refills it whenever a module or loadout asset changes;
    /// nothing needs to be added by hand.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Module Catalog", fileName = "ModuleCatalog")]
    public class ModuleCatalog : ScriptableObject
    {
        [SerializeField] private List<TankModule> modules = new List<TankModule>();
        [SerializeField] private List<TankLoadout> loadouts = new List<TankLoadout>();

        public IReadOnlyList<TankModule> Modules => modules;
        public IReadOnlyList<TankLoadout> Loadouts => loadouts;

        public IReadOnlyList<TankModule> ForSlot(ModuleSlot slot)
        {
            return modules.Where(module => module != null && module.Slot == slot).ToList();
        }

        public TankModule Find(string nameOrDisplayName)
        {
            return modules.FirstOrDefault(module => module != null && Matches(module.name, module.DisplayName, nameOrDisplayName));
        }

        public T Find<T>(string nameOrDisplayName) where T : TankModule => Find(nameOrDisplayName) as T;

        public TankLoadout FindLoadout(string nameOrDisplayName)
        {
            return loadouts.FirstOrDefault(loadout => loadout != null && Matches(loadout.name, loadout.DisplayName, nameOrDisplayName));
        }

        public void SetContents(IEnumerable<TankModule> newModules, IEnumerable<TankLoadout> newLoadouts)
        {
            modules = newModules.Where(module => module != null).Distinct()
                .OrderBy(module => module.Slot).ThenBy(module => module.Tier).ThenBy(module => module.DisplayName)
                .ToList();
            loadouts = newLoadouts.Where(loadout => loadout != null).Distinct()
                .OrderBy(loadout => loadout.DisplayName)
                .ToList();
        }

        private static bool Matches(string assetName, string displayName, string query)
        {
            return string.Equals(assetName, query, System.StringComparison.OrdinalIgnoreCase)
                   || string.Equals(displayName, query, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
