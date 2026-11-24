using System;

namespace PolishTanks.GameLoop
{
    /// <summary>
    /// Stops the game clock for as long as anyone holds it. Every holder (pause menu, module panel) holds and releases
    /// on its own, so closing one menu never resumes the game while another still needs it stopped.
    /// </summary>
    public interface IGamePause
    {
        bool IsPaused { get; }
        event Action<bool> Changed;
        void Hold(object holder);
        void Release(object holder);
    }
}
