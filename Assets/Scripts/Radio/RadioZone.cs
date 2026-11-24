using PolishTanks.Missions;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Radio
{
    /// <summary>
    /// Circle on the ground that plays a <see cref="RadioSequence"/> once, when the player's tank first drives in. Its
    /// colour comes from its <see cref="RadioZoneKind"/>. It can wait for an objective: then it shows and works only
    /// while that objective waits to start or is in progress. The ring disappears once the message has played or the
    /// mission is over.
    /// </summary>
    public class RadioZone : MonoBehaviour
    {
        [SerializeField] private RadioSequence sequence;
        [SerializeField] private RadioZoneKind kind = RadioZoneKind.Info;
        [SerializeField, Min(0.5f)] private float radius = 3f;
        [Tooltip("Optional. The zone works only while this objective waits to start or is in progress.")]
        [SerializeField] private string objectiveId;
        [Tooltip("Optional ring drawn around the zone while it can be used.")]
        [SerializeField] private LineRenderer ring;
        [SerializeField, Min(0.05f)] private float ringWidth = 0.45f;
        [SerializeField, Min(0.1f)] private float pulsePeriod = 1.4f;

        [Inject] private RadioChannel radio;
        [Inject] private PlayerTank playerTank;
        [Inject] private IMission mission;

        private ZoneRing drawnRing;

        public bool HasPlayed { get; private set; }

        public static Color ColorOf(RadioZoneKind kind)
        {
            switch (kind)
            {
                case RadioZoneKind.Hint:
                    return new Color(1f, 0.82f, 0.2f, 0.9f);
                case RadioZoneKind.Danger:
                    return new Color(0.95f, 0.25f, 0.2f, 0.9f);
                default:
                    return new Color(0.25f, 0.5f, 1f, 0.9f);
            }
        }

        private void Awake() => drawnRing = new ZoneRing(ring, radius, ringWidth, pulsePeriod);

        private void Update()
        {
            var usable = !HasPlayed && IsUsable();
            drawnRing.Show(usable, ColorOf(kind), Time.time);
            if (!usable || radio == null || playerTank == null || playerTank.Controller.IsDestroyed)
                return;

            var offset = (Vector2)(playerTank.Transform.position - transform.position);
            if (offset.sqrMagnitude > radius * radius)
                return;
            HasPlayed = true;
            radio.Play(sequence);
        }

        private bool IsUsable()
        {
            var progress = mission != null ? mission.Progress : null;
            if (progress == null)
                return string.IsNullOrEmpty(objectiveId);
            if (progress.Outcome != MissionOutcome.Running)
                return false;
            if (string.IsNullOrEmpty(objectiveId))
                return true;
            var objectives = progress.Objectives;
            for (var i = 0; i < objectives.Count; i++)
            {
                var status = objectives[i];
                if (status.Objective.Id == objectiveId
                    && (status.State == ObjectiveState.Active || status.State == ObjectiveState.Waiting))
                    return true;
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = ColorOf(kind);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
