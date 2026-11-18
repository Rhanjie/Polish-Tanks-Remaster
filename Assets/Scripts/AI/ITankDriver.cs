using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// The controls an AI drives a tank with: hull movement through path finding, the turret and the guns.
    /// Orders and behaviours only talk to this, so they run against a fake in tests.
    /// The turret follows <see cref="AimAt"/> for the frame it is called in and falls back to the watch point
    /// (or straight ahead) on frames nobody aims it.
    /// </summary>
    public interface ITankDriver
    {
        Vector2 Position { get; }
        Vector2 Forward { get; }
        bool IsMoving { get; }
        bool HasArrived { get; }
        bool PathFailed { get; }
        float AimError { get; }

        bool MoveTo(Vector2 destination);
        void Stop();
        void AimAt(Vector2 point);
        void Watch(Vector2? point);
        bool FireMainGun();
        void SetMachineGuns(bool held, Vector2 target);
    }
}
