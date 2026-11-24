namespace PolishTanks.AI.Orders
{
    /// <summary>What an order gets to work with: the unit's controls and the current time.</summary>
    public readonly struct OrderContext
    {
        public OrderContext(IUnitDriver driver, float time)
        {
            Driver = driver;
            Time = time;
        }

        public IUnitDriver Driver { get; }
        public float Time { get; }
    }
}
