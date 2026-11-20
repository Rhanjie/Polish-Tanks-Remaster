using System;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// One objective of a <see cref="MissionDefinition"/>. Scene objects find it by <see cref="Id"/>: a
    /// <see cref="MissionZone"/> for Reach, every <see cref="MissionTarget"/> with the id for Destroy.
    /// Primary objectives are played in list order; with <see cref="StartMarker"/> one waits until the player drives
    /// into its start marker. Optional ones only add respect; they are open from the start, or from the moment the
    /// objective named in <see cref="OpenWithObjectiveId"/> begins, until the end. An Avoid objective fails
    /// when the player's tank touches one of its <see cref="MissionObstacle"/>s and is done once the objective named in
    /// <see cref="UntilObjectiveId"/> is (or the mission is won).
    /// </summary>
    [Serializable]
    public class MissionObjective
    {
        [SerializeField] private string id;
        [SerializeField] private MissionObjectiveKind kind;
        [Tooltip("Player-facing text of the objective.")]
        [SerializeField] private string description;
        [SerializeField] private bool optional;
        [SerializeField, Min(0)] private int respect = 50;
        [Tooltip("Survive: seconds to hold out once the objective starts.")]
        [SerializeField, Min(0f)] private float surviveSeconds = 60f;
        [Tooltip("Destroy: targets to destroy. 0 means every MissionTarget with this id in the scene.")]
        [SerializeField, Min(0)] private int targetCount;
        [Tooltip("Primary: wait for the player to drive into this objective's start marker before it begins.")]
        [SerializeField] private bool startMarker;
        [Tooltip("Avoid: done once this objective is done. Empty means at the end of a won mission.")]
        [SerializeField] private string untilObjectiveId;
        [Tooltip("Optional: opens when this objective begins. Empty means from the start of the mission.")]
        [SerializeField] private string openWithObjectiveId;

        public MissionObjective(string id, MissionObjectiveKind kind, string description, bool optional = false,
            int respect = 50, float surviveSeconds = 60f, int targetCount = 0, bool startMarker = false,
            string untilObjectiveId = null, string openWithObjectiveId = null)
        {
            this.id = id;
            this.kind = kind;
            this.description = description;
            this.optional = optional;
            this.respect = respect;
            this.surviveSeconds = surviveSeconds;
            this.targetCount = targetCount;
            this.startMarker = startMarker;
            this.untilObjectiveId = untilObjectiveId;
            this.openWithObjectiveId = openWithObjectiveId;
        }

        public string Id => id;
        public MissionObjectiveKind Kind => kind;
        public string Description => description;
        public bool Optional => optional;
        public int Respect => respect;
        public float SurviveSeconds => surviveSeconds;
        public int TargetCount => targetCount;
        public bool StartMarker => startMarker;
        public string UntilObjectiveId => untilObjectiveId;
        public string OpenWithObjectiveId => openWithObjectiveId;
    }
}
