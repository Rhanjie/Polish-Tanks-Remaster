using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// Stand-in for a proper workshop: when the player's tank drives into the circle, every damaged or destroyed part is
    /// fixed at once, a fire goes out and the crew's own field repair stops. Works again each time the tank leaves and comes back.
    /// </summary>
    public class RepairZone : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float radius = 4f;

        [Inject] private PlayerTank playerTank;

        private ModuleDamage damage;
        private FieldRepair fieldRepair;
        private bool inside;

        private void Start()
        {
            if (playerTank != null)
            {
                damage = playerTank.Controller.GetComponent<ModuleDamage>();
                fieldRepair = playerTank.Controller.GetComponent<FieldRepair>();
            }
        }

        private void Update()
        {
            if (damage == null || playerTank.Controller.IsDestroyed)
                return;
            var offset = (Vector2)(playerTank.Transform.position - transform.position);
            var nowInside = offset.sqrMagnitude <= radius * radius;
            if (nowInside && !inside)
            {
                if (fieldRepair != null)
                    fieldRepair.Cancel();
                damage.RepairAll();
            }
            inside = nowInside;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 1f, 0.5f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
