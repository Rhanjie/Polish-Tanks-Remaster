using System.Collections.Generic;
using PolishTanks.AI;
using PolishTanks.AI.Orders;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>An <see cref="IAiAgent"/> with settable position, activity and foot/tank kind that records the alerts it gets.</summary>
    internal sealed class FakeAiAgent : IAiAgent
    {
        public readonly List<Vector2> Alerts = new List<Vector2>();

        public string Name => "Fake";
        public Vector2 Position { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsOnFoot { get; set; }
        public float SightRange => 0f;
        public float NearMissRadius => 0f;
        public TargetMemory Target { get; } = new TargetMemory();
        public bool IsTargetSpotted => false;
        public OrderList Orders => null;
        public AiBehaviourKind? CurrentBehaviour => null;
        public bool ScriptedCombat => false;

        public void HearPlayerFire(GunMount mount, float time)
        {
        }

        public void Alert(Vector2 position, AlertSource source, float time) => Alerts.Add(position);
    }
}
