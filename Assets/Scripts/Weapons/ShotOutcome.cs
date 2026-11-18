using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>What one of a shooter's projectiles did on impact, sent back to the shooter through <see cref="IShotOutcomeReceiver"/>.</summary>
    public readonly struct ShotOutcome
    {
        public readonly Collider2D Target;
        public readonly HitOutcome Outcome;
        public readonly Vector2 Point;
        public readonly float Caliber;

        public ShotOutcome(Collider2D target, HitOutcome outcome, Vector2 point, float caliber)
        {
            Target = target;
            Outcome = outcome;
            Point = point;
            Caliber = caliber;
        }
    }
}
