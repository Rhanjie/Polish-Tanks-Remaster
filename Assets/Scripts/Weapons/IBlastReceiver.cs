using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// Something soft that an explosion hurts by how close it went off (a soldier), instead of rolling against armor
    /// the way a tank or a crate does.
    /// </summary>
    public interface IBlastReceiver
    {
        void OnBlast(Vector2 centre, float distance, in BlastSettings blast, Rigidbody2D source);
    }
}
