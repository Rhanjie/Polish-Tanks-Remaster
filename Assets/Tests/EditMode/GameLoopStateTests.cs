using NUnit.Framework;
using PolishTanks.GameLoop;

namespace PolishTanks.Tests.EditMode
{
    /// <summary><see cref="GameLoopState"/>: pause toggling and the delayed defeat.</summary>
    public class GameLoopStateTests
    {
        [Test]
        public void Tick_AfterPlayerDestroyed_DefeatsOnlyOnceDelayPassed()
        {
            var state = new GameLoopState(2f);
            state.PlayerDestroyed();

            state.Tick(1.9f);
            var beforeDelay = state.Phase;
            state.Tick(0.2f);

            Assert.That(beforeDelay, Is.EqualTo(GameLoopPhase.Playing));
            Assert.That(state.Phase, Is.EqualTo(GameLoopPhase.Defeated));
        }

        [Test]
        public void Tick_WhilePaused_DoesNotRunDefeatDelay()
        {
            var state = new GameLoopState(2f);
            state.PlayerDestroyed();
            state.TogglePause();

            state.Tick(10f);
            state.TogglePause();

            Assert.That(state.Phase, Is.EqualTo(GameLoopPhase.Playing));
        }

        [Test]
        public void Tick_AfterMissionWon_ReachesVictoryOnlyOnceDelayPassed()
        {
            var state = new GameLoopState(2f, 1f);
            state.MissionWon();

            state.Tick(0.9f);
            var beforeDelay = state.Phase;
            state.Tick(0.2f);

            Assert.That(beforeDelay, Is.EqualTo(GameLoopPhase.Playing));
            Assert.That(state.Phase, Is.EqualTo(GameLoopPhase.Victory));
        }

        [Test]
        public void PlayerDestroyed_DuringPendingVictory_StaysVictory()
        {
            var state = new GameLoopState(0f, 1f);
            state.MissionWon();

            state.PlayerDestroyed();
            state.Tick(2f);

            Assert.That(state.Phase, Is.EqualTo(GameLoopPhase.Victory));
        }

        [Test]
        public void TogglePause_AfterDefeat_StaysDefeated()
        {
            var state = new GameLoopState(0f);
            state.PlayerDestroyed();

            state.TogglePause();

            Assert.That(state.Phase, Is.EqualTo(GameLoopPhase.Defeated));
        }
    }
}
