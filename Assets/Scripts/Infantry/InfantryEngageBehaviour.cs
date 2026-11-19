using System;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// The player is spotted. With cover nearby the soldier runs to it and fights from behind it, moving on whenever the
    /// player gets round it. With no cover it closes to fighting distance in the open. A grenadier with a grenade ready
    /// leaves cover and runs in to throwing distance. The shooting and throwing themselves are done by the soldier's
    /// AI, also while running. Off while combat is scripted.
    /// </summary>
    public sealed class InfantryEngageBehaviour : AiBehaviour
    {
        public const float BaseScore = 0.9f;
        public const float ReplanDistance = 3f;
        public const float ReplanSeconds = 1f;
        public const float AtCoverDistance = 0.9f;
        public const float CoverMovedDistance = 0.8f;

        private readonly Func<float> fightingDistance;
        private readonly Func<Vector2?> coverSpot;
        private readonly Func<bool> closingIn;
        private Vector2 headingFor;
        private float replanAt;

        public InfantryEngageBehaviour(Func<float> fightingDistance, Func<Vector2?> coverSpot, Func<bool> closingIn)
        {
            this.fightingDistance = fightingDistance;
            this.coverSpot = coverSpot;
            this.closingIn = closingIn;
        }

        public override AiBehaviourKind Kind => AiBehaviourKind.Engage;

        public override float Score(AiContext context) => !context.ScriptedCombat && context.IsTargetSpotted ? BaseScore : 0f;

        public override void Enter(AiContext context)
        {
            replanAt = context.Time;
            headingFor = new Vector2(float.NaN, float.NaN);
        }

        public override void Tick(AiContext context)
        {
            var cover = closingIn() ? null : coverSpot();
            if (cover.HasValue)
                HoldCover(context, cover.Value);
            else
                CloseIn(context);
        }

        public override void Exit(AiContext context) => context.Driver.Stop();

        private void HoldCover(AiContext context, Vector2 spot)
        {
            var driver = context.Driver;
            if (Vector2.Distance(driver.Position, spot) <= AtCoverDistance)
            {
                if (driver.IsMoving)
                    driver.Stop();
                return;
            }
            if (!driver.IsMoving || !(Vector2.Distance(headingFor, spot) <= CoverMovedDistance))
            {
                headingFor = spot;
                driver.MoveTo(spot);
            }
        }

        private void CloseIn(AiContext context)
        {
            var driver = context.Driver;
            var target = context.Target.LastKnownPosition;
            if (Vector2.Distance(driver.Position, target) <= fightingDistance())
            {
                if (driver.IsMoving)
                    driver.Stop();
                return;
            }

            if (!driver.IsMoving || context.Time >= replanAt && !(Vector2.Distance(headingFor, target) <= ReplanDistance))
            {
                headingFor = target;
                replanAt = context.Time + ReplanSeconds;
                driver.MoveTo(target);
            }
        }
    }
}
