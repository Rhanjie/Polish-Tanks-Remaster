using System;
using UnityEngine;

namespace PolishTanks.AI.Orders
{
    /// <summary>
    /// Keep the turret on a point (or a marker object) from now on, while driving too, until another watch order
    /// changes it. Clearing it points the turret straight ahead again. Done at once.
    /// </summary>
    [Serializable]
    public class WatchOrder : Order
    {
        [SerializeField] private Vector2 point;
        [Tooltip("Optional scene object to watch instead of the point, read when the order runs.")]
        [SerializeField] private Transform marker;
        [Tooltip("Point the turret straight ahead again instead of watching anything.")]
        [SerializeField] private bool clear;

        public WatchOrder()
        {
        }

        public WatchOrder(Vector2 point)
        {
            this.point = point;
        }

        public Vector2 Point => marker != null ? (Vector2)marker.position : point;

        public override void Begin(in OrderContext context) => context.Driver.Watch(clear ? (Vector2?)null : Point);

        public override OrderStatus Tick(in OrderContext context) => OrderStatus.Done;
    }
}
