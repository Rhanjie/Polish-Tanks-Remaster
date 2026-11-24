using PolishTanks.AI.Orders;

namespace PolishTanks.AI
{
    /// <summary>
    /// Everything a behaviour reads and drives in one tick: the unit's controls, its orders, what it knows about the
    /// target, its doctrine, the ground it can search and the clock. Owned by one AI and refreshed every frame.
    /// </summary>
    public sealed class AiContext
    {
        public AiContext(IUnitDriver driver, OrderList orders, TargetMemory target, CombatDoctrine doctrine)
        {
            Driver = driver;
            Orders = orders;
            Target = target;
            Doctrine = doctrine;
        }

        public IUnitDriver Driver { get; }
        public OrderList Orders { get; set; }
        public TargetMemory Target { get; }
        public CombatDoctrine Doctrine { get; set; }
        public ISearchSpace SearchSpace { get; set; }
        public bool ScriptedCombat { get; set; }
        public float Time { get; set; }
        public float DeltaTime { get; set; }

        public bool IsTargetSpotted => Target.IsSpotted(Time, Doctrine.ReactionTime);
    }
}
