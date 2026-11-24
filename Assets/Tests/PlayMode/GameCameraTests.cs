using NUnit.Framework;
using PolishTanks.CameraRig;
using UnityEngine;

namespace PolishTanks.Tests.PlayMode
{
    public class GameCameraTests
    {
        private GameObject cameraObject;

        [TearDown]
        public void TearDown()
        {
            if (cameraObject != null)
                Object.DestroyImmediate(cameraObject);
        }

        [Test]
        public void ScreenToWorld_ScreenCenter_ReturnsCameraPositionInXY()
        {
            cameraObject = new GameObject("TestCamera");
            cameraObject.transform.position = new Vector3(3f, 4f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            var gameCamera = new GameCamera(camera);

            var world = gameCamera.ScreenToWorld(new Vector2(camera.pixelWidth / 2f, camera.pixelHeight / 2f));

            Assert.That(Vector2.Distance(world, new Vector2(3f, 4f)), Is.LessThan(0.01f));
        }
    }
}
