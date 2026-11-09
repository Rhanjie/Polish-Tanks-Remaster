using System;
using System.Collections.Generic;
using PolishTanks.Tank.Modules;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>
    /// Builds in-memory modules and loadouts for tests. Module fields are private and serialized,
    /// so values are set through SerializedObject, the same path the Inspector uses.
    /// </summary>
    internal sealed class ModuleFactory : IDisposable
    {
        private readonly List<Object> created = new List<Object>();

        public T Create<T>(params (string field, object value)[] values) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = typeof(T).Name;
            created.Add(asset);
            Set(asset, values);
            return asset;
        }

        public static void Set(Object target, params (string field, object value)[] values)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in values)
            {
                var prop = so.FindProperty(field) ?? throw new ArgumentException($"No serialized field '{field}' on {target.GetType().Name}");
                switch (value)
                {
                    case float f: prop.floatValue = f; break;
                    case int i when prop.propertyType == SerializedPropertyType.Float: prop.floatValue = i; break;
                    case int i: prop.intValue = i; break;
                    case string s: prop.stringValue = s; break;
                    case Vector2 v: prop.vector2Value = v; break;
                    case Object o: prop.objectReferenceValue = o; break;
                    case Object[] list:
                        prop.arraySize = list.Length;
                        for (var n = 0; n < list.Length; n++)
                            prop.GetArrayElementAtIndex(n).objectReferenceValue = list[n];
                        break;
                    case null: prop.objectReferenceValue = null; break;
                    default: throw new ArgumentException($"Unsupported value type {value.GetType()} for '{field}'");
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>A valid medium tank: 20 t, 300 hp, no penalties, no optional parts.</summary>
        public TankLoadout CreateBaseline()
        {
            var loadout = Create<TankLoadout>();
            Set(loadout,
                ("hull", Create<HullModule>(("mass", 14000f), ("equipmentSlots", 2))),
                ("engine", Create<EngineModule>(("mass", 1000f), ("horsePower", 300f))),
                ("transmission", Create<TransmissionModule>(("mass", 500f), ("efficiency", 1f), ("accelerationMultiplier", 1f), ("reverseSpeedRatio", 0.4f), ("turnRateMultiplier", 1f))),
                ("tracks", Create<TracksModule>(("mass", 1500f), ("grip", 1f), ("maxSpeedMultiplier", 1f), ("turnRate", 70f), ("maxLoad", 40000f))),
                ("turret", Create<TurretModule>(("mass", 2000f), ("maxTraverseSpeed", 90f), ("maxGunMass", 2500f))),
                ("mainGun", Create<MainGunModule>(("mass", 1000f), ("reloadTime", 2f), ("dispersion", 0.5f))));
            return loadout;
        }

        public void Dispose()
        {
            foreach (var obj in created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            created.Clear();
        }
    }
}
