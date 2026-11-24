using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// A thrown anti-tank grenade. It flies in an arc to where it was thrown (the sprite grows and spins to show the
    /// height), lies there for its fuse time and then goes off as a <see cref="Blast"/>.
    /// </summary>
    public class Grenade : PooledBehaviour
    {
        [Tooltip("Child holding the grenade sprite; it is scaled and spun in flight.")]
        [SerializeField] private Transform visual;
        [Tooltip("How much bigger the sprite gets at the top of the arc.")]
        [SerializeField, Min(0f)] private float arcScale = 0.8f;
        [Tooltip("Spin in flight, degrees per second.")]
        [SerializeField] private float spin = 540f;

        private Vector2 from;
        private Vector2 to;
        private float flightTime;
        private float fuse;
        private float elapsed;
        private BlastSettings blast;
        private Rigidbody2D thrower;
        private IPoolService pools;
        private Vector3 visualScale = Vector3.one;

        public bool HasLanded => elapsed >= flightTime;
        public Vector2 LandingPoint => to;

        private void Awake()
        {
            if (visual != null)
                visualScale = visual.localScale;
        }

        public void Throw(Vector2 start, Vector2 landing, float seconds, float fuseSeconds, in BlastSettings settings,
            Rigidbody2D source, IPoolService effectPools)
        {
            from = start;
            to = landing;
            flightTime = Mathf.Max(0.05f, seconds);
            fuse = fuseSeconds;
            blast = settings;
            thrower = source;
            pools = effectPools;
            elapsed = 0f;
            transform.position = start;
        }

        protected internal override void OnReturned()
        {
            thrower = null;
            pools = null;
            if (visual != null)
                visual.localScale = visualScale;
        }

        private void Update()
        {
            if (!IsSpawned)
                return;
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / flightTime);
            transform.position = Vector2.Lerp(from, to, progress);
            if (visual != null)
            {
                visual.localScale = visualScale * (1f + arcScale * Mathf.Sin(progress * Mathf.PI));
                if (progress < 1f)
                    visual.Rotate(0f, 0f, spin * Time.deltaTime);
            }

            if (elapsed >= flightTime + fuse)
                Explode();
        }

        private void Explode()
        {
            Blast.Detonate(to, blast, thrower);
            if (blast.effect != null && pools != null)
                pools.For(blast.effect, 1).Get(to, Quaternion.identity);
            ReturnToPool();
        }
    }
}
