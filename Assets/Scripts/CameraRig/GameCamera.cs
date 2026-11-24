using System;
using UnityEngine;

namespace PolishTanks.CameraRig
{
    /// <summary><see cref="IGameCamera"/> over a Unity camera looking down the Z axis at the gameplay plane.</summary>
    public sealed class GameCamera : IGameCamera
    {
        private readonly Camera camera;

        public GameCamera(Camera camera)
        {
            this.camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
        }

        public Vector2 Position => camera.transform.position;

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            var point = new Vector3(screenPosition.x, screenPosition.y, -camera.transform.position.z);
            return camera.ScreenToWorldPoint(point);
        }
    }
}
