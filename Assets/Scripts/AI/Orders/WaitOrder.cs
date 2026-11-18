using System;
using UnityEngine;

namespace PolishTanks.AI.Orders
{
    /// <summary>Stand still for a while. Time spent fighting doesn't count, the wait starts over after combat.</summary>
    [Serializable]
    public class WaitOrder : Order
    {
        [SerializeField, Min(0f)] private float seconds = 3f;

        private float until;

        public WaitOrder()
        {
        }

        public WaitOrder(float seconds)
        {
            this.seconds = seconds;
        }

        public override void Begin(in OrderContext context)
        {
            context.Driver.Stop();
            until = context.Time + seconds;
        }

        public override OrderStatus Tick(in OrderContext context) => context.Time >= until ? OrderStatus.Done : OrderStatus.Running;
    }
}
