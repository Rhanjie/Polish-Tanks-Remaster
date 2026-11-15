using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Prototypes
{
    /// <summary>
    /// Temporary sandbox opponent for testing damage on the player's own tank. It stands still and, while it is on
    /// the player's screen, turns its turret toward the player and fires the main gun when it is on target and the
    /// shot interval has passed, plus short machine-gun bursts. Every shell is aimed a little off centre by a fresh random angle, so
    /// hits land on different plates and at different angles.
    /// </summary>
    [RequireComponent(typeof(TankController))]
    public class SentryTank : MonoBehaviour
    {
        [Tooltip("Largest random aim offset in degrees, picked again after every main gun shot.")]
        [SerializeField, Range(0f, 30f)] private float aimJitter = 6f;

        [Tooltip("How close in degrees the barrel must be to the aim point before the gun fires.")]
        [SerializeField, Min(0.1f)] private float aimTolerance = 2f;

        [Tooltip("Smallest number of seconds between main gun shots, on top of the gun's own reload.")]
        [SerializeField, Min(0f)] private float shotInterval = 8f;

        [Tooltip("Seconds the machine gun fires in each burst. Zero keeps it silent.")]
        [SerializeField, Min(0f)] private float burstSeconds = 1.2f;

        [Tooltip("Seconds between machine-gun bursts.")]
        [SerializeField, Min(0f)] private float burstPause = 3f;

        [Inject] private PlayerTank playerTank;

        private TankController controller;
        private TurretAimer aimer;
        private TankGun mainGun;
        private TankGun machineGun;
        private TankGun hullGun;
        private BallMount ballMount;
        private float jitter;
        private float burstClock;
        private float nextShot;

        private void Awake()
        {
            controller = GetComponent<TankController>();
            aimer = GetComponentInChildren<TurretAimer>(true);
            mainGun = TankGun.Find(gameObject, GunMount.Main);
            machineGun = TankGun.Find(gameObject, GunMount.Coaxial);
            hullGun = TankGun.Find(gameObject, GunMount.Hull);
            ballMount = GetComponentInChildren<BallMount>(true);
            PickJitter();
            nextShot = Time.time + Random.Range(0f, shotInterval);
        }

        private void OnDisable() => HoldMachineGun(false);

        private void Update()
        {
            if (controller.IsDestroyed || aimer == null || !PlayerInSight(out var target))
            {
                HoldMachineGun(false);
                return;
            }

            Vector2 origin = aimer.transform.position;
            aimer.SetAimPoint(origin + (Vector2)(Quaternion.Euler(0f, 0f, jitter) * (target - origin)));
            var onTarget = Mathf.Abs(aimer.RemainingAngle) <= aimTolerance;
            if (ballMount != null)
                ballMount.SetAimPoint(target);

            if (onTarget && Time.time >= nextShot && mainGun != null && mainGun.TryFire())
            {
                PickJitter();
                nextShot = Time.time + shotInterval;
            }

            burstClock += Time.deltaTime;
            var cycle = burstSeconds + burstPause;
            HoldMachineGun(onTarget && cycle > 0f && burstClock % cycle < burstSeconds);
        }

        private bool PlayerInSight(out Vector2 target)
        {
            target = default;
            if (playerTank == null || playerTank.Controller == null || playerTank.Controller.IsDestroyed)
                return false;
            target = playerTank.Transform.position;

            var view = Camera.main;
            if (view == null || !view.orthographic)
                return false;
            var offset = (Vector2)(transform.position - view.transform.position);
            var halfHeight = view.orthographicSize;
            return Mathf.Abs(offset.y) <= halfHeight && Mathf.Abs(offset.x) <= halfHeight * view.aspect;
        }

        private void PickJitter() => jitter = Random.Range(-aimJitter, aimJitter);

        private void HoldMachineGun(bool held)
        {
            if (machineGun != null)
                machineGun.SetTrigger(held);
            if (hullGun != null)
                hullGun.SetTrigger(held && ballMount != null && ballMount.CanReach);
        }
    }
}
