using PolishTanks.Tank.Modules;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>The hull sprite. Takes its look and size from the fitted hull module.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HullView : MonoBehaviour
    {
        public void Apply(HullModule hull)
        {
            var sprite = GetComponent<SpriteRenderer>();
            if (hull.Sprite != null)
                sprite.sprite = hull.Sprite;
            transform.localScale = Vector3.one * hull.VisualScale;
        }
    }
}
