using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>Lists the obstacles inside an area for <see cref="NavGridBuilder"/>. Tests feed hand-made boxes.</summary>
    public interface IObstacleSource
    {
        IEnumerable<Obstacle> Collect(Rect area);
    }
}
