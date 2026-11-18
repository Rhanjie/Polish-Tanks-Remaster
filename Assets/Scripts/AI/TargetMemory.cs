using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// What an AI knows about one target. Seeing it keeps its position fresh; once it slips out of sight the last
    /// known position is remembered until the memory runs out. A report (a shell from the target, later a call from a
    /// friend) gives a position without sight. A target counts as spotted only after it has stayed in view for the
    /// reaction time, so a tank never fires the same frame the player rounds a corner. <see cref="Revision"/> goes up
    /// whenever the trail changes in a way a search should start over for (the target slipped away, or a report came
    /// in), and <see cref="LastHeading"/> keeps which way the target was moving when last seen.
    /// </summary>
    public sealed class TargetMemory
    {
        private const float MinHeadingStep = 0.05f;

        public TargetAwareness Awareness { get; private set; }
        public Vector2 LastKnownPosition { get; private set; }
        public float LastContactTime { get; private set; } = float.NegativeInfinity;
        public float VisibleSince { get; private set; } = float.NegativeInfinity;
        public Vector2 LastHeading { get; private set; }
        public int Revision { get; private set; }

        public bool IsVisible => Awareness == TargetAwareness.Visible;
        public bool IsKnown => Awareness != TargetAwareness.Unaware;

        public void Observe(bool visible, Vector2 position, float time)
        {
            if (visible)
            {
                if (!IsVisible)
                    VisibleSince = time;
                else if ((position - LastKnownPosition).sqrMagnitude > MinHeadingStep * MinHeadingStep)
                    LastHeading = (position - LastKnownPosition).normalized;
                Awareness = TargetAwareness.Visible;
                LastKnownPosition = position;
                LastContactTime = time;
                return;
            }

            if (IsVisible)
            {
                Awareness = TargetAwareness.Remembered;
                Revision++;
            }
        }

        public void Report(Vector2 position, float time)
        {
            if (IsVisible)
                return;
            Awareness = TargetAwareness.Remembered;
            LastKnownPosition = position;
            LastContactTime = time;
            LastHeading = Vector2.zero;
            Revision++;
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
