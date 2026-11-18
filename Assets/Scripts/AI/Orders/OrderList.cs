using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.AI.Orders
{
    /// <summary>
    /// A tank's scripted orders, run one after another. When the last one is done the list starts over if it loops,
    /// otherwise the tank stands where it ended up; an order added after that starts on the next tick.
    /// <see cref="Suspend"/> hands the tank over to combat and <see cref="Resume"/> restarts the current order where it
    /// left off. <see cref="Prepare"/> begins and suspends the first order once, so orders that remember where they
    /// started (a guard post) start from where the tank was placed even when a fight comes first. Code can replace the
    /// whole list with <see cref="Set"/> (a cutscene giving new orders) or append with <see cref="Add"/>.
    /// </summary>
    [Serializable]
    public class OrderList
    {
        [SerializeReference] private List<Order> orders = new List<Order>();
        [Tooltip("Start again from the first order after the last one is done.")]
        [SerializeField] private bool loop;

        private int index;
        private bool started;
        private bool currentBegun;

        public IReadOnlyList<Order> Orders => orders;
        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        public int Index => index;
        public bool IsFinished => index >= orders.Count;
        public Order Current => started && !IsFinished ? orders[index] : null;

        public void Set(IEnumerable<Order> newOrders, bool loopOrders, in OrderContext context)
        {
            EndCurrent(context);
            orders = new List<Order>(newOrders);
            loop = loopOrders;
            Restart();
        }

        public void Add(Order order) => orders.Add(order);

        public void Restart()
        {
            index = 0;
            started = false;
            currentBegun = false;
            foreach (var order in orders)
                order?.Reset();
        }

        public void Prepare(in OrderContext context)
        {
            Resume(context);
            Suspend(context);
        }

        public void Resume(in OrderContext context)
        {
            started = true;
            currentBegun = false;
            SkipEmpty();
            if (IsFinished)
                context.Driver.Stop();
            else
                BeginCurrent(context);
        }

        public void Suspend(in OrderContext context)
        {
            EndCurrent(context);
            started = false;
        }

        public void Tick(in OrderContext context)
        {
            if (!started)
                Resume(context);

            var guard = orders.Count + 1;
            while (guard-- > 0)
            {
                SkipEmpty();
                if (IsFinished)
                    return;
                if (!currentBegun)
                    BeginCurrent(context);
                if (orders[index].Tick(context) == OrderStatus.Running)
                    return;
                EndCurrent(context);
                Advance();
                if (IsFinished)
                {
                    context.Driver.Stop();
                    return;
                }
            }
        }

        private void BeginCurrent(in OrderContext context)
        {
            orders[index].Begin(context);
            currentBegun = true;
        }

        private void EndCurrent(in OrderContext context)
        {
            if (currentBegun && !IsFinished && orders[index] != null)
                orders[index].End(context);
            currentBegun = false;
        }

        private void Advance()
        {
            index++;
            SkipEmpty();
            if (IsFinished && loop && orders.Count > 0)
            {
                foreach (var order in orders)
                    order?.Reset();
                index = 0;
                SkipEmpty();
            }
        }

        private void SkipEmpty()
        {
            while (index < orders.Count && orders[index] == null)
                index++;
        }
    }
}
