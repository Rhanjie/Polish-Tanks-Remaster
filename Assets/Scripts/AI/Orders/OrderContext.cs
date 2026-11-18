namespace PolishTanks.AI.Orders
{
    /// <summary>What an order gets to work with: the tank's controls and the current time.</summary>
    public readonly struct OrderContext
    {
        public OrderContext(ITankDriver driver, float time)
        {
            Driver = driver;
            Time = time;
        }

        public ITankDriver Driver { get; }
        public float Time { get; }
    }
}
