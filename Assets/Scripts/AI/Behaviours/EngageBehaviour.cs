namespace PolishTanks.AI.Behaviours
{
    /// <summary>
    /// The target is in sight and spotted: stop and fight from where the tank stands, as the old sandbox sentries did.
    /// Aiming and firing belong to the <see cref="Gunner"/>, which also shoots while the tank does anything else.
    /// Off while combat is scripted, so the designer's orders keep the hull.
    /// </summary>
    public sealed class EngageBehaviour : AiBehaviour
    {
        public const float BaseScore = 0.9f;

        public override AiBehaviourKind Kind => AiBehaviourKind.Engage;

        public override float Score(AiContext context) => !context.ScriptedCombat && context.IsTargetSpotted ? BaseScore : 0f;

        public override void Enter(AiContext context) => context.Driver.Stop();

        public override void Tick(AiContext context)
        {
        }
    }
}
