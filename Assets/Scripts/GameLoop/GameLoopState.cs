using System;

namespace PolishTanks.GameLoop
{
    /// <summary>
    /// Rules of the scene's game loop without any UI. The pause key toggles between playing and paused. Once the
    /// player's tank is destroyed the defeat comes after a delay that only runs while playing, so the wreck can be
    /// seen burning first; after the defeat the pause key does nothing.
    /// </summary>
    public sealed class GameLoopState
    {
        private readonly float defeatDelay;
        private float defeatCountdown = -1f;

        public GameLoopState(float defeatDelay)
        {
            this.defeatDelay = Math.Max(0f, defeatDelay);
        }

        public GameLoopPhase Phase { get; private set; } = GameLoopPhase.Playing;

        public event Action<GameLoopPhase> PhaseChanged;

        public void TogglePause()
        {
            if (Phase == GameLoopPhase.Playing)
                SetPhase(GameLoopPhase.Paused);
            else if (Phase == GameLoopPhase.Paused)
                SetPhase(GameLoopPhase.Playing);
        }

        public void Resume()
        {
            if (Phase == GameLoopPhase.Paused)
                SetPhase(GameLoopPhase.Playing);
        }

        public void PlayerDestroyed()
        {
            if (Phase == GameLoopPhase.Defeated || defeatCountdown >= 0f)
                return;
            defeatCountdown = defeatDelay;
            Tick(0f);
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (Phase != GameLoopPhase.Playing || defeatCountdown < 0f)
                return;
            defeatCountdown -= unscaledDeltaTime;
            if (defeatCountdown <= 0f)
                SetPhase(GameLoopPhase.Defeated);
        }

        private void SetPhase(GameLoopPhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }
    }
}
