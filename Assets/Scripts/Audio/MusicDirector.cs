using PolishTanks.Combat;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Audio
{
    /// <summary>
    /// Quiet background music for the player's tank: a calm track that crossfades to a war track while the player
    /// is fighting (firing or taking hits) and back once things stay quiet for a while. Sits on a child of the tank
    /// prefab with one looping AudioSource per track and switches itself off on every tank but the player's.
    /// </summary>
    public class MusicDirector : MonoBehaviour
    {
        [SerializeField] private AudioSource calm;
        [SerializeField] private AudioSource war;

        [SerializeField, Range(0f, 1f)] private float volume = 0.25f;

        [Tooltip("Seconds without firing or being hit before the music calms down.")]
        [SerializeField, Min(0f)] private float combatHold = 20f;

        [Tooltip("Seconds for a full crossfade.")]
        [SerializeField, Min(0.01f)] private float fadeTime = 4f;

        [Inject] private PlayerTank playerTank;

        private TankController controller;
        private Damageable damageable;
        private TankGun[] guns;
        private float lastAction = float.NegativeInfinity;
        private float warMix;

        public float WarMix => warMix;

        private void Awake()
        {
            controller = GetComponentInParent<TankController>();
            damageable = GetComponentInParent<Damageable>();
            guns = controller != null ? controller.GetComponentsInChildren<TankGun>(true) : new TankGun[0];
        }

        private void Start()
        {
            if (playerTank == null || controller == null || playerTank.Controller != controller)
            {
                Silence(calm);
                Silence(war);
                enabled = false;
                return;
            }
            foreach (var gun in guns)
                gun.Fired += OnFired;
            if (damageable != null)
                damageable.Hit += OnHit;
            Begin(calm, volume);
            Begin(war, 0f);
        }

        private void OnDestroy()
        {
            if (guns != null)
                foreach (var gun in guns)
                    if (gun != null)
                        gun.Fired -= OnFired;
            if (damageable != null)
                damageable.Hit -= OnHit;
        }

        private void Update()
        {
            var fighting = Time.time - lastAction < combatHold;
            warMix = Mathf.MoveTowards(warMix, fighting ? 1f : 0f, Time.deltaTime / fadeTime);
            if (calm != null)
                calm.volume = volume * (1f - warMix);
            if (war != null)
                war.volume = volume * warMix;
        }

        private void OnFired(TankGun _) => lastAction = Time.time;

        private void OnHit(PenetrationResult _, HitOutcome __, float ___) => lastAction = Time.time;

        private static void Begin(AudioSource source, float startVolume)
        {
            if (source == null || source.clip == null)
                return;
            source.loop = true;
            source.ignoreListenerPause = true;
            source.volume = startVolume;
            source.Play();
        }

        private static void Silence(AudioSource source)
        {
            if (source != null)
                source.Stop();
        }
    }
}
