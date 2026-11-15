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

        private void OnDisable()
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
            if (input == null)
                return;

            var repairing = UpdateRepair();

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

            UpdateTrigger(gun, !repairing && input.FireHeld, !repairing && input.FirePressedThisFrame);
            UpdateTrigger(coaxialGun, !repairing && input.SecondaryFireHeld, !repairing && input.SecondaryFirePressedThisFrame);
            UpdateTrigger(hullGun, !repairing && hullGunCovers && input.SecondaryFireHeld,
                !repairing && hullGunCovers && input.SecondaryFirePressedThisFrame);
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
