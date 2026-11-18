using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// What an AI knows about one target. Seeing it keeps its position fresh; once it slips out of sight the last
    /// known position is remembered until the memory runs out. A report (a shell from the target, later a call from a
    /// friend) gives a position without sight. A target counts as spotted only after it has stayed in view for the
    /// reaction time, so a tank never fires the same frame the player rounds a corner.
    /// </summary>
    public sealed class TargetMemory
    {
        public TargetAwareness Awareness { get; private set; }
        public Vector2 LastKnownPosition { get; private set; }
        public float LastContactTime { get; private set; } = float.NegativeInfinity;
        public float VisibleSince { get; private set; } = float.NegativeInfinity;

        public bool IsVisible => Awareness == TargetAwareness.Visible;
        public bool IsKnown => Awareness != TargetAwareness.Unaware;

        public void Observe(bool visible, Vector2 position, float time)
        {
            if (visible)
            {
                if (!IsVisible)
                    VisibleSince = time;
                Awareness = TargetAwareness.Visible;
                LastKnownPosition = position;
                LastContactTime = time;
                return;
            }

            if (IsVisible)
                Awareness = TargetAwareness.Remembered;
        }

        public void Report(Vector2 position, float time)
        {
            if (IsVisible)
                return;
            Awareness = TargetAwareness.Remembered;
            LastKnownPosition = position;
            LastContactTime = time;
        }

        public void Forget(float time, float memorySeconds)
        {
            if (Awareness == TargetAwareness.Remembered && time - LastContactTime > memorySeconds)
                Awareness = TargetAwareness.Unaware;
        }

        public void Clear() => Awareness = TargetAwareness.Unaware;

        public bool IsSpotted(float time, float reactionTime) => IsVisible && time - VisibleSince >= reactionTime;

        public float SecondsSinceContact(float time) => time - LastContactTime;
    }
}
