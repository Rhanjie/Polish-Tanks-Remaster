using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// Circle on the ground for one objective. A Start zone begins its objective when the player's tank drives in
    /// while the objective waits for it; a Destination zone completes a Reach objective, primary or optional, that is in
    /// progress. The mission ignores the zone at
    /// any other time, so it can be crossed early. The optional ring shows the circle only while it matters and pulses
    /// so it reads from a distance.
    /// </summary>
    public class MissionZone : MonoBehaviour
    {
        private const int RingSegments = 64;

        [SerializeField] private string objectiveId;
        [SerializeField] private MissionZoneRole role = MissionZoneRole.Destination;
        [SerializeField, Min(0.5f)] private float radius = 6f;
        [Tooltip("Optional ring drawn around the zone while it can be used.")]
        [SerializeField] private LineRenderer ring;
        [SerializeField] private Color color = new Color(1f, 0.82f, 0.2f, 0.9f);
        [SerializeField, Min(0.05f)] private float ringWidth = 0.45f;
        [SerializeField, Min(0.1f)] private float pulsePeriod = 1.4f;

        [Inject] private PlayerTank playerTank;
        [Inject] private IMission mission;

        public string ObjectiveId => objectiveId;
        public MissionZoneRole Role => role;

        private void Awake()
        {
            if (ring == null)
                return;
            ring.loop = true;
            ring.useWorldSpace = false;
            ring.positionCount = RingSegments;
            for (var i = 0; i < RingSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / RingSegments;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }
            ring.enabled = false;
        }

        private void Update()
        {
            var progress = mission != null ? mission.Progress : null;
            var usable = progress != null && progress.Outcome == MissionOutcome.Running && IsUsable(progress);
            ShowRing(usable);
            if (!usable || playerTank == null || playerTank.Controller.IsDestroyed)
                return;

            var offset = (Vector2)(playerTank.Transform.position - transform.position);
            if (offset.sqrMagnitude > radius * radius)
                return;
            if (role == MissionZoneRole.Start)
                progress.Begin(objectiveId);
            else
                progress.Reached(objectiveId);
        }

        private bool IsUsable(MissionProgress progress)
        {
            if (role == MissionZoneRole.Start)
            {
                var current = progress.CurrentPrimary;
                return current != null && current.Objective.Id == objectiveId && current.State == ObjectiveState.Waiting;
            }
            var objectives = progress.Objectives;
            for (var i = 0; i < objectives.Count; i++)
            {
                var status = objectives[i];
                if (status.Objective.Id == objectiveId && status.Objective.Kind == MissionObjectiveKind.Reach
                    && status.State == ObjectiveState.Active)
                    return true;
            }
            return false;
        }

        private void ShowRing(bool visible)
        {
            if (ring == null)
                return;
            ring.enabled = visible;
            if (!visible)
                return;
            var pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.PI * 2f / pulsePeriod);
            ring.widthMultiplier = ringWidth * (0.8f + 0.4f * pulse);
            var shown = color;
            shown.a *= 0.65f + 0.35f * pulse;
            ring.startColor = ring.endColor = shown;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = role == MissionZoneRole.Start ? new Color(1f, 0.82f, 0.2f, 0.8f) : new Color(0.3f, 0.7f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
