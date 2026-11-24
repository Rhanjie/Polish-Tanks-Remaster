using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.AI.Behaviours
{
    /// <summary>
    /// The target is out of sight but known (it slipped away, fired from somewhere, or a friend radioed it), or it has
    /// only just come into view and the crew is still reacting. The search runs in phases: drive to the last known spot
    /// (when the doctrine allows), look down every open direction from there, starting with the way the target was
    /// heading (round the corner it took), then roam loosely around the spot for the doctrine's search time. After that
    /// the target is forgotten and the unit goes back to its orders. Any fresh report or another escape starts over
    /// from the drive. Off while combat is scripted.
    /// </summary>
    public sealed class SearchBehaviour : AiBehaviour
    {
        public const float BaseScore = 0.5f;
        public const float ArriveDistance = 3f;
        public const float OpenDirectionDistance = 6f;
        public const float LookDistance = 10f;
        public const int DirectionCount = 8;
        public const int RoamPointAttempts = 8;
        public const float MinRoamStep = 3f;

        private readonly Func<float> random01;
        private readonly List<Vector2> lookDirections = new List<Vector2>(DirectionCount);
        private SearchPhase phase = SearchPhase.Done;
        private int revision;
        private int lookIndex;
        private float phaseClock;
        private float searchEnds;
        private Vector2 lookOrigin;
        private Vector2 roamAim;

        public SearchBehaviour() : this(() => UnityEngine.Random.value)
        {
        }

        public SearchBehaviour(Func<float> random01)
        {
            this.random01 = random01;
        }

        public SearchPhase Phase => phase;
        public IReadOnlyList<Vector2> LookDirections => lookDirections;

        public override AiBehaviourKind Kind => AiBehaviourKind.Search;

        public override float Score(AiContext context) =>
            !context.ScriptedCombat && context.Target.IsKnown && !context.IsTargetSpotted ? BaseScore : 0f;

        public override void Enter(AiContext context) => Approach(context);

        public override void Tick(AiContext context)
        {
            if (context.Target.Revision != revision)
                Approach(context);

            switch (phase)
            {
                case SearchPhase.Approach:
                    TickApproach(context);
                    break;
                case SearchPhase.LookAround:
                    TickLookAround(context);
                    break;
                case SearchPhase.Roam:
                    TickRoam(context);
                    break;
            }
        }

        public override void Exit(AiContext context)
        {
            context.Driver.Stop();
            phase = SearchPhase.Done;
        }

        private void Approach(AiContext context)
        {
            revision = context.Target.Revision;
            phase = SearchPhase.Approach;
            var driver = context.Driver;
            var lead = context.Target.LastKnownPosition;
            if (context.Doctrine.SearchLostTarget && Vector2.Distance(driver.Position, lead) > ArriveDistance && driver.MoveTo(lead))
                return;
            StartLookingAround(context);
        }

        private void TickApproach(AiContext context)
        {
            var driver = context.Driver;
            var lead = context.Target.LastKnownPosition;
            driver.AimAt(lead);
            if (context.Target.IsVisible)
                return;
            if (!driver.IsMoving || Vector2.Distance(driver.Position, lead) <= ArriveDistance)
                StartLookingAround(context);
        }

        private void StartLookingAround(AiContext context)
        {
            var driver = context.Driver;
            driver.Stop();
            phase = SearchPhase.LookAround;
            lookOrigin = driver.Position;
            lookIndex = 0;
            phaseClock = context.Time + context.Doctrine.LookSeconds;

            var target = context.Target;
            var preferred = target.LastHeading;
            if (preferred == Vector2.zero)
                preferred = target.LastKnownPosition - lookOrigin;
            if (preferred == Vector2.zero)
                preferred = driver.Forward;

            lookDirections.Clear();
            for (var i = 0; i < DirectionCount; i++)
            {
                var angle = i * Mathf.PI * 2f / DirectionCount;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                if (IsOpen(context, direction))
                    lookDirections.Add(direction);
            }
            lookDirections.Sort((a, b) => Vector2.Angle(a, preferred).CompareTo(Vector2.Angle(b, preferred)));

            if (lookDirections.Count == 0)
                StartRoaming(context);
        }

        private bool IsOpen(AiContext context, Vector2 direction) =>
            context.SearchSpace == null
            || context.SearchSpace.ClearDistance(lookOrigin, direction, OpenDirectionDistance) >= OpenDirectionDistance;

        private void TickLookAround(AiContext context)
        {
            if (context.Time >= phaseClock)
            {
                lookIndex++;
                phaseClock = context.Time + context.Doctrine.LookSeconds;
            }
            if (lookIndex >= lookDirections.Count)
            {
                StartRoaming(context);
                return;
            }
            context.Driver.AimAt(lookOrigin + lookDirections[lookIndex] * LookDistance);
        }

        private void StartRoaming(AiContext context)
        {
            phase = SearchPhase.Roam;
            searchEnds = context.Time + context.Doctrine.SearchSeconds;
            phaseClock = context.Time;
            roamAim = context.Target.LastKnownPosition;
        }

        private void TickRoam(AiContext context)
        {
            var driver = context.Driver;
            if (context.Time >= searchEnds)
            {
                phase = SearchPhase.Done;
                driver.Stop();
                context.Target.Clear();
                return;
            }

            if (context.Doctrine.SearchLostTarget)
            {
                if (!driver.IsMoving)
                    DriveToRoamPoint(context);
            }
            else if (context.Time >= phaseClock)
            {
                roamAim = driver.Position + RandomDirection() * LookDistance;
                phaseClock = context.Time + context.Doctrine.LookSeconds * 2f;
            }
            driver.AimAt(roamAim);
        }

        private void DriveToRoamPoint(AiContext context)
        {
            var centre = context.Target.LastKnownPosition;
            var radius = Mathf.Max(MinRoamStep, context.Doctrine.RoamRadius);
            for (var attempt = 0; attempt < RoamPointAttempts; attempt++)
            {
                var point = centre + RandomDirection() * Mathf.Lerp(MinRoamStep, radius, random01());
                if (context.SearchSpace == null || !context.SearchSpace.IsWalkable(point))
                    continue;
                if (context.Driver.MoveTo(point))
                {
                    roamAim = point;
                    return;
                }
            }
        }

        private Vector2 RandomDirection()
        {
            var angle = random01() * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}
