using PolishTanks.Tank.Modules;
using Reflex.Attributes;
using Unity.Cinemachine;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>
    /// Eases a Cinemachine camera's orthographic size to the player tank's optics view range, scaled by a common
    /// factor so the whole game sits further out without changing how the optics compare to each other.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class OpticsCameraZoom : MonoBehaviour
    {
        [Inject] private PlayerTank player;

        [Tooltip("Seconds to get most of the way to a new view range.")]
        [SerializeField, Min(0.01f)] private float zoomTime = 0.5f;

        [Tooltip("Multiplies every optics view range; above 1 pulls the camera further out.")]
        [SerializeField, Min(0.1f)] private float viewScale = 1.25f;

        private CinemachineCamera vcam;
        private float targetSize;
        private float zoomVelocity;

        private void Awake()
        {
            vcam = GetComponent<CinemachineCamera>();
            targetSize = vcam.Lens.OrthographicSize;
        }

        private void OnEnable()
        {
            var tank = player?.Controller;
            if (tank == null)
                return;
            tank.StatsApplied += OnStatsApplied;
            if (tank.Stats != null)
                OnStatsApplied(tank.Stats);
        }

        private void OnDisable()
        {
            var tank = player?.Controller;
            if (tank != null)
                tank.StatsApplied -= OnStatsApplied;
        }

        private void OnStatsApplied(TankStats stats) => targetSize = stats.ViewRange * viewScale;

        private void LateUpdate()
        {
            var lens = vcam.Lens;
            if (Mathf.Approximately(lens.OrthographicSize, targetSize))
                return;
            lens.OrthographicSize = Mathf.SmoothDamp(lens.OrthographicSize, targetSize, ref zoomVelocity, zoomTime);
            vcam.Lens = lens;
        }
    }
}
