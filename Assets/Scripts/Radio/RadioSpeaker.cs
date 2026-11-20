using UnityEngine;

namespace PolishTanks.Radio
{
    /// <summary>
    /// Someone who talks to the player over the radio: the name shown above their messages and their portrait.
    /// Without a portrait the radio panel shows a stranger's silhouette.
    /// </summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Radio/Speaker", fileName = "Speaker_")]
    public class RadioSpeaker : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private Sprite portrait;

        public string DisplayName => displayName;
        public Sprite Portrait => portrait;
    }
}
