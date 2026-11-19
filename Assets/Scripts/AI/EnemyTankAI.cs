using System.Collections.Generic;
using PolishTanks.AI.Behaviours;
using PolishTanks.AI.Orders;
using PolishTanks.Combat;
using PolishTanks.Navigation;
using PolishTanks.Tank;
using PolishTanks.Tank.Modules;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// Brain of an enemy tank. Every frame it looks for the player (sight range from the optics and the doctrine, cut by
    /// damaged vision blocks, blocked by buildings), lets the <see cref="UtilityBrain"/> pick what the hull does
    /// (follow the designer's orders, engage, search) and lets the <see cref="Gunner"/> fight whenever the player is
    /// spotted. Once hunting, the crew keeps the player in view along a clear line well past the sight range, and a lost
    /// player is searched for in phases (see <see cref="SearchBehaviour"/>). A shell from the player tells the crew where it came from even when they can't see the shooter, and so
    /// does a player's shot flying close past; the player's gunfire makes the crew see further for a moment. While the
    /// player is spotted the crew radios its position to friends within sight range. It eases off and steers away from
    /// its own soldiers in its lane (<see cref="InfantryCaution"/>).
    /// With Scripted Combat on, the orders keep the hull during a fight (for cutscenes) and only the guns react.
    /// Orders are edited in the inspector or set from code with <see cref="SetOrders"/>. The tank must not have an
    /// enabled <see cref="TankInput"/>. How its main gun shells did goes into <see cref="Penetrations"/> for later tactics.
    /// </summary>
    [RequireComponent(typeof(TankController), typeof(TankPathFollower))]
    public class EnemyTankAI : MonoBehaviour, IShotOutcomeReceiver, IAiAgent
    {
        public const float MinimumShellCaliber = 20f;
        public const float FireLaneHalfWidth = 0.6f;
        public const float FireLaneSpread = 3f;

        [Tooltip("How this tank sees and fights. Empty uses the built-in defaults.")]
        [SerializeField] private CombatDoctrine doctrine;

        [Tooltip("What the tank does outside combat, in order.")]
        [SerializeField] private OrderList orders = new OrderList();

        [Tooltip("Keep following the orders during a fight (cutscenes). The guns still fire at a spotted target.")]
        [SerializeField] private bool scriptedCombat;

        [Inject] private PlayerTank player;
        [Inject] private INavigation navigation;

        private readonly TargetMemory target = new TargetMemory();
        private readonly PenetrationLog penetrations = new PenetrationLog();
        private readonly SightBoost sightBoost = new SightBoost();
        private readonly RadioOperator radio = new RadioOperator();
        private TankController controller;
        private Rigidbody2D body;
        private ModuleDamage moduleDamage;
        private Damageable damageable;
        private TankDriver driver;
        private TankPathFollower follower;
        private UtilityBrain brain;
        private Gunner gunner;
        private AiContext context;
        private CombatDoctrine fallbackDoctrine;
        private bool knockedOut;

        public CombatDoctrine Doctrine => doctrine != null ? doctrine : FallbackDoctrine;
        public OrderList Orders => orders;
        public TargetMemory Target => target;
        public PenetrationLog Penetrations => penetrations;
        public AiBehaviourKind? CurrentBehaviour => brain?.Current?.Kind;
        public bool IsTargetSpotted => context != null && context.IsTargetSpotted;
        public ITankDriver Driver => driver;
        public string Name => name;
        public Vector2 Position => transform.position;
        public bool IsActive => isActiveAndEnabled && !knockedOut;
        public bool IsOnFoot => false;
        public float NearMissRadius => Doctrine.NearMissRadius;

        public bool ScriptedCombat
        {
            get => scriptedCombat;
            set
            {
                scriptedCombat = value;
                if (context != null)
                    context.ScriptedCombat = value;
            }
        }

        public float SightRange
        {
            get
            {
                var stats = controller != null ? controller.Stats : null;
                var viewRange = stats != null ? stats.ViewRange : TankStatsCalculator.DefaultViewRange;
                var visibility = moduleDamage != null && moduleDamage.Model != null ? moduleDamage.Visibility : 1f;
                return Doctrine.SightRange(viewRange, visibility) * sightBoost.Factor(Time.time);
            }
        }

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

        public void SetOrders(IEnumerable<Order> newOrders, bool loop)
        {
            var wasFollowing = brain != null && brain.Current is FollowOrdersBehaviour;
            if (wasFollowing)
                brain.Reset(context);
            orders.Set(newOrders, loop, new OrderContext(driver, Time.time));
        }

        public void AddOrder(Order order) => orders.Add(order);

        public void Alert(Vector2 position, AlertSource source, float time) => target.Report(position, time);

        public void HearPlayerFire(GunMount mount, float time)
        {
            var factor = mount == GunMount.Main ? Doctrine.MainGunRevealFactor : Doctrine.MachineGunRevealFactor;
            sightBoost.Trigger(time, factor, Doctrine.FireRevealSeconds);
        }

        private void Awake()
        {
            controller = GetComponent<TankController>();
            body = GetComponent<Rigidbody2D>();
            moduleDamage = GetComponent<ModuleDamage>();
            damageable = GetComponent<Damageable>();
            follower = GetComponent<TankPathFollower>();
            driver = new TankDriver(gameObject, follower);
            gunner = new Gunner(() => Random.value);
            brain = new UtilityBrain(new AiBehaviour[]
            {
                new FollowOrdersBehaviour(),
                new EngageBehaviour(),
                new SearchBehaviour(),
            });
            context = new AiContext(driver, orders, target, Doctrine) { ScriptedCombat = scriptedCombat };

            var input = GetComponent<TankInput>();
            if (input != null && input.enabled)
                Debug.LogWarning($"{name}: EnemyTankAI and an enabled TankInput both drive this tank.", this);
        }

        private void Start()
        {
            context.SearchSpace = new WorldSearchSpace(follower.Navigation != null ? follower.Navigation : navigation, follower.AgentRadius, body);
            orders.Prepare(new OrderContext(driver, Time.time));
        }

        private void OnEnable()
        {
            AiAgents.Register(this);
            if (damageable != null)
                damageable.Hit += OnHit;
        }

        private void OnDisable()
        {
            AiAgents.Unregister(this);
            if (damageable != null)
                damageable.Hit -= OnHit;
            brain.Reset(context);
            gunner.CeaseFire(driver);
        }

        private void OnDestroy()
        {
            if (fallbackDoctrine != null)
                Destroy(fallbackDoctrine);
        }

        private void Update()
        {
            if (controller.IsDestroyed)
            {
                KnockOut();
                return;
            }

            context.Time = Time.time;
            context.DeltaTime = Time.deltaTime;
            context.Doctrine = Doctrine;
            context.Orders = orders;

            Sense();
            if (context.IsTargetSpotted && Doctrine.RadioAllies)
                radio.TryBroadcast(this, target.LastKnownPosition, SightRange, Time.time, AiAgents.All);
            brain.Tick(context);
            if (context.IsTargetSpotted)
                gunner.Engage(context, driver, target.LastKnownPosition,
                    FireLane.IsClear(driver.Position, target.LastKnownPosition, this, FireLaneHalfWidth, FireLaneSpread, AiAgents.All));
            else if (gunner.IsEngaged)
                gunner.CeaseFire(driver);
            InfantryCaution.Yield(driver.Position, driver.Forward, AiAgents.All, out var speedLimit, out var steerBias);
            follower.SpeedLimit = speedLimit;
            follower.SteerBias = steerBias;
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
            var distance = Vector2.Distance(driver.Position, targetPosition);
            var visible = Perception.InReach(distance, SightRange, Doctrine.PursuitRangeFactor, target.IsKnown)
                && LineOfSight.IsClear(driver.Position, targetPosition, body, playerTank.GetComponent<Rigidbody2D>());
            target.Observe(visible, targetPosition, Time.time);
            target.Forget(Time.time, Doctrine.MemorySeconds);
        }

        private void OnHit(PenetrationResult result, HitOutcome outcome, float damage)
        {
            var shooter = damageable.LastShooter;
            var playerTank = player != null ? player.Controller : null;
            if (shooter != null && playerTank != null && shooter.gameObject == playerTank.gameObject)
                Alert(shooter.position, AlertSource.Hit, Time.time);
        }

        public void OnShotOutcome(in ShotOutcome outcome)
        {
            if (outcome.Outcome == HitOutcome.Ignored || outcome.Caliber < MinimumShellCaliber || outcome.Target == null
                || player == null || player.Controller == null)
                return;
            var hitTank = outcome.Target.GetComponentInParent<TankController>();
            if (hitTank != player.Controller || !hitTank.TryGetComponent<Damageable>(out var armor)
                || armor.IsDead || armor.LastOutcome != outcome.Outcome)
                return;
            penetrations.Record(armor.LastResult.Side, outcome.Outcome == HitOutcome.Penetrated);
        }

        private void KnockOut()
        {
            if (knockedOut)
                return;
            knockedOut = true;
            brain.Reset(context);
            gunner.CeaseFire(driver);
            enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.6f);
            if (Application.isPlaying)
                DrawCircle(transform.position, SightRange);
            else if (doctrine != null)
                DrawCircle(transform.position, doctrine.SightRange(TankStatsCalculator.DefaultViewRange, 1f));

            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            Vector2 previous = transform.position;
            foreach (var order in orders.Orders)
            {
                switch (order)
                {
                    case MoveToOrder move:
                        Gizmos.DrawLine(previous, move.Destination);
                        Gizmos.DrawWireSphere(move.Destination, 0.6f);
                        previous = move.Destination;
                        break;
                    case PatrolOrder patrol:
                        for (var i = 0; i < patrol.Points.Count; i++)
                        {
                            Gizmos.DrawLine(previous, patrol.Points[i]);
                            Gizmos.DrawWireSphere(patrol.Points[i], 0.6f);
                            previous = patrol.Points[i];
                        }
                        break;
                    case WatchOrder watch:
                        Gizmos.DrawLine(previous, watch.Point);
                        Gizmos.DrawWireCube(watch.Point, Vector3.one * 0.6f);
                        break;
                }
            }
        }

        private static void DrawCircle(Vector2 centre, float radius)
        {
            const int Segments = 48;
            var previous = centre + Vector2.right * radius;
            for (var i = 1; i <= Segments; i++)
            {
                var angle = i * Mathf.PI * 2f / Segments;
                var next = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
