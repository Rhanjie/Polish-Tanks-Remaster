using PolishTanks.Controls;
using Reflex.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolishTanks.Installers
{
    /// <summary>
    /// Root bindings shared by every scene. Lives on the RootScope prefab listed in
    /// Resources/ReflexSettings.
    /// </summary>
    public class GameInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterValue(new InputSystemPlayerInput(InputSystem.actions), new[] { typeof(IPlayerInput) });
        }
    }
}
