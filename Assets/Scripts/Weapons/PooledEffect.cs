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
        [Tooltip("Sounds to pick from at random on each play. Empty keeps the AudioSource clip.")]
        [SerializeField] private AudioClip[] soundVariants;

        private ParticleSystem particles;
        private AudioSource sound;
        private float totalDuration;
        private float timeLeft;

        public float TotalDuration => totalDuration;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            sound = GetComponent<AudioSource>();
            if (sound != null)
                sound.playOnAwake = false;
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
            if (sound != null && soundVariants != null && soundVariants.Length > 0)
                sound.clip = soundVariants[Random.Range(0, soundVariants.Length)];
            if (sound != null && sound.clip != null)
            {
                sound.pitch = Random.Range(0.85f, 1.15f);
                sound.Play();
                timeLeft = Mathf.Max(timeLeft, sound.clip.length / sound.pitch);
            }
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
