using System.Collections.Generic;
using NUnit.Framework;
using PolishTanks.Damage;

namespace PolishTanks.Tests.EditMode
{
    /// <summary><see cref="FieldRepairCrew"/> working through destroyed parts with an engine taking 2 s and anything else 1 s.</summary>
    public class FieldRepairCrewTests
    {
        private static readonly PartId Engine = new PartId(VehiclePart.Engine);
        private static readonly PartId LeftTrack = new PartId(VehiclePart.LeftTrack);

        private VehicleDamageModel model;
        private FieldRepairCrew crew;
        private List<PartId> patched;

        private static float First() => 0f;

        [SetUp]
        public void SetUp()
        {
            model = new VehicleDamageModel(new PartDamageChances { HasTracks = true, HasTurretRing = true, VisionBlocks = 2 });
            crew = new FieldRepairCrew(part => part == VehiclePart.Engine ? 2f : 1f);
            patched = new List<PartId>();
        }

        private void Patch(PartId id)
        {
            patched.Add(id);
            model.FieldRepair(id);
        }

        [Test]
        public void Start_NothingDestroyed_DoesNotStartWorking()
        {
            Assert.That(crew.Start(model), Is.False);
        }

        [Test]
        public void Tick_BeforePartDurationElapsed_DoesNotPatch()
        {
            model.Break(VehiclePart.Engine, First);
            crew.Start(model);

            crew.Tick(model, 1.9f, Patch);

            Assert.That(patched, Is.Empty);
        }

        [Test]
        public void Tick_PartDurationElapsed_PatchesThatPart()
        {
            model.Break(VehiclePart.Engine, First);
            crew.Start(model);

            crew.Tick(model, 2f, Patch);

            Assert.That(patched, Is.EqualTo(new[] { Engine }));
        }

        [Test]
        public void Tick_PartPatchedWithMoreDestroyed_MovesOnToNextPart()
        {
            model.Break(VehiclePart.Engine, First);
            model.Break(VehiclePart.LeftTrack, First);
            crew.Start(model);

            crew.Tick(model, 2f, Patch);

            Assert.That(crew.Current, Is.EqualTo(LeftTrack));
        }

        [Test]
        public void Tick_LastPartPatched_CrewStopsWorking()
        {
            model.Break(VehiclePart.Engine, First);
            crew.Start(model);

            crew.Tick(model, 2f, Patch);

            Assert.That(crew.IsWorking, Is.False);
        }

        [Test]
        public void Tick_CurrentPartRepairedElsewhere_PicksNextWithoutPatching()
        {
            model.Break(VehiclePart.Engine, First);
            model.Break(VehiclePart.LeftTrack, First);
            crew.Start(model);
            model.FieldRepair(Engine);

            crew.Tick(model, 5f, Patch);

            Assert.That(patched, Is.Empty);
        }

        [Test]
        public void Tick_EngineBreaksWhileTrackIsBeingPatched_SwitchesToEngine()
        {
            model.Break(VehiclePart.LeftTrack, First);
            crew.Start(model);
            crew.Tick(model, 0.5f, Patch);

            model.Break(VehiclePart.Engine, First);
            crew.Tick(model, 0.1f, Patch);

            Assert.That(crew.Current, Is.EqualTo(Engine));
        }
    }
}
