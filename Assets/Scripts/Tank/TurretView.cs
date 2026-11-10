using PolishTanks.Tank.Modules;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>The turret sprite. Takes its look and size from the fitted turret module; the barrel recoil slides it back.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TurretView : MonoBehaviour
    {
        public void Apply(TurretModule turret)
        {
            var sprite = GetComponent<SpriteRenderer>();
            if (turret.Sprite != null)
                sprite.sprite = turret.Sprite;
            transform.localScale = Vector3.one * turret.VisualScale;
        }
    }
}
