using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>
    /// A mission as data: its title, the ordered list of objectives and the respect paid for enemy losses.
    /// The scene supplies the places and targets through <see cref="MissionZone"/> and <see cref="MissionTarget"/>,
    /// and a <see cref="MissionDirector"/> runs it.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Mission", fileName = "Mission")]
    public class MissionDefinition : ScriptableObject
    {
        [SerializeField] private string title;
        [SerializeField] private List<MissionObjective> objectives = new List<MissionObjective>();
        [SerializeField] private MissionRespect respect = new MissionRespect { tankDestroyed = 100, moduleDestroyed = 10, infantryKilled = 15 };

        public string Title => title;
        public IReadOnlyList<MissionObjective> Objectives => objectives;
        public MissionRespect Respect => respect;
    }
}
