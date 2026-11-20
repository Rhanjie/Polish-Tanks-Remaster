using System;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// One objective of a <see cref="MissionDefinition"/>. Scene objects find it by <see cref="Id"/>: a
    /// <see cref="MissionZone"/> for Reach, every <see cref="MissionTarget"/> with the id for Destroy.
    /// Primary objectives are played in list order; optional ones stay open the whole mission and only add respect.
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

        public MissionObjective(string id, MissionObjectiveKind kind, string description, bool optional = false,
            int respect = 50, float surviveSeconds = 60f, int targetCount = 0)
        {
            this.id = id;
            this.kind = kind;
            this.description = description;
            this.optional = optional;
            this.respect = respect;
            this.surviveSeconds = surviveSeconds;
            this.targetCount = targetCount;
        }

        public string Id => id;
        public MissionObjectiveKind Kind => kind;
        public string Description => description;
        public bool Optional => optional;
        public int Respect => respect;
        public float SurviveSeconds => surviveSeconds;
        public int TargetCount => targetCount;
    }
}
