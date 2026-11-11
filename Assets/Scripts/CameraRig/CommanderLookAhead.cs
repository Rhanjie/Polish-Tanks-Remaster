using PolishTanks.Controls;
using Reflex.Attributes;
using Unity.Cinemachine;
using UnityEngine;

namespace PolishTanks.CameraRig
{
    /// <summary>
    /// The commander's binoculars: shifts the follow camera toward the cursor so the player sees further where
    /// they aim. Uses <see cref="LookAheadMath"/> and eases the shift through a <see cref="CinemachineCameraOffset"/>.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera), typeof(CinemachineCameraOffset))]
    public class CommanderLookAhead : MonoBehaviour
    {
        [Tooltip("Share of the way from the tank to the cursor the camera moves. 0.5 = halfway.")]
        [SerializeField, Range(0f, 1f)] private float strength = 0.5f;

        [Tooltip("Hard cap on the shift in world units, whatever the zoom and screen shape.")]
        [SerializeField, Min(0f)] private float maxDistance = 8f;

        [Tooltip("Seconds to get most of the way to a new shift.")]
        [SerializeField, Min(0.01f)] private float smoothTime = 0.25f;

        [Inject] private IPlayerInput input;

        private CinemachineCamera vcam;
        private CinemachineCameraOffset cameraOffset;
        private Vector2 current;
        private Vector2 velocity;

        private void Awake()
        {
            vcam = GetComponent<CinemachineCamera>();
            cameraOffset = GetComponent<CinemachineCameraOffset>();
        }

        private void OnDisable()
        {
            current = Vector2.zero;
            velocity = Vector2.zero;
            if (cameraOffset != null)
                cameraOffset.Offset = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (input == null)
                return;
            var target = LookAheadMath.Offset(input.PointerScreenPosition, new Vector2(Screen.width, Screen.height),
                vcam.Lens.OrthographicSize, strength, maxDistance);
            current = Vector2.SmoothDamp(current, target, ref velocity, smoothTime);
            cameraOffset.Offset = new Vector3(current.x, current.y, 0f);
        }
    }
}
