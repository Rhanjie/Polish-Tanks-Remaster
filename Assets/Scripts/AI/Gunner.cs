using System;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// The AI gunner, carried over from the old sandbox sentry: lays the turret on the target with a fresh random
    /// offset after every main gun shot (so hits land on different plates and at different angles), fires when the
    /// barrel is on the aim point and the doctrine's shot interval has passed, and fires the machine guns in bursts
    /// while on target. It holds fire while a friend stands in the line of fire. It works whatever the hull is doing,
    /// also during scripted combat.
    /// </summary>
    public sealed class Gunner
    {
        private readonly Func<float> random01;
        private float jitter;
        private float nextShot = float.NegativeInfinity;
        private float burstClock;
        private bool engaged;

        public Gunner(Func<float> random01)
        {
            this.random01 = random01;
        }

        public bool IsEngaged => engaged;
        public float NextShotTime => nextShot;

        public void Engage(AiContext context, ITankDriver driver, Vector2 target, bool laneClear = true)
        {
            var doctrine = context.Doctrine;
            if (!engaged)
            {
                engaged = true;
                burstClock = 0f;
                PickJitter(doctrine);
            }

            var origin = driver.Position;
            driver.AimAt(origin + (Vector2)(Quaternion.Euler(0f, 0f, jitter) * (target - origin)));
            var onTarget = laneClear && Mathf.Abs(driver.AimError) <= doctrine.AimTolerance;

            if (onTarget && context.Time >= nextShot && driver.FireMainGun())
            {
                PickJitter(doctrine);
                nextShot = context.Time + doctrine.ShotInterval;
            }

            burstClock += context.DeltaTime;
            var cycle = doctrine.BurstSeconds + doctrine.BurstPause;
            driver.SetMachineGuns(onTarget && cycle > 0f && burstClock % cycle < doctrine.BurstSeconds, target);
        }

        public void CeaseFire(ITankDriver driver)
        {
            engaged = false;
            driver.SetMachineGuns(false, driver.Position);
        }

        private void PickJitter(CombatDoctrine doctrine) => jitter = (random01() * 2f - 1f) * doctrine.AimJitter;
    }
}
