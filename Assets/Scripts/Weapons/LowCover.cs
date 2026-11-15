using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// Marks an object as lower than a turret gun's barrel, like a crate, a barrel or a low wall. A tank standing close
    /// behind it fires over it: shots ignore low cover within the gun's cover clearance of where they were fired from,
    /// while shots from further away still hit it. That lets a tank hide behind crates and keep shooting back.
    /// </summary>
    [DisallowMultipleComponent]
    public class LowCover : MonoBehaviour
    {
        public static bool ShootsOver(Vector2 hitPoint, Vector2 firedFrom, float clearance, bool ricocheted) =>
            !ricocheted && clearance > 0f && (hitPoint - firedFrom).sqrMagnitude <= clearance * clearance;
    }
}
