using System;

namespace PolishTanks.AI.Orders
{
    /// <summary>Stand guard where the tank is, for good. Put a <see cref="WatchOrder"/> before it to point the turret.</summary>
    [Serializable]
    public class HoldOrder : Order
    {
        public override void Begin(in OrderContext context) => context.Driver.Stop();

        public override OrderStatus Tick(in OrderContext context) => OrderStatus.Running;
    }
}
