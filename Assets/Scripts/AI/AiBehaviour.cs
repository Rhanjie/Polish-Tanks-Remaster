namespace PolishTanks.AI
{
    /// <summary>
    /// One thing an AI tank can be doing (follow orders, engage, search). The <see cref="UtilityBrain"/> asks every
    /// behaviour for a score each tick and runs the best one; it calls <see cref="Enter"/> and <see cref="Exit"/> when
    /// it switches. New tactics (flanking, cover, angling the hull) are new subclasses with their own scores.
    /// </summary>
    public abstract class AiBehaviour
    {
        public abstract AiBehaviourKind Kind { get; }

        public abstract float Score(AiContext context);

        public virtual void Enter(AiContext context)
        {
        }

        public abstract void Tick(AiContext context);

        public virtual void Exit(AiContext context)
        {
        }
    }
}
