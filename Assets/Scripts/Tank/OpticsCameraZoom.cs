using PolishTanks.Tank.Modules;
using Unity.Cinemachine;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>Eases a Cinemachine camera's orthographic size to the tank's optics view range.</summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class OpticsCameraZoom : MonoBehaviour
    {
        [SerializeField] private TankController tank;

        [Tooltip("Seconds to get most of the way to a new view range.")]
        [SerializeField, Min(0.01f)] private float zoomTime = 0.5f;

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
            if (tank == null)
                return;
            tank.StatsApplied += OnStatsApplied;
            if (tank.Stats != null)
                OnStatsApplied(tank.Stats);
        }

        private void OnDisable()
        {
            if (tank != null)
                tank.StatsApplied -= OnStatsApplied;
        }

        private void OnStatsApplied(TankStats stats) => targetSize = stats.ViewRange;

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
