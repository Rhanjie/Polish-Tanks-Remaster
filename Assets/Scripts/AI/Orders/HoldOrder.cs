using System;
using UnityEngine;

namespace PolishTanks.AI.Orders
{
    /// <summary>
    /// Stand guard for good at the spot where the order first started. After a fight or a search took the tank away it
    /// drives back to that post. Put a <see cref="WatchOrder"/> before it to point the turret.
    /// </summary>
    [Serializable]
    public class HoldOrder : Order
    {
        public const float PostTolerance = 2f;

        [Tooltip("Drive back to the post after combat. Off holds wherever the tank ends up.")]
        [SerializeField] private bool returnToPost = true;

        private Vector2? post;

        public Vector2? Post => post;

        public override void Begin(in OrderContext context)
        {
            var driver = context.Driver;
            post ??= driver.Position;
            if (returnToPost && Vector2.Distance(driver.Position, post.Value) > PostTolerance)
                driver.MoveTo(post.Value);
            else
                driver.Stop();
        }

        public override OrderStatus Tick(in OrderContext context) => OrderStatus.Running;

        public override void End(in OrderContext context) => context.Driver.Stop();

        public override void Reset() => post = null;
    }
}
