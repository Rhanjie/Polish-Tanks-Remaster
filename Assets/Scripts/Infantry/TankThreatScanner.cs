using System.Collections.Generic;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>Finds the working tanks (any side) around a soldier, as <see cref="TankThreat"/>s for the give-way check.</summary>
    public static class TankThreatScanner
    {
        private static readonly List<Collider2D> Colliders = new List<Collider2D>(32);
        private static readonly HashSet<Rigidbody2D> Seen = new HashSet<Rigidbody2D>();

        public static void Scan(Vector2 position, float radius, List<TankThreat> results)
        {
            results.Clear();
            Seen.Clear();
            var filter = new ContactFilter2D { useTriggers = false };
            Physics2D.OverlapCircle(position, radius, filter, Colliders);
            foreach (var collider in Colliders)
            {
                var body = collider.attachedRigidbody;
                if (body == null || !Seen.Add(body) || !body.TryGetComponent<TankController>(out var tank) || tank.IsDestroyed)
                    continue;
                results.Add(new TankThreat(body.position, body.linearVelocity));
            }
        }
    }
}
