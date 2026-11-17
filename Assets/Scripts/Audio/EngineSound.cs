using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Audio
{
    /// <summary>
    /// Loops the engine clip on the hull and raises its pitch and volume with <see cref="EngineRevs"/>.
    /// Sits next to <see cref="TankMovement"/>, which supplies the speed and driver input. Goes quiet while the engine
    /// is knocked out or the tank is wrecked.
    /// </summary>
    [RequireComponent(typeof(TankMovement), typeof(AudioSource))]
    public class EngineSound : MonoBehaviour
    {
        [SerializeField] private AudioClip engineLoop;

        [SerializeField] private Vector2 pitch = new Vector2(0.7f, 1.45f);
        [SerializeField] private Vector2 volume = new Vector2(0.12f, 0.3f);

        [Tooltip("Revs gained and lost per second.")]
        [SerializeField, Min(0.01f)] private float riseRate = 1.6f;
        [SerializeField, Min(0.01f)] private float fallRate = 0.8f;

        private TankMovement movement;
        private AudioSource source;
        private float revs;

        public float Revs => revs;

        private void Awake()
        {
            movement = GetComponent<TankMovement>();
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            if (engineLoop != null)
                source.clip = engineLoop;
        }

        private void OnEnable()
        {
            revs = 0f;
            Apply();
            if (source.clip != null)
            {
                source.time = Random.Range(0f, source.clip.length);
                source.Play();
            }
        }

        private void OnDisable() => source.Stop();

        private void Update()
        {
            var running = movement.enabled && movement.Condition.Power > 0f;
            if (running != source.isPlaying && source.clip != null)
            {
                if (running)
                    source.Play();
                else
                    source.Stop();
            }

            var target = EngineRevs.Target(movement.CurrentSpeed, movement.Settings.maxForwardSpeed, movement.Throttle, movement.Steer);
            revs = EngineRevs.Approach(revs, target, riseRate, fallRate, Time.deltaTime);
            Apply();
        }

        private void Apply()
        {
            source.pitch = Mathf.Lerp(pitch.x, pitch.y, revs);
            source.volume = Mathf.Lerp(volume.x, volume.y, revs);
        }
    }
}
