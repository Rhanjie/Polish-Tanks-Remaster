using PolishTanks.Controls;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Player input for tests: every value is set directly by the test.</summary>
    internal sealed class FakePlayerInput : IPlayerInput
    {
        public Vector2 Move { get; set; }
        public Vector2 PointerScreenPosition { get; set; }
        public bool FireHeld { get; set; }
        public bool FirePressedThisFrame { get; set; }
        public bool SecondaryFireHeld { get; set; }
        public bool SecondaryFirePressedThisFrame { get; set; }
    }
}
