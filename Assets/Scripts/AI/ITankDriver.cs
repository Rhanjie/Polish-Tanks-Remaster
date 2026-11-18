using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// <see cref="IUnitDriver"/> plus what only a tank has: how far the turret still has to turn, the main gun and the
    /// machine guns. The <see cref="Gunner"/> fights through this.
    /// </summary>
    public interface ITankDriver : IUnitDriver
    {
        float AimError { get; }

        bool FireMainGun();
        void SetMachineGuns(bool held, Vector2 target);
    }
}
