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
    public class TankInput : MonoBehaviour
    {
        [SerializeField] private TankMovement movement;
        [SerializeField] private TurretAimer aimer;
        [SerializeField] private TankGun gun;
        [SerializeField] private TankGun coaxialGun;

        [Inject] private IPlayerInput input;
        [Inject] private IGameCamera gameCamera;

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
