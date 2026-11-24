using System.Collections.Generic;
using PolishTanks.Navigation;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Hand-made obstacle list for building <see cref="NavGrid"/>s in tests.</summary>
    internal sealed class BoxObstacleSource : IObstacleSource
    {
        private readonly List<Obstacle> obstacles = new List<Obstacle>();

        public BoxObstacleSource Solid(Rect rect)
        {
            obstacles.Add(Obstacle.Box(rect));
            return this;
        }

        public IEnumerable<Obstacle> Collect(Rect area) => obstacles;

        public NavGrid BuildGrid(Rect area) => NavGridBuilder.Build(area, 1f, this);
    }
}
