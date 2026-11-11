using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolishTanks.Controls
{
    /// <summary>
    /// <see cref="IPlayerInput"/> backed by the "Player" map of an Input Actions asset
    /// (Move, Aim, Attack, FireSecondary, Repair).
    /// </summary>
    public sealed class InputSystemPlayerInput : IPlayerInput
    {
        private readonly InputAction move;
        private readonly InputAction aim;
        private readonly InputAction fire;
        private readonly InputAction secondaryFire;
        private readonly InputAction repair;

        public InputSystemPlayerInput(InputActionAsset actions)
        {
            if (actions == null)
                throw new ArgumentNullException(nameof(actions));

            move = actions.FindAction("Player/Move", true);
            aim = actions.FindAction("Player/Aim", true);
            fire = actions.FindAction("Player/Attack", true);
            secondaryFire = actions.FindAction("Player/FireSecondary", true);
            repair = actions.FindAction("Player/Repair", true);
        }

        public Vector2 Move => move.ReadValue<Vector2>();
        public Vector2 PointerScreenPosition => aim.ReadValue<Vector2>();
        public bool FireHeld => fire.IsPressed();
        public bool FirePressedThisFrame => fire.WasPressedThisFrame();
        public bool SecondaryFireHeld => secondaryFire.IsPressed();
        public bool SecondaryFirePressedThisFrame => secondaryFire.WasPressedThisFrame();
        public bool RepairPressedThisFrame => repair.WasPressedThisFrame();
        public string RepairKeyLabel => repair.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
    }
}
