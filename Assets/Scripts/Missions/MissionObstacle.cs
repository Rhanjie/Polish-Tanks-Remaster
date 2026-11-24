using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// Marks a prop for an Avoid objective: when the player's tank bumps into it, the objective fails.
    /// Needs a collider on the same object.
    /// </summary>
    public class MissionObstacle : MonoBehaviour
    {
        [SerializeField] private string objectiveId;

        [Inject] private PlayerTank playerTank;
        [Inject] private IMission mission;

        public string ObjectiveId => objectiveId;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (playerTank == null || mission == null || mission.Progress == null)
                return;
            var body = collision.rigidbody;
            if (body != null && body.gameObject == playerTank.Controller.gameObject)
                mission.Progress.Violated(objectiveId);
        }
    }
}
