using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Radio
{
    /// <summary>Radio messages played one after another; the player confirms each before the next one comes in.</summary>
    [CreateAssetMenu(menuName = "Polish Tanks/Radio/Sequence", fileName = "Radio_")]
    public class RadioSequence : ScriptableObject
    {
        [SerializeField] private List<RadioMessage> messages = new List<RadioMessage>();

        public IReadOnlyList<RadioMessage> Messages => messages;
    }
}
