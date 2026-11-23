using PolishTanks.Combat;
using PolishTanks.Infantry;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// Marks a tank, soldier or prop as a target of a Destroy objective. When its <see cref="Damageable"/> or
    /// <see cref="InfantryHealth"/> dies it reports the objective once.
    /// </summary>
    public class MissionTarget : MonoBehaviour
    {
        [SerializeField] private string objectiveId;

        [Inject] private IMission mission;

        private Damageable damageable;
        private InfantryHealth soldier;
        private bool reported;

        public string ObjectiveId => objectiveId;

        public void Assign(string objective) => objectiveId = objective;

        private void Awake()
        {
            damageable = GetComponent<Damageable>();
            soldier = GetComponent<InfantryHealth>();
            if (damageable != null)
                damageable.Died += OnDamageableDied;
            if (soldier != null)
                soldier.Died += OnSoldierDied;
        }

        private void OnDestroy()
        {
            if (damageable != null)
                damageable.Died -= OnDamageableDied;
            if (soldier != null)
                soldier.Died -= OnSoldierDied;
        }

        private void OnDamageableDied(Damageable _) => Report();

        private void OnSoldierDied(InfantryHealth _) => Report();

        private void Report()
        {
            if (reported)
                return;
            reported = true;
            mission?.Progress?.TargetDestroyed(objectiveId);
        }
    }
}
