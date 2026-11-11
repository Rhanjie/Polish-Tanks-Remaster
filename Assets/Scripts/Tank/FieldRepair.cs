using System;
using PolishTanks.Damage;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Field repair: the crew patches destroyed parts one by one, most urgent first, back to half their normal effect.
    /// Started and stopped from outside (the player's repair key); the driver and gunner keep still while it runs,
    /// which <see cref="TankInput"/> enforces. Stops by itself when nothing destroyed is left or the tank is knocked out.
    /// </summary>
    [RequireComponent(typeof(ModuleDamage), typeof(TankController))]
    public class FieldRepair : MonoBehaviour
    {
        private ModuleDamage damage;
        private TankController controller;
        private FieldRepairCrew crew;
        private PartId? lastPart;

        public event Action Changed;

        public bool IsRepairing => crew != null && crew.IsWorking;
        public PartId? Current => crew != null ? crew.Current : null;
        public float Progress => crew != null ? crew.Progress : 0f;
        public float RemainingSeconds => crew != null ? Mathf.Max(0f, crew.Duration - crew.Elapsed) : 0f;
        public bool HasWork => damage != null && RepairQueue.Next(damage.Model).HasValue;

        public void Toggle()
        {
            if (IsRepairing)
                Cancel();
            else
                Begin();
        }

        public bool Begin()
        {
            if (crew == null || controller.IsDestroyed || IsRepairing)
                return IsRepairing;
            crew.Start(damage.Model);
            NotifyIfChanged();
            return IsRepairing;
        }

        public void Cancel()
        {
            if (!IsRepairing)
                return;
            crew.Stop();
            NotifyIfChanged();
        }

        private void Awake()
        {
            damage = GetComponent<ModuleDamage>();
            controller = GetComponent<TankController>();
            crew = new FieldRepairCrew(part => damage.Settings.RepairSeconds(part));
            controller.Destroyed += OnTankDestroyed;
        }

        private void OnDestroy()
        {
            if (controller != null)
                controller.Destroyed -= OnTankDestroyed;
        }

        private void Update()
        {
            if (!IsRepairing || damage.Model == null)
                return;
            crew.Tick(damage.Model, Time.deltaTime, Patch);
            NotifyIfChanged();
        }

        private void Patch(PartId id) => damage.FieldRepair(id);

        private void OnTankDestroyed(TankController _) => Cancel();

        private void NotifyIfChanged()
        {
            var part = crew.Current;
            if (part == lastPart)
                return;
            lastPart = part;
            Changed?.Invoke();
        }
    }
}
