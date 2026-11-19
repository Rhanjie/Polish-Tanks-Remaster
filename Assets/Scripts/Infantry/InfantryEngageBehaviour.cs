using System;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// The player is spotted: get within fighting distance and fight from there. A rifleman closes to most of its rifle
    /// range; a grenadier with a grenade ready runs in to throwing distance. The shooting and throwing themselves are
    /// done by the soldier's AI whenever it stands facing the target. Off while combat is scripted.
    /// </summary>
    public sealed class InfantryEngageBehaviour : AiBehaviour
    {
        public const float BaseScore = 0.9f;
        public const float ReplanDistance = 3f;
        public const float ReplanSeconds = 1f;

        private readonly Func<float> fightingDistance;
        private Vector2 headingFor;
        private float replanAt;

        public InfantryEngageBehaviour(Func<float> fightingDistance)
        {
            this.fightingDistance = fightingDistance;
        }

        public override AiBehaviourKind Kind => AiBehaviourKind.Engage;

        public override float Score(AiContext context) => !context.ScriptedCombat && context.IsTargetSpotted ? BaseScore : 0f;

        public override void Enter(AiContext context)
        {
            context.Driver.Stop();
            replanAt = context.Time;
            headingFor = context.Driver.Position;
        }

        public override void Tick(AiContext context)
        {
            var driver = context.Driver;
            var target = context.Target.LastKnownPosition;
            var distance = Vector2.Distance(driver.Position, target);
            if (distance <= fightingDistance())
            {
                if (driver.IsMoving)
                    driver.Stop();
                driver.AimAt(target);
                return;
            }

            if (!driver.IsMoving || context.Time >= replanAt && Vector2.Distance(headingFor, target) > ReplanDistance)
            {
                headingFor = target;
                replanAt = context.Time + ReplanSeconds;
                driver.MoveTo(target);
            }
        }

        public override void Exit(AiContext context) => context.Driver.Stop();
    }
}
