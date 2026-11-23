using System.Collections.Generic;
using PolishTanks.Missions;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary><see cref="IMission"/> around a given <see cref="MissionProgress"/> that remembers the units it was asked to track.</summary>
    public sealed class FakeMission : IMission
    {
        public FakeMission(MissionProgress progress = null)
        {
            Progress = progress;
        }

        public MissionDefinition Definition => null;
        public MissionProgress Progress { get; }
        public List<GameObject> Tracked { get; } = new List<GameObject>();

        public void Track(GameObject unit) => Tracked.Add(unit);
    }
}
