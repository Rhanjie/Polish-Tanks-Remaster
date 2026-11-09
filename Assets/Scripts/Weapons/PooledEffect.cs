using UnityEngine;

namespace PolishTanks.Weapons
{
    /// <summary>A one-shot particle effect that plays when spawned and returns to its pool when finished.</summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledEffect : PooledBehaviour
    {
        private ParticleSystem particles;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        protected internal override void OnSpawned()
        {
            particles.Clear(true);
            particles.Play(true);
        }

        protected internal override void OnReturned()
        {
            if (particles != null)
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnParticleSystemStopped() => ReturnToPool();
    }
}
