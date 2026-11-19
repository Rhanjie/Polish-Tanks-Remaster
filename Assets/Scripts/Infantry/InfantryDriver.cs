using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// <see cref="IUnitDriver"/> over a soldier's <see cref="InfantryMover"/>, so the shared orders and the search work
    /// for infantry. Aiming turns the whole soldier. <see cref="ApplyIdleAim"/> runs at the end of the AI's frame and
    /// faces the watch point when nothing aimed that frame.
    /// </summary>
    public sealed class InfantryDriver : IUnitDriver
    {
        private readonly InfantryMover mover;
        private Vector2? watchPoint;
        private bool aimedThisFrame;

        public InfantryDriver(InfantryMover mover)
        {
            this.mover = mover;
        }

        public Vector2 Position => mover.Position;
        public Vector2 Forward => mover.Forward;
        public bool IsMoving => mover.IsMoving;
        public bool HasArrived => mover.HasArrived;
        public bool PathFailed => mover.PathFailed;
        public bool Running { get; set; }

        public bool MoveTo(Vector2 destination) => mover.MoveTo(destination, Running);

        public void Stop() => mover.Stop();

        public void StepTo(Vector2 point) => mover.StepTo(point);

        public void AimAt(Vector2 point)
        {
            aimedThisFrame = true;
            mover.FaceThisFrame(point);
        }

        public void Watch(Vector2? point) => watchPoint = point;

        public void ApplyIdleAim()
        {
            if (!aimedThisFrame && watchPoint.HasValue && !mover.IsMoving)
                mover.FaceThisFrame(watchPoint.Value);
            aimedThisFrame = false;
        }
    }
}
