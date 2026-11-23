using System;

namespace PolishTanks.GameLoop
{
    /// <summary>
    /// Rules of the scene's game loop without any UI. The pause key toggles between playing and paused. Losing the
    /// player's tank ends the game in defeat and winning the mission in victory, each after its own delay that only
    /// runs while playing, so the wreck (or the last shot) can be seen first. The first ending that starts wins; after
    /// the end the pause key does nothing.
    /// </summary>
    public sealed class GameLoopState
    {
        private readonly float defeatDelay;
        private readonly float victoryDelay;
        private GameLoopPhase pendingEnd;
        private float endCountdown = -1f;

        public GameLoopState(float defeatDelay, float victoryDelay = 2f)
        {
            this.defeatDelay = Math.Max(0f, defeatDelay);
            this.victoryDelay = Math.Max(0f, victoryDelay);
        }

        public GameLoopPhase Phase { get; private set; } = GameLoopPhase.Playing;

        public bool IsOver => Phase == GameLoopPhase.Defeated || Phase == GameLoopPhase.Victory;

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

        public void PlayerDestroyed() => BeginEnd(GameLoopPhase.Defeated, defeatDelay);

        public void MissionWon() => BeginEnd(GameLoopPhase.Victory, victoryDelay);

        public void Tick(float unscaledDeltaTime)
        {
            if (Phase != GameLoopPhase.Playing || endCountdown < 0f)
                return;
            endCountdown -= unscaledDeltaTime;
            if (endCountdown <= 0f)
                SetPhase(pendingEnd);
        }

        private void BeginEnd(GameLoopPhase end, float delay)
        {
            if (IsOver || endCountdown >= 0f)
                return;
            pendingEnd = end;
            endCountdown = delay;
            Tick(0f);
        }

        private void SetPhase(GameLoopPhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }
    }
}
