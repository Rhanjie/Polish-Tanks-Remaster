using PolishTanks.Damage;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Temporary playtest helper: number keys break the player's own parts so every damage effect can be tried
    /// without an enemy. 1 engine, 2 left track, 3 right track, 4 turret ring, 5 a vision block,
    /// 6 field-repairs everything broken at once. Until the HUD exists it also logs what the field repair crew
    /// is doing. Only the tank driven by <see cref="TankInput"/> listens.
    /// </summary>
    [RequireComponent(typeof(ModuleDamage))]
    public class ModuleDamageDebugKeys : MonoBehaviour
    {
        private ModuleDamage damage;
        private FieldRepair repair;
        private TankInput tankInput;
        private bool wasRepairing;

        private void Awake()
        {
            damage = GetComponent<ModuleDamage>();
            repair = GetComponent<FieldRepair>();
            tankInput = GetComponent<TankInput>();
            if (tankInput == null)
                return;
            damage.PartChanged += OnPartChanged;
            if (repair != null)
                repair.Changed += OnRepairChanged;
        }

        private void OnDestroy()
        {
            if (damage != null)
                damage.PartChanged -= OnPartChanged;
            if (repair != null)
                repair.Changed -= OnRepairChanged;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || tankInput == null || damage.Model == null)
                return;

            if (keyboard.digit1Key.wasPressedThisFrame) Break(VehiclePart.Engine);
            if (keyboard.digit2Key.wasPressedThisFrame) Break(VehiclePart.LeftTrack);
            if (keyboard.digit3Key.wasPressedThisFrame) Break(VehiclePart.RightTrack);
            if (keyboard.digit4Key.wasPressedThisFrame) Break(VehiclePart.TurretRing);
            if (keyboard.digit5Key.wasPressedThisFrame) Break(VehiclePart.VisionBlock);
            if (keyboard.digit6Key.wasPressedThisFrame) RepairAll();
            if (keyboard.rKey.wasPressedThisFrame && repair != null && !repair.IsRepairing && !repair.HasWork)
                Debug.Log($"[Debug] {name}: repair key pressed, nothing destroyed to patch.", this);
        }

        private void OnRepairChanged()
        {
            if (repair.IsRepairing)
                Debug.Log($"[Debug] {name}: repair crew working on {repair.Current} ({repair.RemainingSeconds:F0} s).", this);
            else if (wasRepairing)
                Debug.Log(repair.HasWork
                    ? $"[Debug] {name}: repair called off with the repair key."
                    : $"[Debug] {name}: repair finished.", this);
            wasRepairing = repair.IsRepairing;
        }

        private void OnPartChanged(PartId id, PartCondition condition)
        {
            if (condition == PartCondition.FieldRepaired)
                Debug.Log($"[Debug] {name}: {id} patched to half strength. Visibility {damage.Model.Visibility:P0}.", this);
        }

        private void Break(VehiclePart part)
        {
            var broken = damage.Break(part);
            Debug.Log(broken.HasValue
                ? $"[Debug] {name}: {broken.Value} destroyed. Visibility {damage.Model.Visibility:P0}."
                : $"[Debug] {name}: no intact {part} left.", this);
        }

        private void RepairAll()
        {
            var repaired = 0;
            foreach (var id in damage.Model.Parts)
                if (damage.FieldRepair(id))
                    repaired++;
            Debug.Log($"[Debug] {name}: field-repaired {repaired} parts. Visibility {damage.Model.Visibility:P0}.", this);
        }
    }
}
