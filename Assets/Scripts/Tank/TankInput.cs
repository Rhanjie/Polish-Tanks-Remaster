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

        [Header("Actions (project-wide Input Actions)")]
        [SerializeField] private string moveAction = "Player/Move";
        [SerializeField] private string aimAction = "Player/Aim";
        [SerializeField] private string fireAction = "Player/Attack";

        private InputAction move;
        private InputAction aim;
        private InputAction fire;
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
        }

        private void OnDisable()
        {
            if (movement != null)
                movement.SetInput(0f, 0f);
            if (gun != null)
                gun.SetTrigger(false);
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

            if (gun != null && fire != null)
                gun.SetTrigger(fire.IsPressed());
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
