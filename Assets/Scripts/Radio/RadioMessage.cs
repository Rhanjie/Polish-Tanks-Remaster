using System;
using UnityEngine;

namespace PolishTanks.Radio
{
    /// <summary>One radio message: who says it, what they say and, optionally, the recording of them saying it.</summary>
    [Serializable]
    public class RadioMessage
    {
        [SerializeField] private RadioSpeaker speaker;
        [Tooltip("Player-facing text of the message.")]
        [SerializeField, TextArea(2, 5)] private string text;
        [Tooltip("Optional. Played when the message comes up; it stays on screen at least as long as the recording.")]
        [SerializeField] private AudioClip voice;

        public RadioMessage(RadioSpeaker speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }

        public RadioSpeaker Speaker => speaker;
        public string Text => text ?? string.Empty;
        public AudioClip Voice => voice;
    }
}
