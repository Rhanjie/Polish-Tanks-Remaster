using System;
using System.Collections;
using PolishTanks.AI;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// A soldier's body. Bullets and shells wound it through <see cref="WoundModel"/> and stop in it; a blast hurts it by
    /// distance; a stray ricochet from a friendly AI gun flies past instead (an enemy squad around the player's tank
    /// would otherwise cut itself down with bullets glancing off its armor); a tank driving into it faster than <see cref="crushSpeed"/> runs it over (a slow one only shoves it).
    /// On death the colliders go at once, blood bursts out and the body sinks into the ground and fades away.
    /// </summary>
    public class InfantryHealth : MonoBehaviour, IProjectileHitHandler, IBlastReceiver
    {
        [Tooltip("Speed in m/s a tank must drive into the soldier to run it over.")]
        [SerializeField, Min(0f)] private float crushSpeed = 2f;
        [SerializeField] private PooledEffect bloodEffect;
        [Tooltip("Seconds the body takes to sink and fade out.")]
        [SerializeField, Min(0.1f)] private float sinkSeconds = 3f;
        [Tooltip("Scale the body shrinks to while it sinks.")]
        [SerializeField, Range(0.1f, 1f)] private float sunkScale = 0.75f;

        [Inject] private IPoolService pools;

        private readonly WoundModel wounds = new WoundModel();

        public event Action<Rigidbody2D> Wounded;
        public event Action<InfantryHealth> Died;

        public bool IsDead => wounds.IsDead;
        public float Health => wounds.Health;
        public float CrushSpeed => crushSpeed;

        public HitResponse OnProjectileHit(in ProjectileHit hit)
        {
            if (IsDead || IsFriendlyRicochet(hit))
                return HitResponse.Ignored;
            var killed = wounds.TakeHit(hit.Caliber, Random.value);
            Wounded?.Invoke(hit.Shooter);
            if (killed)
                Die(-hit.Direction);
            return new HitResponse(HitOutcome.Penetrated, -hit.Direction, SurfaceMaterial.Other);
        }

        private bool IsFriendlyRicochet(in ProjectileHit hit) =>
            hit.Ricocheted && hit.Shooter != null && hit.Shooter.TryGetComponent<IAiAgent>(out _) && TryGetComponent<IAiAgent>(out _);

        public void OnBlast(Vector2 centre, float distance, in BlastSettings blast, Rigidbody2D source)
        {
            if (IsDead)
                return;
            var killed = wounds.TakeBlast(distance, blast.radius);
            Wounded?.Invoke(source);
            if (killed)
                Die((Vector2)transform.position - centre);
        }

        public void Kill(Vector2 direction)
        {
            if (IsDead)
                return;
            wounds.Kill();
            Die(direction);
        }

        private void OnCollisionStay2D(Collision2D collision) => CheckRunOver(collision);

        private void OnCollisionEnter2D(Collision2D collision) => CheckRunOver(collision);

        private void CheckRunOver(Collision2D collision)
        {
            var tankBody = collision.rigidbody;
            if (IsDead || tankBody == null || !tankBody.TryGetComponent<TankController>(out _))
                return;
            var towardSoldier = (Vector2)transform.position - tankBody.position;
            if (towardSoldier.sqrMagnitude < 1e-6f)
                return;
            if (Vector2.Dot(tankBody.linearVelocity, towardSoldier.normalized) >= crushSpeed)
                Kill(tankBody.linearVelocity);
        }

        private void Die(Vector2 direction)
        {
            foreach (var part in GetComponentsInChildren<Collider2D>())
                part.enabled = false;
            if (TryGetComponent<Rigidbody2D>(out var body))
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }
            if (bloodEffect != null && pools != null)
            {
                var angle = direction.sqrMagnitude > 1e-6f ? Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f : 0f;
                pools.For(bloodEffect, 2).Get(transform.position, Quaternion.Euler(0f, 0f, angle));
            }
            Died?.Invoke(this);
            StartCoroutine(SinkAndFade());
        }

        private IEnumerator SinkAndFade()
        {
            var sprites = GetComponentsInChildren<SpriteRenderer>();
            var colors = new Color[sprites.Length];
            for (var i = 0; i < sprites.Length; i++)
            {
                colors[i] = sprites[i].color;
                sprites[i].sortingOrder -= 2;
            }
            var startScale = transform.localScale;
            for (var elapsed = 0f; elapsed < sinkSeconds; elapsed += Time.deltaTime)
            {
                var progress = elapsed / sinkSeconds;
                transform.localScale = startScale * Mathf.Lerp(1f, sunkScale, progress);
                for (var i = 0; i < sprites.Length; i++)
                {
                    var color = Color.Lerp(colors[i], colors[i] * 0.45f, Mathf.Min(1f, progress * 3f));
                    color.a = colors[i].a * (1f - progress);
                    sprites[i].color = color;
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
