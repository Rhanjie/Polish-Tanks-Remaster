using System;
using System.Collections.Generic;

namespace PolishTanks.Bouncing
{
    /// <summary>
    /// Counts bounces and keeps impact statistics. Contacts that come too soon after the previous bounce,
    /// or are too soft, are ignored so that resting jitter and multi-contact collisions are not counted twice.
    /// </summary>
    public sealed class BounceTracker
    {
        public const int DefaultHistorySize = 32;

        private readonly float _minInterval;
        private readonly float _minImpactSpeed;
        private readonly FloatRingBuffer _impactSpeeds;
        private readonly FloatRingBuffer _intervals;
        private float _lastBounceTime;

        public BounceTracker(float minInterval = 0.05f, float minImpactSpeed = 0.5f, int historySize = DefaultHistorySize)
        {
            if (minInterval < 0f)
                throw new ArgumentOutOfRangeException(nameof(minInterval), minInterval, "Must not be negative.");
            if (minImpactSpeed < 0f)
                throw new ArgumentOutOfRangeException(nameof(minImpactSpeed), minImpactSpeed, "Must not be negative.");

            _minInterval = minInterval;
            _minImpactSpeed = minImpactSpeed;
            _impactSpeeds = new FloatRingBuffer(historySize);
            _intervals = new FloatRingBuffer(historySize);
        }

        public int Count { get; private set; }
        public float LastImpactSpeed { get; private set; }
        public float PeakImpactSpeed { get; private set; }
        public float LastInterval { get; private set; }

        /// <summary>Impact speeds of the most recent bounces, oldest first.</summary>
        public IReadOnlyList<float> ImpactHistory => _impactSpeeds;
        public int HistoryCapacity => _impactSpeeds.Capacity;

        public float AverageInterval => _intervals.Average();
        public float BouncesPerMinute => AverageInterval > 0f ? 60f / AverageInterval : 0f;

        public bool TryRegister(float time, float impactSpeed, out BounceEvent bounce)
        {
            bounce = default;
            if (float.IsNaN(impactSpeed) || impactSpeed < _minImpactSpeed)
                return false;

            var interval = Count == 0 ? 0f : time - _lastBounceTime;
            if (Count > 0 && interval < _minInterval)
                return false;

            Count++;
            _lastBounceTime = time;
            LastImpactSpeed = impactSpeed;
            LastInterval = interval;
            if (impactSpeed > PeakImpactSpeed)
                PeakImpactSpeed = impactSpeed;

            _impactSpeeds.Add(impactSpeed);
            if (Count > 1)
                _intervals.Add(interval);

            bounce = new BounceEvent(Count, impactSpeed, interval, time);
            return true;
        }

        public void Reset()
        {
            Count = 0;
            LastImpactSpeed = 0f;
            PeakImpactSpeed = 0f;
            LastInterval = 0f;
            _lastBounceTime = 0f;
            _impactSpeeds.Clear();
            _intervals.Clear();
        }
    }
}
