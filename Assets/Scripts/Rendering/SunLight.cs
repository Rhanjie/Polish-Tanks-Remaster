using UnityEngine;

namespace PolishTanks.Rendering
{
    /// <summary>
    /// The one sun every ground shadow in the world falls away from, so buildings, tanks, props and trees all cast
    /// their shadows the same way. The direction is in world space and points from an object toward its shadow.
    /// </summary>
    public static class SunLight
    {
        public static readonly Vector2 Direction = new Vector2(0.6f, -0.8f);
    }
}
