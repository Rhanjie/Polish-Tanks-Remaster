using System;
using UnityEngine;

namespace PolishTanks.Combat
{
    /// <summary>Hit points that only go down, and a single death when they reach zero.</summary>
    public sealed class HealthPool
    {
        public HealthPool(float max)
        {
            Reset(max);
        }

        public event Action Died;

        public float Max { get; private set; }
        public float Current { get; private set; }
        public bool IsDead => Current <= 0f;
        public float Fraction => Max > 0f ? Current / Max : 0f;

        public void Reset(float max)
        {
            Max = Mathf.Max(1f, max);
            Current = Max;
        }

        public void SetMax(float max)
        {
            var fraction = Fraction;
            Max = Mathf.Max(1f, max);
            if (!IsDead)
                Current = Mathf.Max(1f, Max * fraction);
        }

        public float TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
                return 0f;

            var dealt = Mathf.Min(amount, Current);
            Current -= dealt;
            if (IsDead)
                Died?.Invoke();
            return dealt;
        }
    }
}
