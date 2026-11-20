using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// Place for a Reach objective: while the player's tank is within <see cref="radius"/>, it reports the objective as
    /// reached. The mission only counts it while that objective is open, so the zone can be crossed early.
    /// </summary>
    public class MissionZone : MonoBehaviour
    {
        [SerializeField] private string objectiveId;
        [SerializeField, Min(0.5f)] private float radius = 6f;

        [Inject] private PlayerTank playerTank;
        [Inject] private IMission mission;

        public string ObjectiveId => objectiveId;

        private void Update()
        {
            if (playerTank == null || mission?.Progress == null || playerTank.Controller.IsDestroyed)
                return;
            var offset = (Vector2)(playerTank.Transform.position - transform.position);
            if (offset.sqrMagnitude <= radius * radius)
                mission.Progress.Reached(objectiveId);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
