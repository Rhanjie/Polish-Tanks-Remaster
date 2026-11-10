using System.Collections.Generic;
using PolishTanks.CameraRig;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    /// <summary>Camera shake for tests: records every requested kick instead of moving a camera.</summary>
    internal sealed class RecordingCameraShake : ICameraShake
    {
        public List<(Vector2 direction, float force)> Kicks { get; } = new List<(Vector2, float)>();

        public void Shake(Vector2 direction, float force) => Kicks.Add((direction, force));
    }
}
