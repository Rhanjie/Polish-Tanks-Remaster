using System;
using UnityEngine;

namespace PolishTanks.Tank
{
    /// <summary>The tank the player drives in the current scene, bound by the scene installer.</summary>
    public sealed class PlayerTank
    {
        public PlayerTank(TankController controller)
        {
            Controller = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
        }

        public TankController Controller { get; }
        public Transform Transform => Controller.transform;
    }
}
