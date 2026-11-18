using UnityEngine;

namespace PolishTanks.AI.Behaviours
{
    /// <summary>
    /// The target is out of sight but remembered (last seen there, or it fired from there), or it has only just come
    /// into view and the crew is still reacting: turn the turret to that spot and, when the doctrine allows, drive over
    /// to look. A fresher position more than a few metres away sends the tank there instead. Ends when the target is
    /// spotted or the memory runs out. Off while combat is scripted.
    /// </summary>
    public sealed class SearchBehaviour : AiBehaviour
    {
        public const float BaseScore = 0.5f;
        public const float NewLeadDistance = 4f;

        private Vector2 heading;

        public override AiBehaviourKind Kind => AiBehaviourKind.Search;

        public override float Score(AiContext context) =>
            !context.ScriptedCombat && context.Target.IsKnown && !context.IsTargetSpotted ? BaseScore : 0f;

        public override void Enter(AiContext context)
        {
            context.Driver.Stop();
            DriveToLead(context);
        }

        public override void Tick(AiContext context)
        {
            var lead = context.Target.LastKnownPosition;
            context.Driver.AimAt(lead);
            if (Vector2.Distance(lead, heading) > NewLeadDistance)
                DriveToLead(context);
        }

        public override void Exit(AiContext context) => context.Driver.Stop();

        private void DriveToLead(AiContext context)
        {
            heading = context.Target.LastKnownPosition;
            if (context.Doctrine.SearchLostTarget)
                context.Driver.MoveTo(heading);
        }
    }
}
