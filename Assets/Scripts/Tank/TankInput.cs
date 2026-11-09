using UnityEngine;
using UnityEngine.InputSystem;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Feeds player input from the project-wide Input Actions into the tank components.
    /// Disable it to drive the tank from code (tests, AI).
    /// </summary>
    public class TankInput : MonoBehaviour
    {
        [SerializeField] private TankMovement movement;
        [SerializeField] private TurretAimer aimer;
        [SerializeField] private TankGun gun;
        [SerializeField] private TankGun coaxialGun;

        [Header("Actions (project-wide Input Actions)")]
        [SerializeField] private string moveAction = "Player/Move";
        [SerializeField] private string aimAction = "Player/Aim";
        [SerializeField] private string fireAction = "Player/Attack";
        [SerializeField] private string secondaryFireAction = "Player/FireSecondary";

        private InputAction move;
        private InputAction aim;
        private InputAction fire;
        private InputAction secondaryFire;
        private Camera cachedCamera;

        private void OnEnable()
        {
            var actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogWarning($"{nameof(TankInput)}: no project-wide Input Actions asset is set.", this);
                return;
            }

            move = actions.FindAction(moveAction);
            aim = actions.FindAction(aimAction);
            fire = actions.FindAction(fireAction);
            secondaryFire = actions.FindAction(secondaryFireAction);
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
            if (movement != null && move != null)
            {
                var value = move.ReadValue<Vector2>();
                movement.SetInput(value.y, value.x);
            }

            if (aimer != null && aim != null && TryGetCamera(out var cam))
            {
                Vector3 screen = aim.ReadValue<Vector2>();
                screen.z = -cam.transform.position.z;
                aimer.SetAimPoint(cam.ScreenToWorldPoint(screen));
            }

            UpdateTrigger(gun, fire);
            UpdateTrigger(coaxialGun, secondaryFire);
        }

        private static void UpdateTrigger(TankGun weapon, InputAction action)
        {
            if (weapon == null || action == null)
                return;
            weapon.SetTrigger(action.IsPressed());
            // A click pressed and released within one frame never shows up in IsPressed.
            if (action.WasPressedThisFrame())
                weapon.TryFire();
        }

        private bool TryGetCamera(out Camera cam)
        {
            if (cachedCamera == null || !cachedCamera.isActiveAndEnabled)
                cachedCamera = Camera.main;
            cam = cachedCamera;
            return cam != null;
        }
    }
}
