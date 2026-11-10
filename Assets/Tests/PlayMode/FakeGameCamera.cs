using PolishTanks.CameraRig;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Game camera for tests: fixed position, screen coordinates map straight to world.</summary>
    internal sealed class FakeGameCamera : IGameCamera
    {
        public Vector2 Position => Vector2.zero;
        public Vector2 ScreenToWorld(Vector2 screenPosition) => screenPosition;
    }
}
