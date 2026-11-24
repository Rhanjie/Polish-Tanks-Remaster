using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Temporary main menu until the real one: starts the tutorial mission or the sandbox, or quits the game.
    /// The first button takes focus so a keyboard or gamepad works without a mouse. The web build hides Quit,
    /// since a browser tab can't be closed by the game.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private string tutorialScene = "Mission_00_Tutorial";
        [SerializeField] private string sandboxScene = "TankSandbox";

        private Button tutorial;
        private Button sandbox;
        private Button quit;

        private void OnEnable()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            var tree = GetComponent<UIDocument>().rootVisualElement;
            tutorial = tree.Q<Button>("tutorial");
            sandbox = tree.Q<Button>("sandbox");
            quit = tree.Q<Button>("quit");
            tutorial.clicked += StartTutorial;
            sandbox.clicked += StartSandbox;
            quit.clicked += Quit;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                quit.style.display = DisplayStyle.None;
            tutorial.Focus();
        }

        private void OnDisable()
        {
            if (tutorial == null)
                return;
            tutorial.clicked -= StartTutorial;
            sandbox.clicked -= StartSandbox;
            quit.clicked -= Quit;
        }

        private void StartTutorial() => SceneManager.LoadScene(tutorialScene);

        private void StartSandbox() => SceneManager.LoadScene(sandboxScene);

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
