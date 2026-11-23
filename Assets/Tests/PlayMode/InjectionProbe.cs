using PolishTanks.Radio;
using Reflex.Attributes;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Remembers whether its injected service was already there when it woke up.</summary>
    public class InjectionProbe : MonoBehaviour
    {
        [Inject] private RadioChannel radio;

        public bool InjectedBeforeAwake { get; private set; }

        private void Awake() => InjectedBeforeAwake = radio != null;
    }
}
