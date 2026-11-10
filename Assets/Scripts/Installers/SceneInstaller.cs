using PolishTanks.CameraRig;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace PolishTanks.Installers
{
    /// <summary>
    /// Bindings for a gameplay scene: its camera, camera shake, the player's tank and the object pools.
    /// Must be a child of the scene's SceneScope (ContainerScope) object.
    /// </summary>
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private CinemachineImpulseSource cameraShakeSource;
        [SerializeField] private TankController playerTank;

        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterValue(new GameCamera(gameCamera), new[] { typeof(IGameCamera) });
            builder.RegisterValue(new CinemachineCameraShake(cameraShakeSource), new[] { typeof(ICameraShake) });
            builder.RegisterValue(new PlayerTank(playerTank));

            var poolsRoot = new GameObject("[Pools]").transform;
            poolsRoot.SetParent(transform, false);
            builder.RegisterValue(new PoolService(poolsRoot), new[] { typeof(IPoolService) });
        }
    }
}
