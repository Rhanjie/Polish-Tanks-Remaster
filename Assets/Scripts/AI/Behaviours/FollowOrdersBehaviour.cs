using PolishTanks.AI.Orders;

namespace PolishTanks.AI.Behaviours
{
    /// <summary>
    /// The fallback when nothing else wants the tank: run the level designer's orders. Leaving it suspends the current
    /// order and coming back restarts it from where it was.
    /// </summary>
    public sealed class FollowOrdersBehaviour : AiBehaviour
    {
        public const float BaseScore = 0.1f;

        public override AiBehaviourKind Kind => AiBehaviourKind.FollowOrders;

        public override float Score(AiContext context) => BaseScore;

        public override void Enter(AiContext context) => context.Orders.Resume(OrderContextOf(context));

        public override void Tick(AiContext context) => context.Orders.Tick(OrderContextOf(context));

        public override void Exit(AiContext context) => context.Orders.Suspend(OrderContextOf(context));

        private static OrderContext OrderContextOf(AiContext context) => new OrderContext(context.Driver, context.Time);
    }
}
