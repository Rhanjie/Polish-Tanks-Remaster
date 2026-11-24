using UnityEngine;

namespace PolishTanks.AI
{
    /// <summary>
    /// <see cref="IUnitDriver"/> plus what only a tank has: how far the turret still has to turn, the main gun and the
    /// machine guns, and turning the hull in place to a heading while standing (angling the armor). The
    /// <see cref="Gunner"/> fights through this.
    /// </summary>
    public interface ITankDriver : IUnitDriver
    {
        float AimError { get; }

        bool FireMainGun();
        void HoldHullHeading(Vector2? heading);
        void SetMachineGuns(bool held, Vector2 target);
    }
}
