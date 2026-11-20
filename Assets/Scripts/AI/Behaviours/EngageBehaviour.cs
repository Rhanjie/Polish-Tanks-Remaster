namespace PolishTanks.AI.Behaviours
{
    /// <summary>
    /// The target is in sight and spotted: stop and fight from where the tank stands, with the hull angled to the
    /// target and swaying (<see cref="HullAngling"/>). Aiming and firing belong to the <see cref="Gunner"/>, which also
    /// shoots while the tank does anything else.
    /// Off while combat is scripted, so the designer's orders keep the hull.
    /// </summary>
    public sealed class EngageBehaviour : AiBehaviour
    {
        public const float BaseScore = 0.9f;

        private readonly HullAngling angling;

        public EngageBehaviour() : this(() => UnityEngine.Random.value)
        {
        }

        public EngageBehaviour(System.Func<float> random01)
        {
            angling = new HullAngling(random01);
        }

        public override AiBehaviourKind Kind => AiBehaviourKind.Engage;

        public override float Score(AiContext context) => !context.ScriptedCombat && context.IsTargetSpotted ? BaseScore : 0f;

        public override void Enter(AiContext context) => context.Driver.Stop();

        public override void Tick(AiContext context)
        {
            if (context.Driver is not ITankDriver tank)
                return;
            var toTarget = context.Target.LastKnownPosition - tank.Position;
            tank.HoldHullHeading(angling.DesiredForward(toTarget, context.Time, context.Doctrine));
        }

        public override void Exit(AiContext context)
        {
            if (context.Driver is ITankDriver tank)
                tank.HoldHullHeading(null);
        }
    }
}
