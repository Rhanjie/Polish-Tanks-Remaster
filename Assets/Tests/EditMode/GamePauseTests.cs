using NUnit.Framework;
using PolishTanks.GameLoop;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    /// <summary><see cref="GamePause"/> with several holders sharing one clock.</summary>
    public class GamePauseTests
    {
        private float originalTimeScale;
        private bool originalAudioPause;

        [SetUp]
        public void SetUp()
        {
            originalTimeScale = Time.timeScale;
            originalAudioPause = AudioListener.pause;
            Time.timeScale = 0.5f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = originalTimeScale;
            AudioListener.pause = originalAudioPause;
        }

        [Test]
        public void Release_WithTwoHolders_RestoresTimeScaleOnlyAfterLastRelease()
        {
            var pause = new GamePause();
            var first = new object();
            var second = new object();
            pause.Hold(first);
            pause.Hold(second);

            pause.Release(first);
            var afterFirst = Time.timeScale;
            pause.Release(second);

            Assert.That(afterFirst, Is.Zero);
            Assert.That(Time.timeScale, Is.EqualTo(0.5f));
        }

        [Test]
        public void Dispose_WhilePaused_ResumesClock()
        {
            var pause = new GamePause();
            pause.Hold(new object());

            pause.Dispose();

            Assert.That(Time.timeScale, Is.EqualTo(0.5f));
        }
    }
}
