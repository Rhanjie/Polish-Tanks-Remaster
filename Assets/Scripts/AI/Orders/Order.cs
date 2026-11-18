using System;

namespace PolishTanks.AI.Orders
{
    /// <summary>
    /// One scripted step a level designer gives a tank outside combat (drive somewhere, wait, patrol). Orders sit in an
    /// <see cref="OrderList"/> as <c>[SerializeReference]</c> entries, so each subclass keeps its own fields in the
    /// inspector. <see cref="Begin"/> runs when the order starts and again when the tank comes back to it after a fight,
    /// so it must pick up where it left off. Subclasses need a parameterless constructor for the inspector's Add menu.
    /// </summary>
    [Serializable]
    public abstract class Order
    {
        public abstract void Begin(in OrderContext context);

        public abstract OrderStatus Tick(in OrderContext context);

        public virtual void End(in OrderContext context)
        {
        }

        public virtual void Reset()
        {
        }
    }
}
