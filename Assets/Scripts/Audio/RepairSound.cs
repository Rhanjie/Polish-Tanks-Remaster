using PolishTanks.Tank;
using UnityEngine;

namespace PolishTanks.Audio
{
    /// <summary>Loops the crew's tinkering while the tank's <see cref="FieldRepair"/> is running. Sits on a child of the tank.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class RepairSound : MonoBehaviour
    {
        [SerializeField] private AudioClip repairLoop;

        private AudioSource source;
        private FieldRepair repair;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            if (repairLoop != null)
                source.clip = repairLoop;
            repair = GetComponentInParent<FieldRepair>();
        }

        private void OnDisable() => source.Stop();

        private void Update()
        {
            var repairing = repair != null && repair.IsRepairing;
            if (repairing == source.isPlaying || source.clip == null)
                return;
            if (repairing)
            {
                source.time = Random.Range(0f, source.clip.length);
                source.Play();
            }
            else
            {
                source.Stop();
            }
        }
    }
}
