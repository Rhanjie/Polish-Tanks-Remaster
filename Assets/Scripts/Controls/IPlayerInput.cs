using UnityEngine;

namespace PolishTanks.Controls
{
    /// <summary>
    /// What the player is pressing this frame, independent of the input backend.
    /// Move is x = steer, y = throttle; the pointer is in screen pixels.
    /// </summary>
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        Vector2 PointerScreenPosition { get; }
        bool FireHeld { get; }
        bool FirePressedThisFrame { get; }
        bool SecondaryFireHeld { get; }
        bool SecondaryFirePressedThisFrame { get; }
    }
}
