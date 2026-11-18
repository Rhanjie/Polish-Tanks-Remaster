using PolishTanks.AI.Orders;
using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// One enemy unit with a brain, tank or soldier, as the rest of the world sees it: where it is, how far it sees,
    /// what it knows about the player and what it is doing. <see cref="AiAgents"/> keeps the live ones so the player's
    /// gunfire, passing shells and radio calls reach them, and the F3 view can draw them.
    /// </summary>
    public interface IAiAgent
    {
        string Name { get; }
        Vector2 Position { get; }
        bool IsActive { get; }
        float SightRange { get; }
        float NearMissRadius { get; }
        TargetMemory Target { get; }
        bool IsTargetSpotted { get; }
        OrderList Orders { get; }
        AiBehaviourKind? CurrentBehaviour { get; }
        bool ScriptedCombat { get; }

        void HearPlayerFire(GunMount mount, float time);
        void Alert(Vector2 position, AlertSource source, float time);
    }
}
