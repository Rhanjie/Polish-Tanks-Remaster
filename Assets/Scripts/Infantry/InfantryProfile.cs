using UnityEngine;

namespace PolishTanks.Infantry
{
    /// <summary>
    /// One kind of soldier (rifleman, grenadier): how far it sees, its rifle and, if it has any, its anti-tank grenades.
    /// How it reacts and searches comes from the soldier's combat doctrine, like a tank's.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/AI/Infantry Profile", fileName = "Infantry_")]
    public class InfantryProfile : ScriptableObject
    {
        [Tooltip("Base sight distance in metres, before the doctrine's sight factor.")]
        [SerializeField, Min(1f)] private float viewRange = 14f;
        [SerializeField] private RifleSettings rifle = RifleSettings.Default;
        [SerializeField] private GrenadeSettings grenades = GrenadeSettings.Default;

        public float ViewRange => viewRange;
        public RifleSettings Rifle => rifle;
        public GrenadeSettings Grenades => grenades;
    }
}
