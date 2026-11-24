using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Audio
{
    /// <summary>Plays a random shot sound from the AudioSource on the same muzzle each time its <see cref="TankGun"/> fires.</summary>
    [RequireComponent(typeof(TankGun), typeof(AudioSource))]
    public class GunSound : MonoBehaviour
    {
        [SerializeField] private AudioClip[] shots;

        [Tooltip("Random pitch range applied to each shot so repeated fire does not sound identical.")]
        [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.08f);

        private TankGun gun;
        private AudioSource source;

        private void Awake()
        {
            gun = GetComponent<TankGun>();
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
        }

        private void OnEnable() => gun.Fired += Play;

        private void OnDisable() => gun.Fired -= Play;

        private void Play(TankGun _)
        {
            if (shots == null || shots.Length == 0)
                return;
            var clip = shots[Random.Range(0, shots.Length)];
            if (clip == null)
                return;
            source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            source.PlayOneShot(clip);
        }
    }
}
