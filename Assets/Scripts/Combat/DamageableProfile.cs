using PolishTanks.Weapons;
using UnityEngine;

namespace PolishTanks.Combat
{
    /// <summary>
    /// Toughness of a non-tank object (crate, wall, building part) and what happens when it breaks.
    /// Tanks take hit points and armor from their modules; for them only the destruction settings apply.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Damageable Profile", fileName = "Damageable_")]
    public class DamageableProfile : ScriptableObject
    {
        [SerializeField, Min(1f)] private float hitPoints = 100f;
        [SerializeField] private ArmorProfile armor = new ArmorProfile(10f, 10f, 10f);

        [Tooltip("Pooled effect played where the object breaks. Optional.")]
        [SerializeField] private PooledEffect destroyedEffect;

        [Tooltip("Remove the object when it breaks. Off for tanks, which stay as wrecks.")]
        [SerializeField] private bool removeOnDeath = true;

        public float HitPoints => hitPoints;
        public ArmorProfile Armor => armor;
        public PooledEffect DestroyedEffect => destroyedEffect;
        public bool RemoveOnDeath => removeOnDeath;
    }
}
