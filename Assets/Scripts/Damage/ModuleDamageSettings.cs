using UnityEngine;

namespace PolishTanks.Damage
{
    /// <summary>
    /// Tuning shared by every vehicle for part damage: how fires burn, how a thrown track behaves and how often
    /// shells that stay in the plate still break a track. Per-part break chances come from the fitted modules.
    /// Rounds below <see cref="MinCaliber"/> (rifle-calibre machine guns) leave the parts alone, except for the
    /// vision blocks: they can crack one or blind it with a short flash.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Module Damage Settings", fileName = "ModuleDamageSettings")]
    public class ModuleDamageSettings : ScriptableObject
    {
        [Tooltip("Smallest calibre in mm that can knock out a part. Machine-gun bullets below it only threaten the vision blocks.")]
        [SerializeField, Min(0f)] private float minCaliber = 20f;

        [Tooltip("Track break chance for a side hit that does not penetrate, as a fraction of the module's chance.")]
        [SerializeField, Range(0f, 1f)] private float nonPenetratingTrackFactor = 0.4f;

        [Header("Thrown track")]
        [Tooltip("How hard the dead track drags the hull round when it snaps at speed. Scales with speed and mass.")]
        [SerializeField, Min(0f)] private float trackYank = 0.6f;

        [Tooltip("Forward creep with one track out, as a fraction of top speed. The good track mostly just spins the hull.")]
        [SerializeField, Range(0f, 0.3f)] private float crippledCrawl = 0.05f;

        [Header("Damaged track")]
        [Tooltip("How hard a damaged track pulls the hull toward its side, as a fraction of the turn rate at full throttle.")]
        [SerializeField, Range(0f, 0.5f)] private float damagedTrackPull = 0.12f;

        [Header("Machine-gun fire on vision blocks")]
        [Tooltip("Chance that a machine-gun bullet on the front or side damages a vision block one step.")]
        [SerializeField, Range(0f, 1f)] private float smallArmsBlockDamageChance = 0.01f;

        [Tooltip("Chance that a machine-gun bullet on the front or side blinds a vision block for a moment.")]
        [SerializeField, Range(0f, 1f)] private float smallArmsBlindChance = 0.07f;

        [Tooltip("Seconds a blinded vision block stays blind before it starts to clear.")]
        [SerializeField, Min(0f)] private float blindSeconds = 1.2f;

        [Tooltip("How much of a blinded block's view is lost, from 0 (nothing) to 1 (all).")]
        [SerializeField, Range(0f, 1f)] private float blindStrength = 0.85f;

        [Header("Engine fire")]
        [Tooltip("Seconds a fire burns before it goes out by itself.")]
        [SerializeField, Min(0.1f)] private float fireDuration = 12f;

        [Tooltip("Seconds between fire damage ticks.")]
        [SerializeField, Min(0.05f)] private float fireTickInterval = 1.5f;

        [Tooltip("Damage per fire tick as a fraction of maximum hit points.")]
        [SerializeField, Range(0f, 1f)] private float fireTickDamage = 0.03f;

        [Header("Field repair (seconds per part)")]
        [SerializeField, Min(0f)] private float engineRepair = 8f;
        [SerializeField, Min(0f)] private float trackRepair = 6f;
        [SerializeField, Min(0f)] private float turretRingRepair = 5f;
        [SerializeField, Min(0f)] private float visionBlockRepair = 3f;

        public float RepairSeconds(VehiclePart part) => part switch
        {
            VehiclePart.Engine => engineRepair,
            VehiclePart.LeftTrack => trackRepair,
            VehiclePart.RightTrack => trackRepair,
            VehiclePart.TurretRing => turretRingRepair,
            _ => visionBlockRepair,
        };

        public float MinCaliber => minCaliber;
        public float NonPenetratingTrackFactor => nonPenetratingTrackFactor;
        public float TrackYank => trackYank;
        public float DamagedTrackPull => damagedTrackPull;
        public float SmallArmsBlockDamageChance => smallArmsBlockDamageChance;
        public float SmallArmsBlindChance => smallArmsBlindChance;
        public float BlindSeconds => blindSeconds;
        public float BlindStrength => blindStrength;
        public float CrippledCrawl => crippledCrawl;
        public float FireDuration => fireDuration;
        public float FireTickInterval => fireTickInterval;
        public float FireTickDamage => fireTickDamage;
    }
}
