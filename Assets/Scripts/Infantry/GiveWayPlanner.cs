using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// Decides when a soldier has to get out of a tank's way, friend or foe, and where to step. A moving tank is a
    /// danger when the soldier stands in its lane (hull width plus a margin) close enough ahead to be reached within
    /// <see cref="LookAheadSeconds"/>, or simply stands right next to its front. The soldier then sidesteps across the
    /// lane to the nearer edge, onto walkable ground, and waits on that side until the tank has gone by (a wider lane
    /// counts while it waits, so it doesn't walk straight back in front of the tracks). It is a
    /// person, not a machine: it notices the tank only after a random reaction time, now and then picks the wrong side
    /// (across the tank's path), and sometimes steps too short.
    /// </summary>
    public sealed class GiveWayPlanner
    {
        public const float LookAheadSeconds = 3f;
        public const float MinThreatSpeed = 0.4f;
        public const float LaneMargin = 1.2f;
        public const float CloseRange = 2f;
        public const float WaitingMargin = 3f;
        public const float StepClearance = 0.8f;
        public const float MinReaction = 0.1f;
        public const float MaxReaction = 0.5f;
        public const float WrongWayChance = 0.2f;
        public const float ShortStepChance = 0.2f;
        public const float ShortStepFactor = 0.55f;

        private readonly Func<float> random01;
        private float reactAt = float.PositiveInfinity;
        private float side;
        private bool shortStep;
        private bool threatened;

        public GiveWayPlanner(Func<float> random01)
        {
            this.random01 = random01;
        }

        public bool IsThreatened => threatened;

        public static bool IsInLane(Vector2 soldier, in TankThreat tank) => IsInLane(soldier, tank, LaneMargin);

        public static bool IsInLane(Vector2 soldier, in TankThreat tank, float margin)
        {
            var speed = tank.Velocity.magnitude;
            if (speed < MinThreatSpeed)
                return false;
            var heading = tank.Velocity / speed;
            var offset = soldier - tank.Position;
            var ahead = Vector2.Dot(offset, heading);
            if (ahead < -tank.HalfWidth)
                return false;
            if (offset.magnitude <= tank.HalfWidth + CloseRange)
                return true;
            var lateral = Mathf.Abs(Cross(heading, offset));
            return lateral <= tank.HalfWidth + margin && ahead / speed <= LookAheadSeconds;
        }

        public static float NearerSide(Vector2 soldier, in TankThreat tank)
        {
            var lateral = Cross(tank.Velocity.normalized, soldier - tank.Position);
            return lateral >= 0f ? 1f : -1f;
        }

        public static Vector2 SideStep(Vector2 soldier, in TankThreat tank, float side, float lengthFactor)
        {
            var heading = tank.Velocity.normalized;
            var lateral = Cross(heading, soldier - tank.Position);
            var left = new Vector2(-heading.y, heading.x);
            var distanceOut = tank.HalfWidth + LaneMargin + StepClearance - side * lateral;
            return distanceOut <= 0f ? soldier : soldier + left * side * distanceOut * lengthFactor;
        }

        public bool TryPlan(Vector2 soldier, IReadOnlyList<TankThreat> tanks, float time, Func<Vector2, bool> isWalkable, out Vector2 step)
        {
            step = soldier;
            if (!TryFindNearestThreat(soldier, tanks, threatened ? WaitingMargin : LaneMargin, out var tank))
            {
                threatened = false;
                reactAt = float.PositiveInfinity;
                return false;
            }

            if (!threatened)
            {
                threatened = true;
                reactAt = time + Mathf.Lerp(MinReaction, MaxReaction, random01());
                side = NearerSide(soldier, tank);
                if (random01() < WrongWayChance)
                    side = -side;
                shortStep = random01() < ShortStepChance;
                if (isWalkable != null && !isWalkable(SideStep(soldier, tank, side, 1f)) && isWalkable(SideStep(soldier, tank, -side, 1f)))
                    side = -side;
            }
            if (time < reactAt)
                return false;

            step = SideStep(soldier, tank, side, shortStep ? ShortStepFactor : 1f);
            return true;
        }

        private static bool TryFindNearestThreat(Vector2 soldier, IReadOnlyList<TankThreat> tanks, float margin, out TankThreat nearest)
        {
            nearest = default;
            var found = false;
            var nearestDistance = float.PositiveInfinity;
            foreach (var tank in tanks)
            {
                if (!IsInLane(soldier, tank, margin))
                    continue;
                var distance = Vector2.Distance(soldier, tank.Position);
                if (distance < nearestDistance)
                {
                    nearest = tank;
                    nearestDistance = distance;
                    found = true;
                }
            }
            return found;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
