using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// How one kind of AI unit sees and fights: sight, memory, reaction, alerts and gunnery. One asset per unit type
    /// (light, medium, heavy tank, infantry), shared by every unit of that type; behaviours read their own settings from here.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/AI/Combat Doctrine", fileName = "Doctrine_")]
    public class CombatDoctrine : ScriptableObject
    {
        [Header("Senses")]
        [Tooltip("Sight distance as a multiple of the optics view range. The player's camera shows about 1.25 up and 2.2 sideways.")]
        [SerializeField, Min(0.1f)] private float sightRangeFactor = 1.8f;
        [Tooltip("Longest a target out of sight is remembered at all. Searching normally ends sooner, after Search Seconds of roaming.")]
        [SerializeField, Min(0f)] private float memorySeconds = 60f;
        [Tooltip("Seconds a target must stay in view before the tank stops and opens fire.")]
        [SerializeField, Min(0f)] private float reactionTime = 0.6f;
        [Tooltip("Drive to where the target was last seen or shot from. Off keeps the tank in place with the turret on that spot.")]
        [SerializeField] private bool searchLostTarget = true;
        [Tooltip("While hunting, a target in a clear line of sight stays seen up to this multiple of the sight range.")]
        [SerializeField, Min(1f)] private float pursuitRangeFactor = 2.5f;

        [Header("Search")]
        [Tooltip("Seconds spent looking down each open direction at the spot the target was lost.")]
        [SerializeField, Min(0.1f)] private float lookSeconds = 1f;
        [Tooltip("Seconds of roaming around the lost spot after looking around, before giving up and going back to the orders.")]
        [SerializeField, Min(0f)] private float searchSeconds = 15f;
        [Tooltip("How far from the lost spot, in metres, the unit roams while searching.")]
        [SerializeField, Min(1f)] private float roamRadius = 14f;

        [Header("Alerts")]
        [Tooltip("Sight range multiplier right after the player fires the main gun.")]
        [SerializeField, Min(1f)] private float mainGunRevealFactor = 1.6f;
        [Tooltip("Sight range multiplier right after the player fires a machine gun.")]
        [SerializeField, Min(1f)] private float machineGunRevealFactor = 1.25f;
        [Tooltip("Seconds the sight range takes to shrink back to normal after a player shot.")]
        [SerializeField, Min(0f)] private float fireRevealSeconds = 4f;
        [Tooltip("A player's shot passing closer than this, in metres, gives away where it came from.")]
        [SerializeField, Min(0f)] private float nearMissRadius = 4f;
        [Tooltip("Radio the player's position to friends within sight range while the player is spotted.")]
        [SerializeField] private bool radioAllies = true;

        [Header("Hull")]
        [Tooltip("Turn the hull towards the target while fighting. Off keeps the hull where it stands (only the turret turns), e.g. for a tutorial target the player should flank.")]
        [SerializeField] private bool turnHullToFight = true;
        [Tooltip("Degrees the hull is turned off the line to the target while fighting, so shells hit the front plate at a slant. 0 faces it head on.")]
        [SerializeField, Range(0f, 45f)] private float hullAngle = 25f;
        [Tooltip("Degrees the hull sways either way around that angle.")]
        [SerializeField, Range(0f, 20f)] private float hullSway = 6f;
        [Tooltip("Seconds for one full sway.")]
        [SerializeField, Min(0.1f)] private float hullSwayPeriod = 2.5f;
        [Tooltip("Shortest and longest seconds before the hull swings over to the other side.")]
        [SerializeField, Min(0.5f)] private float hullSideSwitchMin = 4f;
        [SerializeField, Min(0.5f)] private float hullSideSwitchMax = 9f;

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
        public float PursuitRangeFactor => pursuitRangeFactor;
        public float LookSeconds => lookSeconds;
        public float SearchSeconds => searchSeconds;
        public float RoamRadius => roamRadius;
        public float MainGunRevealFactor => mainGunRevealFactor;
        public float MachineGunRevealFactor => machineGunRevealFactor;
        public float FireRevealSeconds => fireRevealSeconds;
        public float NearMissRadius => nearMissRadius;
        public bool RadioAllies => radioAllies;
        public bool TurnHullToFight => turnHullToFight;
        public float HullAngle => hullAngle;
        public float HullSway => hullSway;
        public float HullSwayPeriod => hullSwayPeriod;
        public float HullSideSwitchMin => hullSideSwitchMin;
        public float HullSideSwitchMax => Mathf.Max(hullSideSwitchMin, hullSideSwitchMax);
        public float AimJitter => aimJitter;
        public float AimTolerance => aimTolerance;
        public float ShotInterval => shotInterval;
        public float BurstSeconds => burstSeconds;
        public float BurstPause => burstPause;

        public float SightRange(float viewRange, float visibility) => viewRange * sightRangeFactor * Mathf.Clamp01(visibility);
    }
}
