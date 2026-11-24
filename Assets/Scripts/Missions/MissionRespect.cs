using System;
using UnityEngine;

namespace PolishTanks.Missions
{
    /// <summary>Respect a mission pays for each enemy loss, on top of the respect of completed objectives.</summary>
    [Serializable]
    public struct MissionRespect
    {
        [Min(0)] public int tankDestroyed;
        [Min(0)] public int moduleDestroyed;
        [Min(0)] public int infantryKilled;

        public int For(EnemyLoss loss) => loss switch
        {
            EnemyLoss.Tank => tankDestroyed,
            EnemyLoss.Module => moduleDestroyed,
            EnemyLoss.Infantry => infantryKilled,
            _ => 0,
        };
    }
}
