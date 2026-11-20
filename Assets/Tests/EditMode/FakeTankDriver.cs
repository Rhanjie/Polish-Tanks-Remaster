using System.Collections.Generic;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>An <see cref="ITankDriver"/> that records what the AI asked for and lets a test set what the tank reports back.</summary>
    internal sealed class FakeTankDriver : ITankDriver
    {
        public readonly List<Vector2> MoveToCalls = new List<Vector2>();
        public readonly List<Vector2> AimCalls = new List<Vector2>();
        public int StopCount;
        public int MainGunShots;
        public bool CanFire = true;
        public bool MachineGunsHeld;

        public Vector2 Position { get; set; }
        public Vector2 Forward { get; set; } = Vector2.up;
        public bool IsMoving { get; set; }
        public bool HasArrived { get; set; }
        public bool PathFailed { get; set; }
        public float AimError { get; set; }
        public Vector2? HullHeading { get; private set; }

        public Vector2 LastMoveTo => MoveToCalls[MoveToCalls.Count - 1];

        public bool MoveTo(Vector2 destination)
        {
            MoveToCalls.Add(destination);
            IsMoving = true;
            HasArrived = false;
            PathFailed = false;
            return true;
        }

        public void Stop()
        {
            StopCount++;
            IsMoving = false;
        }

        public void AimAt(Vector2 point) => AimCalls.Add(point);

        public void Watch(Vector2? point)
        {
        }

        public bool FireMainGun()
        {
            if (!CanFire)
                return false;
            MainGunShots++;
            return true;
        }

        public void SetMachineGuns(bool held, Vector2 target) => MachineGunsHeld = held;

        public void HoldHullHeading(Vector2? heading) => HullHeading = heading;

        public void Arrive()
        {
            IsMoving = false;
            HasArrived = true;
        }

        public void FailPath()
        {
            IsMoving = false;
            HasArrived = false;
            PathFailed = true;
        }
    }
}
