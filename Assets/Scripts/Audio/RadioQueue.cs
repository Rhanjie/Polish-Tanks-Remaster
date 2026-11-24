using System.Collections.Generic;
using PolishTanks.Damage;

namespace PolishTanks.Audio
{
    /// <summary>
    /// Orders crew reports so they are spoken one at a time. A line already waiting, or spoken within the cooldown,
    /// is dropped so a burst of hits does not repeat the same report; the queue keeps only the latest few.
    /// </summary>
    public sealed class RadioQueue
    {
        private readonly Queue<CrewLine> waiting = new Queue<CrewLine>();
        private readonly Dictionary<CrewLine, float> lastSpoken = new Dictionary<CrewLine, float>();
        private readonly float cooldown;
        private readonly int capacity;

        public RadioQueue(float cooldown, int capacity = 3)
        {
            this.cooldown = cooldown;
            this.capacity = capacity;
        }

        public int Count => waiting.Count;

        public bool Enqueue(CrewLine line, float now)
        {
            if (waiting.Contains(line))
                return false;
            if (lastSpoken.TryGetValue(line, out var at) && now - at < cooldown)
                return false;
            if (waiting.Count >= capacity)
                waiting.Dequeue();
            waiting.Enqueue(line);
            return true;
        }

        public bool TryNext(float now, out CrewLine line)
        {
            if (waiting.Count == 0)
            {
                line = default;
                return false;
            }
            line = waiting.Dequeue();
            lastSpoken[line] = now;
            return true;
        }

        public void Clear() => waiting.Clear();

        public static CrewLine? ForPart(VehiclePart part) => part switch
        {
            VehiclePart.Engine => CrewLine.EngineDamaged,
            VehiclePart.LeftTrack => CrewLine.TrackBroken,
            VehiclePart.RightTrack => CrewLine.TrackBroken,
            VehiclePart.TurretRing => CrewLine.TurretJammed,
            VehiclePart.VisionBlock => CrewLine.PeriscopeDamaged,
            _ => null,
        };
    }
}
