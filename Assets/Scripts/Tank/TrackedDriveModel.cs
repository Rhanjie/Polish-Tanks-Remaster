using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Speed and turn rate of a tracked hull. The tracks don't slip sideways,
    /// so the hull only moves along its forward axis. A weak engine or field-patched track scales speed and turning;
    /// with one track out only the other one pulls, so the hull pivots round the dead side; with no engine or
    /// no tracks it stops.
    /// </summary>
    public sealed class TrackedDriveModel
    {
        private const float StoppedSpeed = 0.01f;

        public TankDriveSettings Settings { get; set; }

        public DriveCondition Condition { get; set; } = DriveCondition.Healthy;

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
            var c = Condition;

            if (c.Power <= 0f || (c.LeftTrackOut && c.RightTrackOut))
            {
                Approach(0f, 0f, s.braking, deltaTime);
                return;
            }

            if (c.LeftTrackOut || c.RightTrackOut)
            {
                StepCrippled(throttle, steer, deltaTime);
                return;
            }

            var mobility = c.Power * Mathf.Min(c.LeftTrack, c.RightTrack);
            var targetSpeed = mobility * (throttle >= 0f ? throttle * s.maxForwardSpeed : throttle * s.maxReverseSpeed);
            var speedFactor = s.maxForwardSpeed > 0f ? Mathf.Clamp01(Mathf.Abs(Speed) / s.maxForwardSpeed) : 0f;
            var maxTurn = mobility * Mathf.Lerp(s.turnRate, s.turnRateAtMaxSpeed, speedFactor);
            Approach(targetSpeed, -steer * maxTurn, SpeedChangeRate(throttle), deltaTime);
        }

        public void Reset()
        {
            Speed = 0f;
            AngularSpeed = 0f;
        }

        private void StepCrippled(float throttle, float steer, float deltaTime)
        {
            var s = Settings;
            var c = Condition;
            var leftOut = c.LeftTrackOut;
            var goodTrack = leftOut ? c.RightTrack : c.LeftTrack;
            var drive = Mathf.Clamp(leftOut ? throttle - steer : throttle + steer, -1f, 1f) * c.Power * goodTrack;
            var towardDeadSide = leftOut ? 1f : -1f;
            Approach(drive * s.maxForwardSpeed * c.CrippledCrawl, towardDeadSide * drive * s.turnRate, s.braking, deltaTime);
        }

        private void Approach(float targetSpeed, float targetTurn, float speedRate, float deltaTime)
        {
            Speed = Mathf.MoveTowards(Speed, targetSpeed, speedRate * deltaTime);
            AngularSpeed = Mathf.MoveTowards(AngularSpeed, targetTurn, Settings.turnAcceleration * deltaTime);
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
