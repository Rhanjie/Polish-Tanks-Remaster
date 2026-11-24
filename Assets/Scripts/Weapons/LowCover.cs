using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// Marks an object as lower than a turret gun's barrel, like a crate, a barrel or a low wall. A tank standing close
    /// behind it fires over it: shots ignore low cover within the gun's cover clearance of where they were fired from,
    /// while shots from further away still hit it. That lets a tank hide behind crates and keep shooting back.
    /// <see cref="IsInLine"/> tells a gun mounted too low to clear anything (a hull machine gun) to hold fire.
    /// </summary>
    [DisallowMultipleComponent]
    public class LowCover : MonoBehaviour
    {
        private static readonly System.Collections.Generic.List<RaycastHit2D> Hits = new System.Collections.Generic.List<RaycastHit2D>(16);

        public static bool IsInLine(Vector2 from, Vector2 to, Rigidbody2D shooter)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            Physics2D.Linecast(from, to, filter, Hits);
            foreach (var hit in Hits)
                if ((shooter == null || hit.rigidbody != shooter) && hit.collider.GetComponentInParent<LowCover>() != null)
                    return true;
            return false;
        }

        public static bool ShootsOver(Vector2 hitPoint, Vector2 firedFrom, float clearance, bool ricocheted) =>
            !ricocheted && clearance > 0f && (hitPoint - firedFrom).sqrMagnitude <= clearance * clearance;
    }
}
