using PolishTanks.CameraRig;
using PolishTanks.GameLoop;
using PolishTanks.Missions;
using PolishTanks.Navigation;
using PolishTanks.Tank;
using PolishTanks.Weapons;
using Reflex.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace PolishTanks.Installers
{
    /// <summary>
    /// Bindings for a gameplay scene: its camera, camera shake, the player's tank, the game pause, the mission (a <see cref="NoMission"/> stand-in
    /// when the scene has no <see cref="MissionDirector"/>), the object pools and its path
    /// finding (a <see cref="NoNavigation"/> stand-in when the scene has no <see cref="NavigationGridSource"/>).
    /// Must be a child of the scene's SceneScope (ContainerScope) object.
    /// </summary>
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private CinemachineImpulseSource cameraShakeSource;
        [SerializeField] private TankController playerTank;
        [Tooltip("Optional. Without it every path search fails, so AI can't drive.")]
        [SerializeField] private NavigationGridSource navigation;
        [Tooltip("Optional. The mission this scene plays; without it the scene is free play.")]
        [SerializeField] private MissionDirector mission;

        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterValue(new GameCamera(gameCamera), new[] { typeof(IGameCamera) });
            builder.RegisterValue(new CinemachineCameraShake(cameraShakeSource), new[] { typeof(ICameraShake) });
            builder.RegisterValue(new PlayerTank(playerTank));
            builder.RegisterValue(new GamePause(), new[] { typeof(IGamePause) });
            builder.RegisterValue(mission != null ? mission : (IMission)new NoMission(), new[] { typeof(IMission) });
            var sceneNavigation = navigation != null ? navigation.Navigation : new NoNavigation();
            builder.RegisterValue(sceneNavigation, new[] { typeof(INavigation) });

            var poolsRoot = new GameObject("[Pools]").transform;
            poolsRoot.SetParent(transform, false);
            builder.RegisterValue(new PoolService(poolsRoot), new[] { typeof(IPoolService) });
        }
    }
}
