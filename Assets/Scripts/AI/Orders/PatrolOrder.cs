using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.AI.Orders
{
    /// <summary>
    /// Drive a route point by point, pausing at each. A looping patrol never ends; one that doesn't loop is done after
    /// the last point. After a fight the tank heads back to the point it was going to. A point no route reaches is skipped.
    /// </summary>
    [Serializable]
    public class PatrolOrder : Order
    {
        [SerializeField] private List<Vector2> points = new List<Vector2>();
        [Tooltip("Seconds to stand at each point before driving on.")]
        [SerializeField, Min(0f)] private float pause = 2f;
        [SerializeField] private bool loop = true;

        private int next;
        private float waitUntil = float.NegativeInfinity;
        private bool waiting;

        public PatrolOrder()
        {
        }

        public PatrolOrder(IEnumerable<Vector2> points, float pause, bool loop)
        {
            this.points = new List<Vector2>(points);
            this.pause = pause;
            this.loop = loop;
        }

        public IReadOnlyList<Vector2> Points => points;

        public int NextPoint => next;

        public override void Begin(in OrderContext context)
        {
            waiting = false;
            if (points.Count == 0)
                return;
            if (next >= points.Count)
                next = loop ? 0 : points.Count - 1;
            context.Driver.MoveTo(points[next]);
        }

        public override OrderStatus Tick(in OrderContext context)
        {
            if (points.Count == 0)
                return OrderStatus.Done;

            var driver = context.Driver;
            if (waiting)
            {
                if (context.Time < waitUntil)
                    return OrderStatus.Running;
                waiting = false;
                if (next >= points.Count)
                {
                    if (!loop)
                        return OrderStatus.Done;
                    next = 0;
                }
                driver.MoveTo(points[next]);
                return OrderStatus.Running;
            }

            if (driver.IsMoving)
                return OrderStatus.Running;

            next++;
            waiting = true;
            waitUntil = driver.HasArrived ? context.Time + pause : context.Time;
            return OrderStatus.Running;
        }

        public override void End(in OrderContext context) => context.Driver.Stop();

        public override void Reset()
        {
            next = 0;
            waiting = false;
        }
    }
}
