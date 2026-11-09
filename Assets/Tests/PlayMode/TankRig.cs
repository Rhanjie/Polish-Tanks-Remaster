using System;
using System.Collections.Generic;
using System.Reflection;
using PolishTanks.Tank;
using PolishTanks.Tank.Modules;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>
    /// Builds modules, loadouts and a fully wired tank in code. PlayMode tests can't use SerializedObject,
    /// so private serialized fields are set through reflection. Dispose destroys everything created.
    /// </summary>
    internal sealed class TankRig : IDisposable
    {
        private const BindingFlags Fields = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        private readonly List<Object> created = new List<Object>();

        public sealed class BuiltTank
        {
            public GameObject Root;
            public Rigidbody2D Body;
            public BoxCollider2D Collider;
            public TankMovement Movement;
            public TurretAimer Aimer;
            public TankGun Gun;
            public TankController Controller;
            public Transform Turret;
            public Transform Muzzle;
        }

        /// <summary>Sets a field declared anywhere in the type hierarchy, private or not.</summary>
        public static void SetField(object target, string name, object value)
        {
            for (var type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(name, Fields | BindingFlags.DeclaredOnly);
                if (field == null)
                    continue;
                field.SetValue(target, value);
                return;
            }
            throw new ArgumentException($"No field '{name}' on {target.GetType().Name}");
        }

        public T Create<T>(params (string field, object value)[] values) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = typeof(T).Name;
            created.Add(asset);
            foreach (var (field, value) in values)
                SetField(asset, field, value);
            return asset;
        }

        /// <summary>A valid 20 t, 300 hp tank with no optional parts (same numbers as the EditMode baseline).</summary>
        public TankLoadout CreateLoadout(float horsePower = 300f, float muzzleDistance = 4f)
        {
            var loadout = Create<TankLoadout>();
            SetField(loadout, "hull", Create<HullModule>(("mass", 14000f), ("equipmentSlots", 2)));
            SetField(loadout, "engine", Create<EngineModule>(("mass", 1000f), ("horsePower", horsePower)));
            SetField(loadout, "transmission", Create<TransmissionModule>(("mass", 500f), ("efficiency", 1f),
                ("accelerationMultiplier", 1f), ("reverseSpeedRatio", 0.4f), ("turnRateMultiplier", 1f)));
            SetField(loadout, "tracks", Create<TracksModule>(("mass", 1500f), ("grip", 1f), ("maxSpeedMultiplier", 1f),
                ("turnRate", 70f), ("maxLoad", 40000f)));
            SetField(loadout, "turret", Create<TurretModule>(("mass", 2000f), ("maxTraverseSpeed", 90f),
                ("maxGunMass", 2500f), ("muzzleDistance", muzzleDistance)));
            SetField(loadout, "mainGun", Create<MainGunModule>(("mass", 1000f), ("reloadTime", 2f), ("dispersion", 0.5f)));
            return loadout;
        }

        /// <summary>
        /// Builds a tank hierarchy with every part wired into the controller. The tank is created inactive
        /// so the controller's Awake applies <paramref name="loadout"/> (or nothing, if null) on activation.
        /// </summary>
        public BuiltTank BuildTank(string name, TankLoadout loadout, Vector2 position)
        {
            var tank = new BuiltTank();
            tank.Root = new GameObject(name);
            created.Add(tank.Root);
            tank.Root.SetActive(false);
            tank.Root.transform.position = position;

            tank.Body = tank.Root.AddComponent<Rigidbody2D>();
            tank.Collider = tank.Root.AddComponent<BoxCollider2D>();
            tank.Movement = tank.Root.AddComponent<TankMovement>();

            var turretObject = new GameObject("Turret");
            turretObject.transform.SetParent(tank.Root.transform, false);
            tank.Turret = turretObject.transform;
            var muzzleObject = new GameObject("Muzzle");
            muzzleObject.transform.SetParent(tank.Turret, false);
            tank.Muzzle = muzzleObject.transform;

            tank.Aimer = tank.Root.AddComponent<TurretAimer>();
            SetField(tank.Aimer, "turret", tank.Turret);
            tank.Gun = tank.Root.AddComponent<TankGun>();
            SetField(tank.Gun, "muzzle", tank.Muzzle);

            tank.Controller = tank.Root.AddComponent<TankController>();
            SetField(tank.Controller, "loadout", loadout);
            SetField(tank.Controller, "movement", tank.Movement);
            SetField(tank.Controller, "aimer", tank.Aimer);
            SetField(tank.Controller, "gun", tank.Gun);
            SetField(tank.Controller, "body", tank.Body);
            SetField(tank.Controller, "hullCollider", tank.Collider);

            tank.Root.SetActive(true);
            return tank;
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
