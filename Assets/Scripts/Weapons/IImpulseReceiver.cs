using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// A body that handles projectile hits itself instead of taking raw Rigidbody2D forces, such as a tank whose
    /// drive overwrites its velocity every physics step. An off-centre hit also turns it, and a hard one can knock
    /// it off its controls for a moment.
    /// </summary>
    public interface IImpulseReceiver
    {
        void ReceiveHit(Vector2 impulse, Vector2 worldPoint);
    }
}
