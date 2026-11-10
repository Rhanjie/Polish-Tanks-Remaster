using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>
    /// A one-shot particle effect that plays when spawned and returns to its pool once every
    /// particle system in it (children included) has had time to finish.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledEffect : PooledBehaviour
    {
        private ParticleSystem particles;
        private float totalDuration;
        private float timeLeft;

        public float TotalDuration => totalDuration;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            foreach (var system in GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = system.main;
                main.loop = false;
                main.playOnAwake = false;
                totalDuration = Mathf.Max(totalDuration, main.duration + main.startLifetime.constantMax);
            }
        }

        protected internal override void OnSpawned()
        {
            timeLeft = totalDuration;
            particles.Clear(true);
            particles.Play(true);
        }

        protected internal override void OnReturned()
        {
            if (particles != null)
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
                ReturnToPool();
        }
    }
}
