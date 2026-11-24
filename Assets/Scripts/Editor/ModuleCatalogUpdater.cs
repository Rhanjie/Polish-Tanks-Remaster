using System.Linq;
using PolishTanks.Tank.Modules;
using UnityEditor;

namespace PolishTanks.Editor
{
    /// <summary>
    /// Keeps every <see cref="ModuleCatalog"/> in sync with the module and loadout assets in the project. Runs after any
    /// .asset import, move or delete, and saves a catalog only when its contents actually changed.
    /// </summary>
    public class ModuleCatalogUpdater : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (imported.Concat(deleted).Concat(moved).Any(path => path.EndsWith(".asset")))
                RefreshAll();
        }

        [MenuItem("Tools/Polish Tanks/Refresh Module Catalog")]
        public static void RefreshAll()
        {
            var modules = Load<TankModule>();
            var loadouts = Load<TankLoadout>();
            foreach (var catalog in Load<ModuleCatalog>())
            {
                var before = catalog.Modules.ToList();
                var loadoutsBefore = catalog.Loadouts.ToList();
                catalog.SetContents(modules, loadouts);
                if (before.SequenceEqual(catalog.Modules) && loadoutsBefore.SequenceEqual(catalog.Loadouts))
                    continue;
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssetIfDirty(catalog);
            }
        }

        private static T[] Load<T>() where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToArray();
        }
    }
}
