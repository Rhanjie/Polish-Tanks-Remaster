using System;
using Unity.Cinemachine;
using UnityEngine;

namespace PolishTanks.CameraRig
{
    /// <summary><see cref="ICameraShake"/> sent through a Cinemachine impulse source to the listener on the follow camera.</summary>
    public sealed class CinemachineCameraShake : ICameraShake
    {
        private readonly CinemachineImpulseSource source;

        public CinemachineCameraShake(CinemachineImpulseSource source)
        {
            this.source = source != null ? source : throw new ArgumentNullException(nameof(source));
        }

        public void Shake(Vector2 origin, Vector2 direction, float force)
        {
            if (force <= 0f || direction.sqrMagnitude <= 0f)
                return;
            source.GenerateImpulseAtPositionWithVelocity(origin, direction.normalized * force);
        }
    }
}
