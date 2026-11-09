using System.Collections.Generic;
using PolishTanks.Tank.Modules;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Test double: records when Modify runs and scales the hit points so call order matters.</summary>
    internal sealed class RecordingEquipmentModule : EquipmentModule
    {
        public List<string> Log;
        public string Label;
        public float HitPointsMultiplier = 1f;
        public float HitPointsBonus;

        public override void Modify(TankStats stats)
        {
            Log?.Add(Label);
            stats.HitPoints = stats.HitPoints * HitPointsMultiplier + HitPointsBonus;
        }
    }
}
