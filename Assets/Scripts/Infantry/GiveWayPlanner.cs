using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// Decides when a soldier has to get out of a tank's way, friend or foe, and where to step. A tank is a danger when
    /// it moves and the soldier stands in its lane (hull width plus a margin) close enough ahead to be reached within
    /// <see cref="LookAheadSeconds"/>. The soldier then sidesteps across the lane, normally to the nearer edge and onto
    /// walkable ground. It is a person, not a machine: it notices the tank only after a random reaction time, now and
    /// then jumps the wrong way, and sometimes steps too short.
    /// </summary>
    public sealed class GiveWayPlanner
    {
        public const float LookAheadSeconds = 2.5f;
        public const float MinThreatSpeed = 0.8f;
        public const float LaneMargin = 1.2f;
        public const float StepClearance = 0.8f;
        public const float MinReaction = 0.2f;
        public const float MaxReaction = 0.8f;
        public const float WrongWayChance = 0.2f;
        public const float ShortStepChance = 0.2f;
        public const float ShortStepFactor = 0.55f;

        private readonly Func<float> random01;
        private float reactAt = float.PositiveInfinity;
        private bool wrongWay;
        private bool shortStep;
        private bool threatened;

        public GiveWayPlanner(Func<float> random01)
        {
            this.random01 = random01;
        }

        public bool IsThreatened => threatened;

        public static bool IsInLane(Vector2 soldier, in TankThreat tank)
        {
            var speed = tank.Velocity.magnitude;
            if (speed < MinThreatSpeed)
                return false;
            var heading = tank.Velocity / speed;
            var offset = soldier - tank.Position;
            var ahead = Vector2.Dot(offset, heading);
            if (ahead < -tank.HalfWidth)
                return false;
            var lateral = Mathf.Abs(Cross(heading, offset));
            return lateral <= tank.HalfWidth + LaneMargin && ahead / speed <= LookAheadSeconds;
        }

        public static Vector2 SideStep(Vector2 soldier, in TankThreat tank, bool wrongWay, float lengthFactor)
        {
            var heading = tank.Velocity.normalized;
            var offset = soldier - tank.Position;
            var lateral = Cross(heading, offset);
            var side = lateral >= 0f ? 1f : -1f;
            if (wrongWay)
                side = -side;
            var left = new Vector2(-heading.y, heading.x);
            var distanceOut = tank.HalfWidth + LaneMargin + StepClearance - side * lateral;
            return soldier + left * side * distanceOut * lengthFactor;
        }

        public bool TryPlan(Vector2 soldier, IReadOnlyList<TankThreat> tanks, float time, Func<Vector2, bool> isWalkable, out Vector2 step)
        {
            step = soldier;
            var found = false;
            var nearest = default(TankThreat);
            var nearestDistance = float.PositiveInfinity;
            foreach (var tank in tanks)
            {
                if (!IsInLane(soldier, tank))
                    continue;
                var distance = Vector2.Distance(soldier, tank.Position);
                if (distance < nearestDistance)
                {
                    nearest = tank;
                    nearestDistance = distance;
                    found = true;
                }
            }

            if (!found)
            {
                threatened = false;
                reactAt = float.PositiveInfinity;
                return false;
            }

            if (!threatened)
            {
                threatened = true;
                reactAt = time + Mathf.Lerp(MinReaction, MaxReaction, random01());
                wrongWay = random01() < WrongWayChance;
                shortStep = random01() < ShortStepChance;
            }
            if (time < reactAt)
                return false;

            var length = shortStep ? ShortStepFactor : 1f;
            step = SideStep(soldier, nearest, wrongWay, length);
            if (isWalkable != null && !isWalkable(step))
            {
                var other = SideStep(soldier, nearest, !wrongWay, length);
                if (isWalkable(other))
                    step = other;
            }
            return true;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
