using System;
using UnityEngine;

namespace PolishTanks.Bouncing
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BounceCounter : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Contacts closer together than this (seconds) count as one bounce.")]
        private float minInterval = 0.05f;

        [SerializeField, Min(0f), Tooltip("Impacts slower than this (m/s, along the contact normal) are ignored.")]
        private float minImpactSpeed = 0.5f;

        [SerializeField, Min(1)]
        private int historySize = BounceTracker.DefaultHistorySize;

        private BounceTracker _tracker;

        /// <summary>Created lazily, so listeners can read it from their own OnEnable regardless of Awake order.</summary>
        public BounceTracker Tracker => _tracker ??= new BounceTracker(minInterval, minImpactSpeed, historySize);

        public event Action<BounceEvent> Bounced;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Tracker.TryRegister(Time.time, ImpactSpeed(collision), out var bounce))
                Bounced?.Invoke(bounce);
        }

        private static float ImpactSpeed(Collision2D collision)
        {
            if (collision.contactCount == 0)
                return collision.relativeVelocity.magnitude;
            return Mathf.Abs(Vector2.Dot(collision.relativeVelocity, collision.GetContact(0).normal));
        }
    }
}
