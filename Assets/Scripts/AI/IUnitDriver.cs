using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// The controls every AI unit moves and looks with, tank or soldier: path finding to a point, stopping, and where the
    /// turret or the soldier faces. Orders and the shared behaviours only talk to this, so they work for both and run
    /// against a fake in tests. <see cref="AimAt"/> holds for the frame it is called in; on other frames the unit looks
    /// at its watch point, or straight ahead.
    /// </summary>
    public interface IUnitDriver
    {
        Vector2 Position { get; }
        Vector2 Forward { get; }
        bool IsMoving { get; }
        bool HasArrived { get; }
        bool PathFailed { get; }

        bool MoveTo(Vector2 destination);
        void Stop();
        void AimAt(Vector2 point);
        void Watch(Vector2? point);
    }
}
