using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Props
{
    /// <summary>Briefly brightens a sprite when a projectile hits it, so impacts read at a glance.</summary>
    public class HitFlash : MonoBehaviour, IProjectileHitHandler
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Color flashColor = new Color(1f, 0.85f, 0.6f);
        [SerializeField, Min(0.01f)] private float duration = 0.12f;

        private Color baseColor;
        private float timeLeft;

        public bool IsFlashing => timeLeft > 0f;

        public HitOutcome OnProjectileHit(in ProjectileHit hit)
        {
            if (target == null)
                return HitOutcome.Ignored;
            timeLeft = duration;
            target.color = flashColor;
            return HitOutcome.Ignored;
        }

        private void Awake()
        {
            if (target == null)
                target = GetComponentInChildren<SpriteRenderer>();
            if (target != null)
                baseColor = target.color;
        }

        private void Update()
        {
            if (timeLeft <= 0f || target == null)
                return;
            timeLeft -= Time.deltaTime;
            target.color = Color.Lerp(baseColor, flashColor, Mathf.Clamp01(timeLeft / duration));
        }
    }
}
