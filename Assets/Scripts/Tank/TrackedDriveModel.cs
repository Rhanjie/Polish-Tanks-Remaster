using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Speed and turn rate of a tracked hull. The tracks don't slip sideways,
    /// so the hull only moves along its forward axis.
    /// </summary>
    public sealed class TrackedDriveModel
    {
        private const float StoppedSpeed = 0.01f;

        public TankDriveSettings Settings { get; set; }

        public float Speed { get; private set; }

        public float AngularSpeed { get; private set; }

        public TrackedDriveModel(TankDriveSettings settings)
        {
            Settings = settings;
        }

        public void SyncSpeed(float measuredSpeed)
        {
            Speed = Mathf.Clamp(measuredSpeed, -Settings.maxReverseSpeed, Settings.maxForwardSpeed);
        }

        public void Step(float throttle, float steer, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            throttle = Mathf.Clamp(throttle, -1f, 1f);
            steer = Mathf.Clamp(steer, -1f, 1f);
            var s = Settings;

            var targetSpeed = throttle >= 0f ? throttle * s.maxForwardSpeed : throttle * s.maxReverseSpeed;
            Speed = Mathf.MoveTowards(Speed, targetSpeed, SpeedChangeRate(throttle) * deltaTime);

            var speedFactor = s.maxForwardSpeed > 0f ? Mathf.Clamp01(Mathf.Abs(Speed) / s.maxForwardSpeed) : 0f;
            var maxTurn = Mathf.Lerp(s.turnRate, s.turnRateAtMaxSpeed, speedFactor);
            AngularSpeed = Mathf.MoveTowards(AngularSpeed, -steer * maxTurn, s.turnAcceleration * deltaTime);
        }

        public void Reset()
        {
            Speed = 0f;
            AngularSpeed = 0f;
        }

        private float SpeedChangeRate(float throttle)
        {
            if (Mathf.Approximately(throttle, 0f))
                return Settings.coastDeceleration;

            var opposesMotion = Mathf.Abs(Speed) > StoppedSpeed && Mathf.Sign(throttle) != Mathf.Sign(Speed);
            return opposesMotion ? Settings.braking : Settings.acceleration;
        }
    }
}
