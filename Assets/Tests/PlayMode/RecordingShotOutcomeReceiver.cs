using System.Collections.Generic;
using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Collects every <see cref="ShotOutcome"/> the shooter is told about.</summary>
    internal sealed class RecordingShotOutcomeReceiver : MonoBehaviour, IShotOutcomeReceiver
    {
        public readonly List<ShotOutcome> Outcomes = new List<ShotOutcome>();

        public void OnShotOutcome(in ShotOutcome outcome) => Outcomes.Add(outcome);
    }
}
