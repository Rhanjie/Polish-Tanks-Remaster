using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// How far a gun's shots stray right now, in degrees. Driving, turning the hull or traversing the turret and every
    /// shot open the spread up; standing still lets it close back over the gun's aim time, down to the gun's settled
    /// spread, which never reaches zero. <see cref="CrewFactor"/> scales the whole spread, so a crew's experience can
    /// tighten or loosen it.
    /// </summary>
    public sealed class GunDispersion
    {
        public const float MinimumSettled = 0.05f;
        public const float RiseTime = 0.15f;

        private GunSettings settings;
        private float spread;
        private float crewFactor = 1f;

        public GunDispersion(GunSettings settings)
        {
            Settings = settings;
            Settle();
        }

        public GunSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                spread = Mathf.Clamp(spread, SettledRaw, MaximumRaw);
            }
        }

        public float CrewFactor
        {
            get => crewFactor;
            set => crewFactor = Mathf.Max(0.1f, value);
        }

        public float Current => spread * crewFactor;

        public float Settled => SettledRaw * crewFactor;

        public float Maximum => MaximumRaw * crewFactor;

        private float SettledRaw => Mathf.Max(MinimumSettled, settings.dispersion);

        private float MaximumRaw => Mathf.Max(SettledRaw, settings.maxDispersion);

        public float Target(float speed, float rotationSpeed)
        {
            var target = SettledRaw
                         + settings.movingDispersion * Mathf.Abs(speed)
                         + settings.turningDispersion * Mathf.Abs(rotationSpeed);
            return Mathf.Min(target, MaximumRaw);
        }

        public void Tick(float deltaTime, float speed, float rotationSpeed)
        {
            if (deltaTime <= 0f)
                return;

            var target = Target(speed, rotationSpeed);
            var time = spread < target ? RiseTime : settings.aimTime;
            spread = time > 0f
                ? target + (spread - target) * Mathf.Exp(-3f * deltaTime / time)
                : target;
        }

        public void OnShot() => spread = Mathf.Min(MaximumRaw, spread + settings.shotDispersion);

        public void Settle() => spread = SettledRaw;
    }
}
