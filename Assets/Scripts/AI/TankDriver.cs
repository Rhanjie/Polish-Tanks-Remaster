using PolishTanks.Navigation;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// <see cref="ITankDriver"/> over a real tank: the hull drives through <see cref="TankPathFollower"/>, the turret
    /// through <see cref="TurretAimer"/>, and the main gun, coaxial gun and hull machine gun through <see cref="TankGun"/>.
    /// <see cref="ApplyIdleAim"/> runs at the end of the AI's frame and points the turret at the watch point (or straight
    /// ahead) when nothing aimed it that frame. The hull machine gun sits too low to fire over crates and barrels, so it
    /// holds fire while one is in its line.
    /// </summary>
    public sealed class TankDriver : ITankDriver
    {
        private const float AheadDistance = 10f;

        private readonly Transform hull;
        private readonly TankPathFollower follower;
        private readonly TurretAimer aimer;
        private readonly TankGun mainGun;
        private readonly TankGun coaxialGun;
        private readonly TankGun hullGun;
        private readonly BallMount ballMount;
        private readonly Rigidbody2D body;
        private Vector2? watchPoint;
        private bool aimedThisFrame;
        private bool pathFailed;

        public TankDriver(GameObject tank, TankPathFollower follower)
        {
            hull = tank.transform;
            this.follower = follower;
            aimer = tank.GetComponentInChildren<TurretAimer>(true);
            mainGun = TankGun.Find(tank, GunMount.Main);
            coaxialGun = TankGun.Find(tank, GunMount.Coaxial);
            hullGun = TankGun.Find(tank, GunMount.Hull);
            ballMount = tank.GetComponentInChildren<BallMount>(true);
            body = tank.GetComponent<Rigidbody2D>();
            follower.PathFailed += _ => pathFailed = true;
        }

        public Vector2 Position => hull.position;
        public Vector2 Forward => hull.up;
        public bool IsMoving => follower.IsMoving;
        public bool HasArrived => follower.HasArrived;
        public bool PathFailed => pathFailed;
        public float AimError => aimer != null ? aimer.RemainingAngle : 0f;
        public Vector2? WatchPoint => watchPoint;

        public bool MoveTo(Vector2 destination)
        {
            pathFailed = false;
            return follower.MoveTo(destination);
        }

        public void Stop() => follower.Stop();

        public void AimAt(Vector2 point)
        {
            aimedThisFrame = true;
            if (aimer != null)
                aimer.SetAimPoint(point);
        }

        public void Watch(Vector2? point) => watchPoint = point;

        public bool FireMainGun() => mainGun != null && mainGun.TryFire();

        public void SetMachineGuns(bool held, Vector2 target)
        {
            if (ballMount != null)
                ballMount.SetAimPoint(target);
            if (coaxialGun != null)
                coaxialGun.SetTrigger(held);
            if (hullGun != null)
                hullGun.SetTrigger(held && ballMount != null && ballMount.IsOnTarget
                    && !LowCover.IsInLine(hullGun.Muzzle.position, target, body));
        }

        public void ApplyIdleAim()
        {
            if (!aimedThisFrame && aimer != null)
                aimer.SetAimPoint(watchPoint ?? Position + Forward * AheadDistance);
            aimedThisFrame = false;
        }
    }
}
