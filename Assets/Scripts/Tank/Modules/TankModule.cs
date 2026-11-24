using UnityEngine;

namespace PolishTanks.Tank.Modules
{
    /// <summary>
    /// Base for every tank part. Holds what the workshop needs to list and price a part;
    /// subclasses add the numbers that feed <see cref="TankStatsCalculator"/>.
    /// </summary>
    public abstract class TankModule : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private Sprite icon;

        [Tooltip("Mass in kilograms.")]
        [SerializeField, Min(0f)] private float mass = 500f;

        [Tooltip("Price in the workshop.")]
        [SerializeField, Min(0)] private int cost = 100;

        [Tooltip("Rough progression level, 1 = starting gear.")]
        [SerializeField, Range(1, 5)] private int tier = 1;

        public abstract ModuleSlot Slot { get; }

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public float Mass => mass;
        public int Cost => cost;
        public int Tier => tier;
    }
}
