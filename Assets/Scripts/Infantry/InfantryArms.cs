using System;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// The timing of a soldier's weapons: rifle bursts with a varied pause between them, the grenade coming ready again
    /// a while after each throw (and at a random moment at first, so a squad doesn't throw all at once), and the short
    /// moments a soldier is busy shooting or throwing and won't notice a tank coming.
    /// </summary>
    public sealed class InfantryArms
    {
        public const float FirstGrenadeSpread = 0.5f;
        public const float PauseVariation = 1f / 3f;
        public const float CooldownVariation = 0.2f;

        private readonly RifleSettings rifle;
        private readonly GrenadeSettings grenades;
        private readonly Func<float> random01;
        private int shotsLeftInBurst;
        private float nextShot = float.NegativeInfinity;
        private float grenadeReadyAt;
        private float busyUntil = float.NegativeInfinity;

        public InfantryArms(RifleSettings rifle, GrenadeSettings grenades, Func<float> random01, float time)
        {
            this.rifle = rifle;
            this.grenades = grenades;
            this.random01 = random01;
            grenadeReadyAt = time + grenades.cooldown * FirstGrenadeSpread * random01();
        }

        public RifleSettings Rifle => rifle;
        public GrenadeSettings Grenades => grenades;
        public bool HasGrenades => grenades.IsCarried;

        public bool IsGrenadeReady(float time) => HasGrenades && time >= grenadeReadyAt;

        public bool IsBusy(float time) => time < busyUntil;

        public bool TryShoot(float time)
        {
            if (time < nextShot)
                return false;
            if (shotsLeftInBurst <= 0)
                shotsLeftInBurst = Mathf.Max(1, rifle.burstShots);
            shotsLeftInBurst--;
            var wait = shotsLeftInBurst > 0 ? rifle.shotInterval : rifle.burstPause * Vary(PauseVariation);
            nextShot = time + Mathf.Max(rifle.shotInterval, wait);
            busyUntil = time + rifle.shotInterval;
            return true;
        }

        public void Throw(float time)
        {
            grenadeReadyAt = time + grenades.cooldown * Vary(CooldownVariation);
            busyUntil = time + grenades.windup;
        }

        public Vector2 Spread(Vector2 direction, float factor = 1f)
        {
            var angle = (random01() * 2f - 1f) * rifle.spread * factor;
            return Quaternion.Euler(0f, 0f, angle) * direction;
        }

        private float Vary(float amount) => 1f + (random01() * 2f - 1f) * amount;
    }
}
