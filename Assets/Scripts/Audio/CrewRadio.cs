using System.Collections.Generic;
using PolishTanks.Damage;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Audio
{
    /// <summary>
    /// The crew's radio reports for the player's tank: a part getting worse, the engine catching fire or going out,
    /// and field repairs starting and finishing. Sits on a child of the tank prefab and stays silent on every tank
    /// but the player's. Reports go through a <see cref="RadioQueue"/> so they never talk over each other.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CrewRadio : MonoBehaviour
    {
        [SerializeField] private CrewLineClip[] lines;

        [Tooltip("Seconds before the same report can be spoken again.")]
        [SerializeField, Min(0f)] private float cooldown = 8f;

        [Tooltip("Silence between two reports.")]
        [SerializeField, Min(0f)] private float gap = 0.4f;

        [Inject] private PlayerTank playerTank;

        private readonly Dictionary<PartId, PartCondition> conditions = new Dictionary<PartId, PartCondition>();
        private RadioQueue queue;
        private AudioSource source;
        private TankController controller;
        private ModuleDamage damage;
        private FieldRepair repair;
        private bool wasRepairing;
        private float quietUntil;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            queue = new RadioQueue(cooldown);
            controller = GetComponentInParent<TankController>();
            damage = GetComponentInParent<ModuleDamage>();
            repair = GetComponentInParent<FieldRepair>();
        }

        private void Start()
        {
            if (playerTank == null || controller == null || playerTank.Controller != controller)
            {
                enabled = false;
                return;
            }
            if (damage != null)
            {
                damage.PartChanged += OnPartChanged;
                damage.FireChanged += OnFireChanged;
            }
            if (repair != null)
                repair.Changed += OnRepairChanged;
            controller.Destroyed += OnTankDestroyed;
        }

        private void OnDestroy()
        {
            if (damage != null)
            {
                damage.PartChanged -= OnPartChanged;
                damage.FireChanged -= OnFireChanged;
            }
            if (repair != null)
                repair.Changed -= OnRepairChanged;
            if (controller != null)
                controller.Destroyed -= OnTankDestroyed;
        }

        private void Update()
        {
            if (source.isPlaying || Time.time < quietUntil || !queue.TryNext(Time.time, out var line))
                return;
            var clip = ClipFor(line);
            if (clip == null)
                return;
            source.PlayOneShot(clip);
            quietUntil = Time.time + clip.length + gap;
        }

        private void OnPartChanged(PartId id, PartCondition condition)
        {
            conditions.TryGetValue(id, out var before);
            conditions[id] = condition;
            if (condition <= before || controller.IsDestroyed)
                return;
            var line = RadioQueue.ForPart(id.Part);
            if (line.HasValue)
                queue.Enqueue(line.Value, Time.time);
        }

        private void OnFireChanged(bool burning)
        {
            if (!controller.IsDestroyed)
                queue.Enqueue(burning ? CrewLine.EngineFire : CrewLine.FireOut, Time.time);
        }

        private void OnRepairChanged()
        {
            var repairing = repair.IsRepairing;
            if (repairing && !wasRepairing)
                queue.Enqueue(CrewLine.RepairStarted, Time.time);
            else if (!repairing && wasRepairing && !controller.IsDestroyed && !repair.HasWork)
                queue.Enqueue(CrewLine.RepairComplete, Time.time);
            wasRepairing = repairing;
        }

        private void OnTankDestroyed(TankController _)
        {
            queue.Clear();
            source.Stop();
        }

        private AudioClip ClipFor(CrewLine line)
        {
            if (lines == null)
                return null;
            foreach (var entry in lines)
                if (entry.line == line)
                    return entry.clip;
            return null;
        }
    }
}
