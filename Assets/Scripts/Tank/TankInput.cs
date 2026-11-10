using PolishTanks.CameraRig;
using PolishTanks.Controls;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Feeds the injected player input into the tank components.
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

        private void Awake()
        {
            movement = GetComponent<TankMovement>();
            aimer = GetComponentInChildren<TurretAimer>(true);
            gun = TankGun.Find(gameObject, GunMount.Main);
            coaxialGun = TankGun.Find(gameObject, GunMount.Coaxial);
        }

        private void OnDisable()
        {
            if (movement != null)
                movement.SetInput(0f, 0f);
            if (gun != null)
                gun.SetTrigger(false);
            if (coaxialGun != null)
                coaxialGun.SetTrigger(false);
        }

        private void Update()
        {
            if (input == null)
                return;

            if (movement != null)
            {
                var value = input.Move;
                movement.SetInput(value.y, value.x);
            }

            if (aimer != null && gameCamera != null)
                aimer.SetAimPoint(gameCamera.ScreenToWorld(input.PointerScreenPosition));

            UpdateTrigger(gun, input.FireHeld, input.FirePressedThisFrame);
            UpdateTrigger(coaxialGun, input.SecondaryFireHeld, input.SecondaryFirePressedThisFrame);
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
