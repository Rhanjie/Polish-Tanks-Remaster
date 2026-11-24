using System;
using UnityEngine;

namespace PolishTanks.Audio
{
    /// <summary>The recording the radio plays for one <see cref="CrewLine"/>.</summary>
    [Serializable]
    public struct CrewLineClip
    {
        public CrewLine line;
        public AudioClip clip;
    }
}
