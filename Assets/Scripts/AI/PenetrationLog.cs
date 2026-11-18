using PolishTanks.Combat;

namespace PolishTanks.AI
{
    /// <summary>
    /// What an AI tank has learned from its own shells against its target: per plate (front, side, rear) how many
    /// hits landed and how many went through. Later tactics read it to decide between fighting head on and flanking.
    /// </summary>
    public sealed class PenetrationLog
    {
        private readonly int[] hits = new int[3];
        private readonly int[] penetrations = new int[3];

        public void Record(HitSide side, bool penetrated)
        {
            hits[(int)side]++;
            if (penetrated)
                penetrations[(int)side]++;
        }

        public int Hits(HitSide side) => hits[(int)side];

        public int Penetrations(HitSide side) => penetrations[(int)side];

        public float PenetrationRate(HitSide side, float unknown) =>
            hits[(int)side] == 0 ? unknown : (float)penetrations[(int)side] / hits[(int)side];

        public void Clear()
        {
            System.Array.Clear(hits, 0, hits.Length);
            System.Array.Clear(penetrations, 0, penetrations.Length);
        }
    }
}
