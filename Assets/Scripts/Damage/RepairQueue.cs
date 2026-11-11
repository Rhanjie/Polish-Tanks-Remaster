namespace PolishTanks.Damage
{
    /// <summary>
    /// Order a crew patches parts in, most urgent first: the engine (it also puts out a fire), a vision block when
    /// the crew is completely blind, the tracks, the turret ring and then the remaining vision blocks.
    /// Only destroyed parts are picked; a field-repaired part waits for the workshop.
    /// </summary>
    public static class RepairQueue
    {
        private static readonly VehiclePart[] MobilityAndTurret = { VehiclePart.LeftTrack, VehiclePart.RightTrack, VehiclePart.TurretRing };

        public static PartId? Next(VehicleDamageModel model)
        {
            if (model == null)
                return null;

            var engine = new PartId(VehiclePart.Engine);
            if (IsDestroyed(model, engine))
                return engine;

            if (model.VisionBlocks > 0 && model.Visibility <= 0f)
                return FirstDestroyedBlock(model);

            foreach (var part in MobilityAndTurret)
            {
                var id = new PartId(part);
                if (IsDestroyed(model, id))
                    return id;
            }

            return FirstDestroyedBlock(model);
        }

        public static int Urgency(VehicleDamageModel model, PartId id)
        {
            if (id.Part == VehiclePart.Engine)
                return 0;
            if (id.Part == VehiclePart.VisionBlock && model.Visibility <= 0f)
                return 1;
            return 2;
        }

        private static PartId? FirstDestroyedBlock(VehicleDamageModel model)
        {
            for (var i = 0; i < model.VisionBlocks; i++)
            {
                var id = new PartId(VehiclePart.VisionBlock, i);
                if (IsDestroyed(model, id))
                    return id;
            }
            return null;
        }

        private static bool IsDestroyed(VehicleDamageModel model, PartId id) =>
            model.Has(id) && model.ConditionOf(id) == PartCondition.Destroyed;
    }
}
