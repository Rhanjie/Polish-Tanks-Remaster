using UnityEngine;

namespace PolishTanks.CameraRig
{
    /// <summary>The camera the player looks through, for screen to world conversion and view center.</summary>
    public interface IGameCamera
    {
        Vector2 Position { get; }
        Vector2 ScreenToWorld(Vector2 screenPosition);
    }
}
