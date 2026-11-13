using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// Condition of every damageable part on one vehicle and what it adds up to: engine power, track and turret
    /// effectiveness and how much the crew can still see. Each hit moves a part one step from intact to damaged to
    /// destroyed. Intact counts as 1, destroyed as 0 and damaged as <see cref="DamagedEffectiveness"/>, which differs
    /// per part. Visibility is the mean of the vision blocks. A hit on the vision blocks lands on any one of them,
    /// so it can strike a block that is already gone and change nothing.
    /// </summary>
    public sealed class VehicleDamageModel
    {
        public const float DamagedEngine = 0.6f;
        public const float DamagedTrack = 0.8f;
        public const float DamagedTurretRing = 0.5f;
        public const float DamagedVisionBlock = 0.5f;

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

        public float Effectiveness(PartId id) => Effectiveness(id.Part, ConditionOf(id));

        public static float Effectiveness(VehiclePart part, PartCondition condition) => condition switch
        {
            PartCondition.Destroyed => 0f,
            PartCondition.Damaged => DamagedEffectiveness(part),
            _ => 1f,
        };

        public static float DamagedEffectiveness(VehiclePart part) => part switch
        {
            VehiclePart.Engine => DamagedEngine,
            VehiclePart.LeftTrack => DamagedTrack,
            VehiclePart.RightTrack => DamagedTrack,
            VehiclePart.TurretRing => DamagedTurretRing,
            _ => DamagedVisionBlock,
        };

        public PartId? Break(VehiclePart part, Func<float> random)
        {
            candidates.Clear();
            foreach (var id in parts)
                if (id.Part == part)
                    candidates.Add(id);
            if (candidates.Count == 0)
                return null;

            var pick = Mathf.Min(candidates.Count - 1, (int)(random() * candidates.Count));
            var target = candidates[pick];
            var condition = conditions[target];
            if (condition == PartCondition.Destroyed)
                return null;

            Set(target, condition == PartCondition.Intact ? PartCondition.Damaged : PartCondition.Destroyed);
            return target;
        }

        public bool FieldRepair(PartId id)
        {
            if (ConditionOf(id) != PartCondition.Destroyed || !Has(id))
                return false;
            Set(id, PartCondition.Damaged);
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
