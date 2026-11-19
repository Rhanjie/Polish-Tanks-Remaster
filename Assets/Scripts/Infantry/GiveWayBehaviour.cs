using System;
using PolishTanks.AI;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// A tank is coming at the soldier: drop everything and run to the side step the <see cref="GiveWayPlanner"/> picked.
    /// Beats every other behaviour while the danger lasts, so the soldier gives way also during orders and fights.
    /// </summary>
    public sealed class GiveWayBehaviour : AiBehaviour
    {
        public const float BaseScore = 0.95f;
        public const float NewStepDistance = 1f;

        private readonly Func<Vector2?> sideStep;
        private readonly InfantryDriver driver;
        private Vector2 steppingTo;

        public GiveWayBehaviour(Func<Vector2?> sideStep, InfantryDriver driver)
        {
            this.sideStep = sideStep;
            this.driver = driver;
        }

        public override AiBehaviourKind Kind => AiBehaviourKind.GiveWay;

        public override float Score(AiContext context) => sideStep().HasValue ? BaseScore : 0f;

        public override void Enter(AiContext context) => Step();

        public override void Tick(AiContext context)
        {
            var step = sideStep();
            if (step.HasValue && (!driver.IsMoving || Vector2.Distance(step.Value, steppingTo) > NewStepDistance))
                Step();
        }

        public override void Exit(AiContext context) => driver.Stop();

        private void Step()
        {
            var step = sideStep();
            if (!step.HasValue)
                return;
            steppingTo = step.Value;
            driver.StepTo(steppingTo);
        }
    }
}
