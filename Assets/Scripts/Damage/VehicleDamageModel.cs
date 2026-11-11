using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// Condition of every damageable part on one vehicle and what it adds up to: engine power, track and turret
    /// effectiveness and how much the crew can still see. Intact counts as 1, destroyed as 0 and field-repaired
    /// as <see cref="FieldRepairEffectiveness"/>. Visibility is the mean of the vision blocks, so with four blocks
    /// each loss costs a quarter and each field repair gives an eighth back.
    /// </summary>
    public sealed class VehicleDamageModel
    {
        public const float FieldRepairEffectiveness = 0.5f;

        private readonly List<PartId> parts = new List<PartId>();
        private readonly Dictionary<PartId, PartCondition> conditions = new Dictionary<PartId, PartCondition>();
        private readonly List<PartId> candidates = new List<PartId>();

        public VehicleDamageModel(in PartDamageChances layout)
        {
            Add(new PartId(VehiclePart.Engine));
            if (layout.HasTracks)
            {
                Add(new PartId(VehiclePart.LeftTrack));
                Add(new PartId(VehiclePart.RightTrack));
            }
            if (layout.HasTurretRing)
                Add(new PartId(VehiclePart.TurretRing));
            for (var i = 0; i < layout.VisionBlocks; i++)
                Add(new PartId(VehiclePart.VisionBlock, i));
            VisionBlocks = Mathf.Max(0, layout.VisionBlocks);
        }

        public event Action<PartId, PartCondition> Changed;

        public IReadOnlyList<PartId> Parts => parts;
        public int VisionBlocks { get; }

        public bool HasDamage
        {
            get
            {
                foreach (var condition in conditions.Values)
                    if (condition != PartCondition.Intact)
                        return true;
                return false;
            }
        }

        public float EnginePower => Effectiveness(new PartId(VehiclePart.Engine));
        public float LeftTrack => Effectiveness(new PartId(VehiclePart.LeftTrack));
        public float RightTrack => Effectiveness(new PartId(VehiclePart.RightTrack));
        public float TurretTraverse => Effectiveness(new PartId(VehiclePart.TurretRing));

        public float Visibility
        {
            get
            {
                if (VisionBlocks == 0)
                    return 1f;
                var sum = 0f;
                for (var i = 0; i < VisionBlocks; i++)
                    sum += Effectiveness(new PartId(VehiclePart.VisionBlock, i));
                return sum / VisionBlocks;
            }
        }

        public bool Has(PartId id) => conditions.ContainsKey(id);

        public PartCondition ConditionOf(PartId id) =>
            conditions.TryGetValue(id, out var condition) ? condition : PartCondition.Intact;

        public float Effectiveness(PartId id) => Effectiveness(ConditionOf(id));

        public static float Effectiveness(PartCondition condition) => condition switch
        {
            PartCondition.Destroyed => 0f,
            PartCondition.FieldRepaired => FieldRepairEffectiveness,
            _ => 1f,
        };

        public PartId? Break(VehiclePart part, Func<float> random)
        {
            candidates.Clear();
            foreach (var id in parts)
                if (id.Part == part && conditions[id] != PartCondition.Destroyed)
                    candidates.Add(id);
            if (candidates.Count == 0)
                return null;

            var pick = Mathf.Min(candidates.Count - 1, (int)(random() * candidates.Count));
            var target = candidates[pick];
            Set(target, PartCondition.Destroyed);
            return target;
        }

        public bool FieldRepair(PartId id)
        {
            if (ConditionOf(id) != PartCondition.Destroyed || !Has(id))
                return false;
            Set(id, PartCondition.FieldRepaired);
            return true;
        }

        public void CopyConditionsFrom(VehicleDamageModel other)
        {
            foreach (var id in parts)
                if (other.Has(id))
                    conditions[id] = other.ConditionOf(id);
        }

        private void Add(PartId id)
        {
            parts.Add(id);
            conditions[id] = PartCondition.Intact;
        }

        private void Set(PartId id, PartCondition condition)
        {
            conditions[id] = condition;
            Changed?.Invoke(id, condition);
        }
    }
}
