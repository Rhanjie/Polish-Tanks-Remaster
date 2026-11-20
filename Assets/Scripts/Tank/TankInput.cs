using System.Collections.Generic;
using PolishTanks.CameraRig;
using PolishTanks.Controls;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Feeds the injected player input into the tank components.
    /// The repair key starts or stops a <see cref="FieldRepair"/>; while it runs the tank stays put and holds fire
    /// (the turret still aims), and only the repair key calls it off.
    /// The machine gun button fires the coaxial gun and, while the cursor lies inside its arc, the hull machine gun too.
    /// Menus <see cref="Hold"/> it while they are open: the tank stops and ignores the player until every holder lets go,
    /// and a fire button still pressed from clicking the menu has to be released before it fires again.
    /// Disable it to drive the tank from code (tests, AI).
    /// </summary>
    [RequireComponent(typeof(TankMovement))]
    public class TankInput : MonoBehaviour
    {
        [Inject] private IPlayerInput input;
        [Inject] private IGameCamera gameCamera;

        private TankMovement movement;
        private TurretAimer aimer;
        private TankGun gun;
        private TankGun coaxialGun;
        private TankGun hullGun;
        private BallMount ballMount;
        private FieldRepair repair;
        private readonly HashSet<object> holders = new HashSet<object>();
        private bool waitForTriggerRelease;

        public bool IsHeld => holders.Count > 0;

        public void Hold(object holder)
        {
            if (holder != null && holders.Add(holder) && holders.Count == 1)
                Stop();
        }

        public void Release(object holder)
        {
            if (holder != null && holders.Remove(holder) && holders.Count == 0)
                waitForTriggerRelease = true;
        }

        private void Awake()
        {
            movement = GetComponent<TankMovement>();
            aimer = GetComponentInChildren<TurretAimer>(true);
            gun = TankGun.Find(gameObject, GunMount.Main);
            coaxialGun = TankGun.Find(gameObject, GunMount.Coaxial);
            hullGun = TankGun.Find(gameObject, GunMount.Hull);
            ballMount = GetComponentInChildren<BallMount>(true);
            repair = GetComponent<FieldRepair>();
        }

        private void OnDisable() => Stop();

        private void Stop()
        {
            if (movement != null)
                movement.SetInput(0f, 0f);
            if (gun != null)
                gun.SetTrigger(false);
            if (coaxialGun != null)
                coaxialGun.SetTrigger(false);
            if (hullGun != null)
                hullGun.SetTrigger(false);
        }

        private void Update()
        {
            if (input == null || IsHeld)
                return;

            if (waitForTriggerRelease && !input.FireHeld && !input.SecondaryFireHeld)
                waitForTriggerRelease = false;

            var repairing = UpdateRepair();
            var canFire = !repairing && !waitForTriggerRelease;

            if (movement != null)
            {
                var value = repairing ? Vector2.zero : input.Move;
                movement.SetInput(value.y, value.x);
            }

            var hullGunCovers = false;
            if (gameCamera != null)
            {
                var aimPoint = gameCamera.ScreenToWorld(input.PointerScreenPosition);
                if (aimer != null)
                    aimer.SetAimPoint(aimPoint);
                if (ballMount != null)
                {
                    ballMount.SetAimPoint(aimPoint);
                    hullGunCovers = ballMount.IsOnTarget;
                }
            }

            UpdateTrigger(gun, canFire && input.FireHeld, canFire && input.FirePressedThisFrame);
            UpdateTrigger(coaxialGun, canFire && input.SecondaryFireHeld, canFire && input.SecondaryFirePressedThisFrame);
            UpdateTrigger(hullGun, canFire && hullGunCovers && input.SecondaryFireHeld,
                canFire && hullGunCovers && input.SecondaryFirePressedThisFrame);
        }

        private bool UpdateRepair()
        {
            if (repair == null)
                return false;

            if (input.RepairPressedThisFrame)
                repair.Toggle();
            return repair.IsRepairing;
        }

        private static void UpdateTrigger(TankGun weapon, bool held, bool pressedThisFrame)
        {
            if (weapon == null)
                return;
            weapon.SetTrigger(held);
            if (pressedThisFrame)
                weapon.TryFire();
        }
    }
}
