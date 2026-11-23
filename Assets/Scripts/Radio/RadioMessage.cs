using System;
using UnityEngine;

namespace PolishTanks.Radio
{
    /// <summary>One radio message: who says it and what they say.</summary>
    [Serializable]
    public class RadioMessage
    {
        [SerializeField] private RadioSpeaker speaker;
        [Tooltip("Player-facing text of the message.")]
        [SerializeField, TextArea(2, 5)] private string text;

        public RadioMessage(RadioSpeaker speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }

        public RadioSpeaker Speaker => speaker;
        public string Text => text ?? string.Empty;
    }
}
