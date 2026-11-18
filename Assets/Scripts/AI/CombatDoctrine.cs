using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// How one kind of AI tank sees and fights: sight, memory, reaction and gunnery. One asset per tank type
    /// (light, medium, heavy), shared by every tank of that type; later behaviours read their own settings from here.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/AI/Combat Doctrine", fileName = "Doctrine_")]
    public class CombatDoctrine : ScriptableObject
    {
        [Header("Senses")]
        [Tooltip("Sight distance as a multiple of the optics view range. The player's camera shows about 1.25 up and 2.2 sideways.")]
        [SerializeField, Min(0.1f)] private float sightRangeFactor = 1.8f;
        [Tooltip("Seconds a target out of sight is still hunted before the tank goes back to its orders.")]
        [SerializeField, Min(0f)] private float memorySeconds = 12f;
        [Tooltip("Seconds a target must stay in view before the tank stops and opens fire.")]
        [SerializeField, Min(0f)] private float reactionTime = 0.6f;
        [Tooltip("Drive to where the target was last seen or shot from. Off keeps the tank in place with the turret on that spot.")]
        [SerializeField] private bool searchLostTarget = true;

        [Header("Gunnery")]
        [Tooltip("Largest random aim offset in degrees, picked again after every main gun shot.")]
        [SerializeField, Range(0f, 30f)] private float aimJitter = 6f;
        [Tooltip("How close in degrees the barrel must be to the aim point before the gun fires.")]
        [SerializeField, Min(0.1f)] private float aimTolerance = 2f;
        [Tooltip("Smallest number of seconds between main gun shots, on top of the gun's own reload.")]
        [SerializeField, Min(0f)] private float shotInterval = 5f;
        [Tooltip("Seconds the machine guns fire in each burst. Zero keeps them silent.")]
        [SerializeField, Min(0f)] private float burstSeconds = 1.2f;
        [Tooltip("Seconds between machine-gun bursts.")]
        [SerializeField, Min(0f)] private float burstPause = 3f;

        public float SightRangeFactor => sightRangeFactor;
        public float MemorySeconds => memorySeconds;
        public float ReactionTime => reactionTime;
        public bool SearchLostTarget => searchLostTarget;
        public float AimJitter => aimJitter;
        public float AimTolerance => aimTolerance;
        public float ShotInterval => shotInterval;
        public float BurstSeconds => burstSeconds;
        public float BurstPause => burstPause;

        public float SightRange(float viewRange, float visibility) => viewRange * sightRangeFactor * Mathf.Clamp01(visibility);
    }
}
