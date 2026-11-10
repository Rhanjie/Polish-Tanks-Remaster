using System;
using System.Collections.Generic;
using PolishTanks.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Builds prefabs, definitions and colliders in code and destroys all of it, including the [Pools] root, on Dispose.</summary>
    internal sealed class WeaponFixture : IDisposable
    {
        private readonly List<Object> created = new List<Object>();

        public T Track<T>(T obj) where T : Object
        {
            created.Add(obj);
            return obj;
        }

        public T CreatePrefab<T>(string name = "Prefab") where T : Component
        {
            var go = Track(new GameObject(name));
            go.SetActive(false);
            return go.AddComponent<T>();
        }

        public Projectile CreateProjectilePrefab() => CreatePrefab<Projectile>("ProjectilePrefab");

        public ProjectileDefinition CreateDefinition(Projectile prefab, float lifetime = 5f, int prewarm = 0)
        {
            var definition = Track(ScriptableObject.CreateInstance<ProjectileDefinition>());
            TankRig.SetField(definition, "prefab", prefab);
            TankRig.SetField(definition, "lifetime", lifetime);
            TankRig.SetField(definition, "prewarmCount", prewarm);
            return definition;
        }

        public BoxCollider2D CreateStaticBox(Vector2 position, Vector2 size, bool trigger = false)
        {
            var go = Track(new GameObject("StaticBox"));
            go.transform.position = position;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.isTrigger = trigger;
            return box;
        }

        public Rigidbody2D CreateDynamicBox(Vector2 position, Vector2 size)
        {
            var go = Track(new GameObject("DynamicBox"));
            go.transform.position = position;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            go.AddComponent<BoxCollider2D>().size = size;
            return body;
        }

        public void Dispose()
        {
            foreach (var obj in created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            created.Clear();

            var pools = GameObject.Find("[Pools]");
            if (pools != null)
                Object.DestroyImmediate(pools);
        }
    }
}
