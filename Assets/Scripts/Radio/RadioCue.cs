using PolishTanks.Missions;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Radio
{
    /// <summary>
    /// Plays a <see cref="RadioSequence"/> once: when the mission starts, or when the objective with
    /// <see cref="objectiveId"/> begins or is completed.
    /// </summary>
    public class RadioCue : MonoBehaviour
    {
        [SerializeField] private RadioSequence sequence;
        [SerializeField] private RadioCueMoment moment = RadioCueMoment.MissionStart;
        [Tooltip("ObjectiveStarted and ObjectiveCompleted: the objective to listen to.")]
        [SerializeField] private string objectiveId;

        [Inject] private RadioChannel radio;
        [Inject] private IMission mission;

        private MissionProgress progress;
        private bool played;

        private void Start()
        {
            if (moment == RadioCueMoment.MissionStart)
            {
                Play();
                return;
            }
            progress = mission != null ? mission.Progress : null;
            if (progress != null)
                progress.ObjectiveChanged += OnObjectiveChanged;
        }

        private void OnDestroy()
        {
            if (progress != null)
                progress.ObjectiveChanged -= OnObjectiveChanged;
        }

        private void OnObjectiveChanged(ObjectiveStatus status)
        {
            if (status.Objective.Id != objectiveId)
                return;
            var expected = moment == RadioCueMoment.ObjectiveStarted ? ObjectiveState.Active : ObjectiveState.Completed;
            if (status.State == expected)
                Play();
        }

        private void Play()
        {
            if (played || radio == null)
                return;
            played = true;
            radio.Play(sequence);
        }
    }
}
