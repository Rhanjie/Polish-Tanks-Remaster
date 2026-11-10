using UnityEngine;

namespace PolishTanks.CameraRig
{
    /// <summary>
    /// Kicks the view in a direction, scaled by force, as if it came from a world point.
    /// Weapons use it for the punch of a shot and pass the muzzle as the origin.
    /// </summary>
    public interface ICameraShake
    {
        void Shake(Vector2 origin, Vector2 direction, float force);
    }
}
