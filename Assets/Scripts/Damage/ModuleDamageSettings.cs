using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// Tuning shared by every vehicle for part damage: how fires burn, how a thrown track behaves and how often
    /// shells that stay in the plate still break a track. Per-part break chances come from the fitted modules.
    /// Rounds below <see cref="MinCaliber"/> (rifle-calibre machine guns) never damage parts.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Module Damage Settings", fileName = "ModuleDamageSettings")]
    public class ModuleDamageSettings : ScriptableObject
    {
        [Tooltip("Smallest calibre in mm that can knock out a part. Machine-gun bullets below it only scratch the paint.")]
        [SerializeField, Min(0f)] private float minCaliber = 20f;

        [Tooltip("Track break chance for a side hit that does not penetrate, as a fraction of the module's chance.")]
        [SerializeField, Range(0f, 1f)] private float nonPenetratingTrackFactor = 0.4f;

        [Header("Thrown track")]
        [Tooltip("How hard the dead track drags the hull round when it snaps at speed. Scales with speed and mass.")]
        [SerializeField, Min(0f)] private float trackYank = 0.6f;

        [Tooltip("Forward creep with one track out, as a fraction of top speed. The good track mostly just spins the hull.")]
        [SerializeField, Range(0f, 0.3f)] private float crippledCrawl = 0.05f;

        [Header("Engine fire")]
        [Tooltip("Seconds a fire burns before it goes out by itself.")]
        [SerializeField, Min(0.1f)] private float fireDuration = 12f;

        [Tooltip("Seconds between fire damage ticks.")]
        [SerializeField, Min(0.05f)] private float fireTickInterval = 1.5f;

        [Tooltip("Damage per fire tick as a fraction of maximum hit points.")]
        [SerializeField, Range(0f, 1f)] private float fireTickDamage = 0.03f;

        public float MinCaliber => minCaliber;
        public float NonPenetratingTrackFactor => nonPenetratingTrackFactor;
        public float TrackYank => trackYank;
        public float CrippledCrawl => crippledCrawl;
        public float FireDuration => fireDuration;
        public float FireTickInterval => fireTickInterval;
        public float FireTickDamage => fireTickDamage;
    }
}
