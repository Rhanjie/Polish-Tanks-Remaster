using PolishTanks.Damage;
using PolishTanks.Rendering;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Feeds the vision damage screen effect from the player's tank: eases each vision block's loss and the fire
    /// alarm toward the tank's current damage and publishes them with the hull's position and heading. Once the tank
    /// is knocked out the view clears again. Disabling the component switches the effect off, so the editor never
    /// keeps a darkened view after Play Mode.
    /// </summary>
    public class VisionDamageView : MonoBehaviour
    {
        [Tooltip("World units around the tank that stay mostly visible even with every vision block gone.")]
        [SerializeField, Min(0f)] private float clearRadius = 1.6f;

        [Tooltip("How fast a sector spoils when a block breaks (full loss per second).")]
        [SerializeField, Min(0.01f)] private float worsenRate = 6f;

        [Tooltip("How fast a sector clears after a repair (full loss per second).")]
        [SerializeField, Min(0.01f)] private float recoverRate = 1.2f;

        [Inject] private PlayerTank playerTank;

        private readonly VisionImpairment impairment = new VisionImpairment();
        private TankController controller;
        private ModuleDamage damage;

        private void OnDisable() => VisionDamageState.Clear();

        private void LateUpdate()
        {
            if (!Bind())
            {
                VisionDamageState.Clear();
                return;
            }

            if (controller.IsDestroyed)
                impairment.Clear();
            else
                impairment.Aim(damage.Model, damage.IsBurning);
            impairment.Step(Time.deltaTime, worsenRate, recoverRate);

            var hull = controller.transform;
            VisionDamageState.Publish(hull.position, hull.eulerAngles.z, clearRadius, impairment);
        }

        private bool Bind()
        {
            if (controller != null)
                return damage != null;
            if (playerTank == null || playerTank.Controller == null)
                return false;
            controller = playerTank.Controller;
            damage = controller.GetComponent<ModuleDamage>();
            return damage != null;
        }
    }
}
