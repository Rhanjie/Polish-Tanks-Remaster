using System;
using UnityEngine;

namespace PolishTanks.AI.Orders
{
    /// <summary>Drive to a point, or to a marker object when one is set. Done on arrival, or when no route gets there.</summary>
    [Serializable]
    public class MoveToOrder : Order
    {
        [Tooltip("World position to drive to. Ignored when Marker is set.")]
        [SerializeField] private Vector2 destination;
        [Tooltip("Optional scene object to drive to instead of the position, read when the order starts.")]
        [SerializeField] private Transform marker;

        public MoveToOrder()
        {
        }

        public MoveToOrder(Vector2 destination)
        {
            this.destination = destination;
        }

        public Vector2 Destination => marker != null ? (Vector2)marker.position : destination;

        public override void Begin(in OrderContext context) => context.Driver.MoveTo(Destination);

        public override OrderStatus Tick(in OrderContext context)
        {
            var driver = context.Driver;
            return driver.HasArrived || driver.PathFailed || !driver.IsMoving ? OrderStatus.Done : OrderStatus.Running;
        }

        public override void End(in OrderContext context) => context.Driver.Stop();
    }
}
