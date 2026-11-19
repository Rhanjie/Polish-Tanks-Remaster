using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// The ground a searching unit reads: whether it can stand on a point, and how far it can see from a point in one
    /// direction before a building or wall cuts the view. A fake stands in for it in tests.
    /// </summary>
    public interface ISearchSpace
    {
        bool IsWalkable(Vector2 position);
        float ClearDistance(Vector2 from, Vector2 direction, float maxDistance);
    }
}
