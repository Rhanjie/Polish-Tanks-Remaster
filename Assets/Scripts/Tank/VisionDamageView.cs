using PolishTanks.Damage;
using PolishTanks.Rendering;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Feeds the vision damage screen effect from the player's tank: eases each vision block's loss and the fire
    /// alarm toward the tank's current damage and publishes them. A random seed picked on wake decides which cells
    /// of the screen belong to which block. Once the tank is knocked out the view clears again. Disabling the component switches the effect off, so the editor never
    /// keeps a darkened view after Play Mode.
    /// </summary>
    public class VisionDamageView : MonoBehaviour
    {
        [Tooltip("How fast a block's cells spoil when it breaks (full loss per second).")]
        [SerializeField, Min(0.01f)] private float worsenRate = 6f;

        [Tooltip("How fast a block's cells clear after a repair (full loss per second).")]
        [SerializeField, Min(0.01f)] private float recoverRate = 1.2f;

        [Inject] private PlayerTank playerTank;

        private readonly VisionImpairment impairment = new VisionImpairment();
        private TankController controller;
        private ModuleDamage damage;
        private float seed;

        private void Awake() => seed = Random.Range(0f, 100f);

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

            VisionDamageState.Publish(seed, impairment);
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
