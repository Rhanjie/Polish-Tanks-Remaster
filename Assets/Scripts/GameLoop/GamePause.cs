using System;
using System.Collections.Generic;
using PolishTanks.CameraRig;
using UnityEngine;

namespace PolishTanks.GameLoop
{
    /// <summary>
    /// <see cref="IGamePause"/> for a gameplay scene: while held, the time scale is zero, sounds stop (sources with
    /// ignoreListenerPause, like music, keep playing) and the commander look-ahead holds the camera where it is.
    /// Disposed with the scene container, so a scene left while paused hands the next one a running clock.
    /// </summary>
    public sealed class GamePause : IGamePause, IDisposable
    {
        private readonly HashSet<object> holders = new HashSet<object>();
        private float timeScaleBeforePause = 1f;

        public bool IsPaused => holders.Count > 0;

        public event Action<bool> Changed;

        public void Hold(object holder)
        {
            if (holder == null)
                throw new ArgumentNullException(nameof(holder));
            if (holders.Add(holder) && holders.Count == 1)
                Apply(true);
        }

        public void Release(object holder)
        {
            if (holder != null && holders.Remove(holder) && holders.Count == 0)
                Apply(false);
        }

        public void Dispose()
        {
            if (holders.Count == 0)
                return;
            holders.Clear();
            Apply(false);
        }

        private void Apply(bool paused)
        {
            if (paused)
            {
                timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = timeScaleBeforePause;
            }
            AudioListener.pause = paused;
            foreach (var lookAhead in UnityEngine.Object.FindObjectsByType<CommanderLookAhead>())
                lookAhead.Frozen = paused;
            Changed?.Invoke(paused);
        }
    }
}
