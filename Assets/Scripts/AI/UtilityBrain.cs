using System.Collections.Generic;

namespace PolishTanks.AI
{
    /// <summary>
    /// Picks the highest scoring <see cref="AiBehaviour"/> every tick and runs it. The running behaviour gets a small
    /// bonus, so two behaviours with close scores don't swap back and forth every frame. A behaviour scoring zero or
    /// less never runs, and when nothing scores above zero the brain idles.
    /// </summary>
    public sealed class UtilityBrain
    {
        public const float Stickiness = 0.05f;

        private readonly List<AiBehaviour> behaviours;

        public UtilityBrain(IEnumerable<AiBehaviour> behaviours)
        {
            this.behaviours = new List<AiBehaviour>(behaviours);
        }

        public AiBehaviour Current { get; private set; }

        public IReadOnlyList<AiBehaviour> Behaviours => behaviours;

        public void Tick(AiContext context)
        {
            AiBehaviour best = null;
            var bestScore = 0f;
            foreach (var behaviour in behaviours)
            {
                var score = behaviour.Score(context);
                if (score <= 0f)
                    continue;
                if (behaviour == Current)
                    score += Stickiness;
                if (score > bestScore)
                {
                    best = behaviour;
                    bestScore = score;
                }
            }

            if (best != Current)
                Switch(best, context);
            Current?.Tick(context);
        }

        public void Reset(AiContext context) => Switch(null, context);

        private void Switch(AiBehaviour next, AiContext context)
        {
            Current?.Exit(context);
            Current = next;
            Current?.Enter(context);
        }
    }
}
