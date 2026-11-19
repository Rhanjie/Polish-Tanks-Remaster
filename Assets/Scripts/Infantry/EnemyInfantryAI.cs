using System.Collections.Generic;
using PolishTanks.AI;
using PolishTanks.AI.Behaviours;
using PolishTanks.AI.Orders;
using PolishTanks.Navigation;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// Brain of an enemy soldier, built on the same parts as the tank AI: it sees the player within its profile's view
    /// range (blocked by buildings, wider right after the player fires), remembers and searches like a tank, radios the
    /// player's position to friends in sight range, and follows the same designer orders. In a fight it runs for the
    /// nearest cover (crates, barrels, low walls, the far side of a friendly tank) and fires short bursts from there,
    /// also while running but with much worse aim; with no cover around it closes to rifle range in the open. Its shots
    /// clear the cover it crouches behind. A grenadier runs in and throws an anti-tank
    /// grenade at the side of the tank facing it whenever one is ready. Any tank, friend or foe, driving at the soldier
    /// makes it try to jump aside, unless it is busy firing or throwing at that moment.
    /// </summary>
    [RequireComponent(typeof(InfantryMover), typeof(InfantryHealth))]
    public class EnemyInfantryAI : MonoBehaviour, IAiAgent
    {
        private const float ThreatScanRadius = 14f;
        private const float ThreatScanInterval = 0.1f;
        private const float FacingTolerance = 12f;
        private const float RifleRangeShare = 0.8f;
        private const float MuzzleOffset = 0.45f;
        private const float GrenadeStandOff = 0.4f;
        private const float FireLaneHalfWidth = 0.4f;
        private const float OwnBlastMargin = 0.6f;
        private const float CoverSearchRadius = 12f;
        private const float CoverRefreshInterval = 0.4f;
        private const float RunningSpreadFactor = 3f;
        private const float RifleCoverClearance = 2.5f;

        [SerializeField] private InfantryProfile profile;
        [Tooltip("How this soldier sees, reacts and searches. Empty uses the built-in defaults.")]
        [SerializeField] private CombatDoctrine doctrine;
        [Tooltip("What the soldier does outside combat, in order.")]
        [SerializeField] private OrderList orders = new OrderList();
        [Tooltip("Keep following the orders during a fight (cutscenes). The soldier still shoots at a spotted target.")]
        [SerializeField] private bool scriptedCombat;

        [Inject] private PlayerTank player;
        [Inject] private IPoolService pools;

        private readonly TargetMemory target = new TargetMemory();
        private readonly SightBoost sightBoost = new SightBoost();
        private readonly RadioOperator radio = new RadioOperator();
        private readonly List<TankThreat> threats = new List<TankThreat>();
        private static readonly List<Collider2D> SpotColliders = new List<Collider2D>(8);
        private readonly List<CoverObject> coverObjects = new List<CoverObject>();
        private readonly List<Vector2> takenSpots = new List<Vector2>();
        private Vector2? coverSpot;
        private CoverObject cover;
        private float nextCoverRefresh;
        private GiveWayPlanner giveWay;
        private InfantryMover mover;
        private InfantryHealth health;
        private Rigidbody2D body;
        private InfantryDriver driver;
        private InfantryArms arms;
        private UtilityBrain brain;
        private AiContext context;
        private CombatDoctrine fallbackDoctrine;
        private InfantryProfile fallbackProfile;
        private Vector2? sideStep;
        private float nextThreatScan;

        public string Name => name;
        public Vector2 Position => transform.position;
        public bool IsActive => isActiveAndEnabled && health != null && !health.IsDead;
        public bool IsOnFoot => true;
        public CombatDoctrine Doctrine => doctrine != null ? doctrine : FallbackDoctrine;
        public InfantryProfile Profile => profile != null ? profile : FallbackProfile;
        public TargetMemory Target => target;
        public OrderList Orders => orders;
        public AiBehaviourKind? CurrentBehaviour => brain?.Current?.Kind;
        public bool IsTargetSpotted => context != null && context.IsTargetSpotted;
        public bool ScriptedCombat => scriptedCombat;
        public float NearMissRadius => Doctrine.NearMissRadius;
        public float SightRange => Doctrine.SightRange(Profile.ViewRange, 1f) * sightBoost.Factor(Time.time);
        public IUnitDriver Driver => driver;
        public bool HasGrenades => arms != null && arms.HasGrenades;
        public Vector2? CoverSpot => coverSpot;

        private CombatDoctrine FallbackDoctrine
        {
            get
            {
                if (fallbackDoctrine == null)
                {
                    fallbackDoctrine = ScriptableObject.CreateInstance<CombatDoctrine>();
                    fallbackDoctrine.hideFlags = HideFlags.DontSave;
                }
                return fallbackDoctrine;
            }
        }

        private InfantryProfile FallbackProfile
        {
            get
            {
                if (fallbackProfile == null)
                {
                    fallbackProfile = ScriptableObject.CreateInstance<InfantryProfile>();
                    fallbackProfile.hideFlags = HideFlags.DontSave;
                }
                return fallbackProfile;
            }
        }

        public void SetOrders(IEnumerable<Order> newOrders, bool loop)
        {
            if (brain != null && brain.Current is FollowOrdersBehaviour)
                brain.Reset(context);
            orders.Set(newOrders, loop, new OrderContext(driver, Time.time));
        }

        public void Alert(Vector2 position, AlertSource source, float time) => target.Report(position, time);

        public void HearPlayerFire(GunMount mount, float time)
        {
            var factor = mount == GunMount.Main ? Doctrine.MainGunRevealFactor : Doctrine.MachineGunRevealFactor;
            sightBoost.Trigger(time, factor, Doctrine.FireRevealSeconds);
        }

        private void Awake()
        {
            mover = GetComponent<InfantryMover>();
            health = GetComponent<InfantryHealth>();
            body = GetComponent<Rigidbody2D>();
            driver = new InfantryDriver(mover);
            giveWay = new GiveWayPlanner(() => Random.value);
            arms = new InfantryArms(Profile.Rifle, Profile.Grenades, () => Random.value, Time.time);
            brain = new UtilityBrain(new AiBehaviour[]
            {
                new FollowOrdersBehaviour(),
                new InfantryEngageBehaviour(FightingDistance, () => coverSpot, IsClosingIn),
                new SearchBehaviour(),
                new GiveWayBehaviour(() => sideStep, driver),
            });
            context = new AiContext(driver, orders, target, Doctrine) { ScriptedCombat = scriptedCombat };
        }

        private void Start()
        {
            context.SearchSpace = new WorldSearchSpace(mover.Navigation, mover.AgentRadius, body);
            orders.Prepare(new OrderContext(driver, Time.time));
            PrewarmPools();
        }

        private void OnEnable()
        {
            AiAgents.Register(this);
            health.Wounded += OnWounded;
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            AiAgents.Unregister(this);
            health.Wounded -= OnWounded;
            health.Died -= OnDied;
            brain.Reset(context);
        }

        private void OnDestroy()
        {
            if (fallbackDoctrine != null)
                Destroy(fallbackDoctrine);
            if (fallbackProfile != null)
                Destroy(fallbackProfile);
        }

        private void Update()
        {
            context.Time = Time.time;
            context.DeltaTime = Time.deltaTime;
            context.Doctrine = Doctrine;
            context.Orders = orders;
            context.ScriptedCombat = scriptedCombat;

            Sense();
            if (context.IsTargetSpotted && Doctrine.RadioAllies)
                radio.TryBroadcast(this, target.LastKnownPosition, SightRange, Time.time, AiAgents.All);
            WatchForTanks();
            UpdateCover();
            driver.Running = target.IsKnown;
            brain.Tick(context);
            if (context.IsTargetSpotted && !(brain.Current is GiveWayBehaviour))
            {
                driver.AimAt(target.LastKnownPosition);
                Fight();
            }
            driver.ApplyIdleAim();
        }

        private void Sense()
        {
            var playerTank = player != null ? player.Controller : null;
            if (playerTank == null || playerTank.IsDestroyed)
            {
                target.Clear();
                return;
            }

            Vector2 targetPosition = playerTank.transform.position;
            var distance = Vector2.Distance(Position, targetPosition);
            var visible = Perception.InReach(distance, SightRange, Doctrine.PursuitRangeFactor, target.IsKnown)
                && LineOfSight.IsClear(Position, targetPosition, body, playerTank.GetComponent<Rigidbody2D>());
            target.Observe(visible, targetPosition, Time.time);
            target.Forget(Time.time, Doctrine.MemorySeconds);
        }

        private void WatchForTanks()
        {
            if (Time.time < nextThreatScan)
                return;
            nextThreatScan = Time.time + ThreatScanInterval;
            TankThreatScanner.Scan(Position, ThreatScanRadius, threats);
            var navigation = mover.Navigation;
            var planned = giveWay.TryPlan(Position, threats, Time.time,
                point => navigation == null || navigation.IsWalkable(point, mover.AgentRadius), out var step);
            sideStep = planned && !arms.IsBusy(Time.time) ? step : (Vector2?)null;
        }

        private bool IsClosingIn()
        {
            if (!arms.IsGrenadeReady(Time.time))
                return false;
            return Vector2.Distance(Position, target.LastKnownPosition) <= arms.Grenades.maxRange * 2f;
        }

        private void UpdateCover()
        {
            if (!context.IsTargetSpotted && !target.IsKnown)
            {
                coverSpot = null;
                return;
            }
            if (Time.time < nextCoverRefresh)
                return;
            nextCoverRefresh = Time.time + CoverRefreshInterval;

            var threat = target.LastKnownPosition;
            CoverScanner.Scan(Position, CoverSearchRadius, coverObjects);
            if (coverSpot.HasValue && TryRefreshCurrent(threat))
                return;

            takenSpots.Clear();
            foreach (var agent in AiAgents.All)
                if (!ReferenceEquals(agent, this) && agent is EnemyInfantryAI mate && mate.coverSpot.HasValue)
                    takenSpots.Add(mate.coverSpot.Value);
            var navigation = mover.Navigation;
            coverSpot = CoverFinder.TryFind(Position, threat, coverObjects, takenSpots, CoverSearchRadius,
                point => IsFreeSpot(point, navigation), out var spot, out cover)
                ? spot
                : (Vector2?)null;
        }

        private bool IsFreeSpot(Vector2 point, Navigation.INavigation navigation)
        {
            if (navigation != null && !navigation.IsWalkable(point, mover.AgentRadius))
                return false;
            var filter = new ContactFilter2D { useTriggers = false };
            var count = Physics2D.OverlapCircle(point, mover.AgentRadius, filter, SpotColliders);
            for (var i = 0; i < count; i++)
                if (SpotColliders[i].attachedRigidbody != body)
                    return false;
            return true;
        }

        private bool TryRefreshCurrent(Vector2 threat)
        {
            foreach (var candidate in coverObjects)
            {
                if (Vector2.Distance(candidate.Centre, cover.Centre) > candidate.Radius + 0.5f)
                    continue;
                cover = candidate;
                var spot = CoverFinder.SpotBehind(cover, threat);
                if (CoverFinder.Shields(cover, coverSpot.Value, threat))
                    return true;
                if (!IsFreeSpot(spot, mover.Navigation))
                    break;
                coverSpot = spot;
                return true;
            }
            coverSpot = null;
            return false;
        }

        private float FightingDistance()
        {
            var grenades = arms.Grenades;
            if (arms.IsGrenadeReady(Time.time))
                return Mathf.Max(grenades.minRange, grenades.maxRange - 1f);
            return arms.Rifle.range * RifleRangeShare;
        }

        private void Fight()
        {
            var playerTank = player.Controller;
            var aim = target.LastKnownPosition;
            var toTarget = aim - Position;
            var distance = toTarget.magnitude;
            if (distance <= 0.01f || Vector2.Angle(driver.Forward, toTarget) > FacingTolerance)
                return;

            if (!driver.IsMoving && arms.IsGrenadeReady(Time.time) && distance <= arms.Grenades.maxRange)
            {
                var grenadeAim = GrenadeAim(playerTank.GetComponent<Rigidbody2D>(), aim);
                if (IsSafeToThrow(grenadeAim))
                {
                    ThrowGrenade(grenadeAim);
                    return;
                }
            }
            if (distance <= arms.Rifle.range
                && FireLane.IsClear(Position + toTarget / distance * MuzzleOffset, aim, this, FireLaneHalfWidth, arms.Rifle.spread, AiAgents.All)
                && arms.TryShoot(Time.time))
                FireRifle(arms.Spread(toTarget / distance, driver.IsMoving ? RunningSpreadFactor : 1f));
        }

        private void FireRifle(Vector2 direction)
        {
            var rifle = arms.Rifle;
            var definition = rifle.projectile;
            if (pools == null || definition == null || definition.Prefab == null)
                return;
            var origin = Position + direction * MuzzleOffset;
            var projectile = pools.For(definition.Prefab, definition.PrewarmCount)
                .Get(origin, Quaternion.LookRotation(Vector3.forward, direction));
            projectile.Launch(new ProjectileShot
            {
                Velocity = direction * rifle.projectileSpeed,
                Lifetime = definition.Lifetime,
                Shooter = body,
                Impulse = rifle.impactImpulse,
                Damage = rifle.damage,
                PenetrationMin = rifle.penetrationMin,
                PenetrationMax = rifle.penetrationMax,
                Caliber = rifle.caliber,
                FiredFrom = origin,
                CoverClearance = RifleCoverClearance,
                ImpactPool = EffectPool(definition.ImpactEffect, definition),
                PenetratedPool = EffectPool(definition.PenetratedEffect, definition),
                BlockedPool = EffectPool(definition.BlockedEffect, definition),
                RicochetPool = EffectPool(definition.RicochetEffect, definition),
                OtherRicochetPool = EffectPool(definition.OtherRicochetEffect, definition),
            });
        }

        private PrefabPool<PooledEffect> EffectPool(PooledEffect effect, ProjectileDefinition definition) =>
            effect != null && pools != null ? pools.For(effect, definition.PrewarmCount) : null;

        private Vector2 GrenadeAim(Rigidbody2D tankBody, Vector2 fallback)
        {
            if (tankBody == null)
                return fallback;
            var edge = tankBody.ClosestPoint(Position);
            var outward = Position - edge;
            return edge + (outward.sqrMagnitude > 1e-6f ? outward.normalized * GrenadeStandOff : Vector2.zero);
        }

        private bool IsSafeToThrow(Vector2 aim)
        {
            var grenades = arms.Grenades;
            var distance = Vector2.Distance(Position, aim);
            return distance >= grenades.minRange
                && distance >= grenades.blast.radius + grenades.scatter + mover.AgentRadius + OwnBlastMargin;
        }

        private void ThrowGrenade(Vector2 aim)
        {
            var grenades = arms.Grenades;
            arms.Throw(Time.time);
            driver.Stop();
            if (pools == null || !grenades.IsCarried)
                return;

            var scatterAngle = Random.value * Mathf.PI * 2f;
            var landing = aim + new Vector2(Mathf.Cos(scatterAngle), Mathf.Sin(scatterAngle)) * grenades.scatter * Random.value;
            var flight = Vector2.Distance(Position, landing) * grenades.secondsPerMetre;
            var grenade = pools.For(grenades.prefab, 2).Get(Position, Quaternion.identity);
            grenade.Throw(Position, landing, flight, grenades.fuse, grenades.blast, body, pools);
        }

        private void PrewarmPools()
        {
            if (pools == null)
                return;
            var definition = Profile.Rifle.projectile;
            if (definition != null && definition.Prefab != null)
                pools.For(definition.Prefab, definition.PrewarmCount);
            var grenades = Profile.Grenades;
            if (grenades.IsCarried)
                pools.For(grenades.prefab, 2);
            if (grenades.blast.effect != null)
                pools.For(grenades.blast.effect, 2);
        }

        private void OnWounded(Rigidbody2D shooter)
        {
            var playerTank = player != null ? player.Controller : null;
            if (shooter != null && playerTank != null && shooter.gameObject == playerTank.gameObject)
                Alert(shooter.position, AlertSource.Hit, Time.time);
        }

        private void OnDied(InfantryHealth _)
        {
            brain.Reset(context);
            enabled = false;
        }
    }
}
