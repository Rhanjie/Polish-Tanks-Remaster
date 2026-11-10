using UnityEngine;

namespace PolishTanks.CameraRig
{
    /// <summary>Kicks the view in a direction, scaled by force. Weapons use it for the punch of a shot.</summary>
    public interface ICameraShake
    {
        void Shake(Vector2 direction, float force);
    }
}
