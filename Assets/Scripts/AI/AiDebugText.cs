using PolishTanks.AI.Orders;
using PolishTanks.Combat;

namespace PolishTanks.AI
{
    /// <summary>
    /// Every string the F3 AI view shows on screen, in one place for a later localization module.
    /// Values go in through format placeholders, never by gluing sentences together.
    /// </summary>
    public static class AiDebugText
    {
        public const string Header = "AI (F3)";
        public const string BehaviourFormat = "{0}: {1}";
        public const string TargetFormat = "Player: {0}";
        public const string TargetAgeFormat = "Player: {0} ({1:0.0} s ago)";
        public const string PenetrationFormat = "Pen. front {0}/{1}, side {2}/{3}, rear {4}/{5}";
        public const string ScriptedBehaviourFormat = "{0}: {1} (scripted combat)";
        public const string NoOrders = "no orders";
        public const string OrdersDone = "orders done";
        public const string Idle = "idle";

        public static string BehaviourName(AiBehaviourKind? kind) => kind switch
        {
            AiBehaviourKind.FollowOrders => "orders",
            AiBehaviourKind.Engage => "engage",
            AiBehaviourKind.Search => "search",
            AiBehaviourKind.GiveWay => "give way",
            _ => Idle,
        };

        public static string AwarenessName(TargetAwareness awareness) => awareness switch
        {
            TargetAwareness.Visible => "in sight",
            TargetAwareness.Remembered => "remembered",
            _ => "unknown",
        };

        public static string OrderName(Order order) => order switch
        {
            MoveToOrder => "move to",
            WaitOrder => "wait",
            PatrolOrder => "patrol",
            HoldOrder => "hold",
            WatchOrder => "watch",
            null => NoOrders,
            _ => order.GetType().Name,
        };

        public static string SideCounts(PenetrationLog log) => string.Format(PenetrationFormat,
            log.Penetrations(HitSide.Front), log.Hits(HitSide.Front),
            log.Penetrations(HitSide.Side), log.Hits(HitSide.Side),
            log.Penetrations(HitSide.Rear), log.Hits(HitSide.Rear));
    }
}
