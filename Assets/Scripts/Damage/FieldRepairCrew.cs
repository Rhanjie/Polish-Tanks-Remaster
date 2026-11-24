using System;
using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// A crew working through <see cref="RepairQueue"/> one part at a time. Each part takes its own time; when it is
    /// done the crew hands it back for patching and moves on to the next, stopping once nothing destroyed is left.
    /// If something more urgent breaks meanwhile (the engine, or the last vision block), the crew drops the current
    /// part and starts over on that one.
    /// </summary>
    public sealed class FieldRepairCrew
    {
        private readonly Func<VehiclePart, float> secondsFor;

        public FieldRepairCrew(Func<VehiclePart, float> secondsFor)
        {
            this.secondsFor = secondsFor;
        }

        public bool IsWorking => Current.HasValue;
        public PartId? Current { get; private set; }
        public float Elapsed { get; private set; }
        public float Duration { get; private set; }
        public float Progress => Duration > 0f ? Mathf.Clamp01(Elapsed / Duration) : 1f;

        public bool Start(VehicleDamageModel model)
        {
            Pick(model);
            return IsWorking;
        }

        public void Stop()
        {
            Current = null;
            Elapsed = 0f;
            Duration = 0f;
        }

        public void Tick(VehicleDamageModel model, float deltaTime, Action<PartId> patch)
        {
            if (!Current.HasValue)
                return;

            if (model.ConditionOf(Current.Value) != PartCondition.Destroyed || SomethingMoreUrgent(model))
            {
                Pick(model);
                return;
            }

            Elapsed += Mathf.Max(0f, deltaTime);
            if (Elapsed < Duration)
                return;

            patch(Current.Value);
            Pick(model);
        }

        private bool SomethingMoreUrgent(VehicleDamageModel model)
        {
            var next = RepairQueue.Next(model);
            return next.HasValue && next.Value != Current.Value
                && RepairQueue.Urgency(model, next.Value) < RepairQueue.Urgency(model, Current.Value);
        }

        private void Pick(VehicleDamageModel model)
        {
            Elapsed = 0f;
            Current = RepairQueue.Next(model);
            Duration = Current.HasValue ? Mathf.Max(0f, secondsFor(Current.Value.Part)) : 0f;
        }
    }
}
