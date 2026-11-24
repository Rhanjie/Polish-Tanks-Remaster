using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolishTanks.Screenshots
{
    /// <summary>
    /// Saves a PNG of the game view to the Screenshots folder next to the game (the project folder in the editor)
    /// when the Screenshot action (F4) is pressed, and names the file on screen for a moment. The notice is left out
    /// of the shot itself. Created on its own in desktop builds and the editor, so scenes need no setup; web builds
    /// get none, since a browser game has no folder of its own to save to.
    /// </summary>
    public class ScreenshotCapturer : MonoBehaviour
    {
        private const string ActionName = "Player/Screenshot";
        private const string FolderName = "Screenshots";
        private const float NoticeSeconds = 2.5f;
        private const float NoticeHeight = 28f;
        private const float NoticeMargin = 12f;

        private InputAction screenshot;
        private string notice;
        private float noticeUntil;
        private int captureFrame = -1;
        private GUIStyle noticeStyle;

#if !UNITY_WEBGL
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var host = new GameObject("[Screenshots]");
            DontDestroyOnLoad(host);
            host.AddComponent<ScreenshotCapturer>();
        }
#endif

        private static string Folder => Path.Combine(Path.GetDirectoryName(Application.dataPath), FolderName);

        private void Awake()
        {
            var actions = InputSystem.actions;
            screenshot = actions != null ? actions.FindAction(ActionName) : null;
            if (screenshot == null)
                Debug.LogWarning($"{nameof(ScreenshotCapturer)}: no '{ActionName}' action, the screenshot key is off.");
        }

        private void Update()
        {
            if (screenshot != null && screenshot.WasPressedThisFrame())
                Capture();
        }

        private void Capture()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                var path = ScreenshotFileNames.Next(Folder, DateTime.Now, File.Exists);
                ScreenCapture.CaptureScreenshot(path);
                captureFrame = Time.frameCount;
                Debug.Log($"Screenshot saved to {path}");
                ShowNotice(string.Format(ScreenshotText.SavedFormat, Path.GetFileName(path)));
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogWarning($"Screenshot failed: {exception.Message}");
                ShowNotice(string.Format(ScreenshotText.FailedFormat, exception.Message));
            }
        }

        private void ShowNotice(string text)
        {
            notice = text;
            noticeUntil = Time.unscaledTime + NoticeSeconds;
        }

        private void OnGUI()
        {
            if (notice == null || Time.frameCount == captureFrame || Time.unscaledTime > noticeUntil)
                return;

            noticeStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleRight };
            var area = new Rect(NoticeMargin, Screen.height - NoticeHeight - NoticeMargin,
                Screen.width - 2f * NoticeMargin, NoticeHeight);
            GUI.Label(area, notice, noticeStyle);
        }
    }
}
