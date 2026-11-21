using PolishTanks.Combat;
using PolishTanks.Missions;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Radio
{
    /// <summary>
    /// Plays a <see cref="RadioSequence"/> once: when the mission starts, when the objective with
    /// <see cref="objectiveId"/> begins or is completed, or when that objective is in progress and the player's shells
    /// have failed to get through the <see cref="target"/>'s armor a few times. Once the player has read the whole
    /// sequence it can open an optional objective.
    /// </summary>
    public class RadioCue : MonoBehaviour
    {
        [SerializeField] private RadioSequence sequence;
        [SerializeField] private RadioCueMoment moment = RadioCueMoment.MissionStart;
        [Tooltip("The objective to listen to; for ShotsBounced, hits count only while it is in progress.")]
        [SerializeField] private string objectiveId;
        [Tooltip("ShotsBounced: what the player shoots at.")]
        [SerializeField] private Damageable target;
        [Tooltip("ShotsBounced: shells that have to bounce off or stop in the armor.")]
        [SerializeField, Min(1)] private int bouncedShots = 3;
        [Tooltip("ShotsBounced: smaller rounds, like machine gun fire, don't count.")]
        [SerializeField, Min(0f)] private float minCaliber = 20f;
        [Tooltip("Optional objective opened once the player has gone through the whole sequence.")]
        [SerializeField] private string thenOpenObjectiveId;

        [Inject] private RadioChannel radio;
        [Inject] private IMission mission;
        [Inject] private PlayerTank playerTank;

        private MissionProgress progress;
        private bool played;
        private int bounced;

        private void Start()
        {
            progress = mission != null ? mission.Progress : null;
            if (moment == RadioCueMoment.MissionStart)
            {
                Play();
                return;
            }
            if (moment == RadioCueMoment.ShotsBounced)
            {
                if (target != null)
                    target.Hit += OnTargetHit;
                return;
            }
            if (progress != null)
                progress.ObjectiveChanged += OnObjectiveChanged;
        }

        private void OnDestroy()
        {
            if (progress != null)
                progress.ObjectiveChanged -= OnObjectiveChanged;
            if (target != null)
                target.Hit -= OnTargetHit;
        }

        private void OnObjectiveChanged(ObjectiveStatus status)
        {
            if (status.Objective.Id != objectiveId)
                return;
            var expected = moment == RadioCueMoment.ObjectiveStarted ? ObjectiveState.Active : ObjectiveState.Completed;
            if (status.State == expected)
                Play();
        }

        private void OnTargetHit(PenetrationResult result, HitOutcome outcome, float damage)
        {
            if (played || outcome == HitOutcome.Penetrated || target.LastCaliber < minCaliber || !IsPlayerShot()
                || !ObjectiveInProgress())
                return;
            if (++bounced >= bouncedShots)
                Play();
        }

        private bool IsPlayerShot() =>
            playerTank != null && target.LastShooter != null && target.LastShooter.gameObject == playerTank.Controller.gameObject;

        private bool ObjectiveInProgress()
        {
            if (progress == null)
                return false;
            var objectives = progress.Objectives;
            for (var i = 0; i < objectives.Count; i++)
                if (objectives[i].Objective.Id == objectiveId && objectives[i].State == ObjectiveState.Active)
                    return true;
            return false;
        }

        private void Play()
        {
            if (played || radio == null)
                return;
            played = true;
            radio.Play(sequence, OpenNextObjective);
        }

        private void OpenNextObjective()
        {
            if (progress != null && !string.IsNullOrEmpty(thenOpenObjectiveId))
                progress.Open(thenOpenObjectiveId);
        }
    }
}
