using System;

namespace PolishTanks.Damage
{
    /// <summary>One concrete part on a vehicle. Only vision blocks come in numbers, so the index is zero for the rest.</summary>
    public readonly struct PartId : IEquatable<PartId>
    {
        public readonly VehiclePart Part;
        public readonly int Index;

        public PartId(VehiclePart part, int index = 0)
        {
            Part = part;
            Index = index;
        }

        public bool Equals(PartId other) => Part == other.Part && Index == other.Index;
        public override bool Equals(object obj) => obj is PartId other && Equals(other);
        public override int GetHashCode() => ((int)Part * 397) ^ Index;
        public override string ToString() => Part == VehiclePart.VisionBlock ? $"{Part} {Index}" : Part.ToString();

        public static bool operator ==(PartId a, PartId b) => a.Equals(b);
        public static bool operator !=(PartId a, PartId b) => !a.Equals(b);
    }
}
