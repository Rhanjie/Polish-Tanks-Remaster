using PolishTanks.Controls;
using PolishTanks.GameLoop;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Pause menu and defeat screen of a gameplay scene. The pause key opens and closes the pause menu, which stops the
    /// game and holds the player's tank. When the player's tank is destroyed the defeat screen comes up after a short
    /// delay, which waits while another menu holds the game. Its Restart button takes keyboard focus only once the fire
    /// button is let go, so Enter held for the gun can't restart by accident. Both screens offer a restart, which
    /// reloads the scene, and quitting the game. <see cref="GameLoopState"/> decides what is shown; this component
    /// reads the input and the tank and writes to the UI tree.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameMenu : MonoBehaviour
    {
        private const string HiddenClass = "game-menu--hidden";

        [Tooltip("Seconds between the player's tank blowing up and the defeat screen.")]
        [SerializeField, Min(0f)] private float defeatDelay = 3f;

        [Inject] private PlayerTank playerTank;
        [Inject] private IPlayerInput input;
        [Inject] private IGamePause gamePause;

        private GameLoopState state;
        private VisualElement pauseScreen;
        private VisualElement defeatScreen;
        private Button resume;
        private Button pauseRestart;
        private Button pauseQuit;
        private Button defeatRestart;
        private Button defeatQuit;
        private TankController tank;
        private TankInput tankInput;
        private bool focusDefeatOnTriggerRelease;

        public GameLoopPhase Phase => state != null ? state.Phase : GameLoopPhase.Playing;

        public void TogglePause() => state?.TogglePause();

        private void Awake()
        {
            state = new GameLoopState(defeatDelay);
            state.PhaseChanged += OnPhaseChanged;
        }

        private void OnEnable()
        {
            var tree = GetComponent<UIDocument>().rootVisualElement;
            pauseScreen = tree.Q("pauseScreen");
            defeatScreen = tree.Q("defeatScreen");
            resume = pauseScreen.Q<Button>("resume");
            pauseRestart = pauseScreen.Q<Button>("restart");
            pauseQuit = pauseScreen.Q<Button>("quit");
            defeatRestart = defeatScreen.Q<Button>("restart");
            defeatQuit = defeatScreen.Q<Button>("quit");

            resume.clicked += Resume;
            pauseRestart.clicked += Restart;
            pauseQuit.clicked += Quit;
            defeatRestart.clicked += Restart;
            defeatQuit.clicked += Quit;

            Bind();
            Show(state.Phase);
        }

        private void OnDisable()
        {
            if (resume != null)
            {
                resume.clicked -= Resume;
                pauseRestart.clicked -= Restart;
                pauseQuit.clicked -= Quit;
                defeatRestart.clicked -= Restart;
                defeatQuit.clicked -= Quit;
            }
            Hold(false);
            Unbind();
        }

        private void OnDestroy()
        {
            if (state != null)
                state.PhaseChanged -= OnPhaseChanged;
        }

        private void Start() => Bind();

        private void Update()
        {
            if (tank == null)
                Bind();
            if (input != null && input.PausePressedThisFrame)
                state.TogglePause();
            var heldByOthers = state.Phase == GameLoopPhase.Playing && gamePause != null && gamePause.IsPaused;
            state.Tick(heldByOthers ? 0f : Time.unscaledDeltaTime);
            if (focusDefeatOnTriggerRelease && (input == null || !input.FireHeld))
            {
                focusDefeatOnTriggerRelease = false;
                defeatRestart.Focus();
            }
        }

        private void Bind()
        {
            if (tank != null || playerTank == null)
                return;
            tank = playerTank.Controller;
            tankInput = tank.GetComponent<TankInput>();
            tank.Destroyed += OnTankDestroyed;
            if (tank.IsDestroyed)
                state.PlayerDestroyed();
        }

        private void Unbind()
        {
            if (tank != null)
                tank.Destroyed -= OnTankDestroyed;
            tank = null;
            tankInput = null;
        }

        private void OnTankDestroyed(TankController _) => state.PlayerDestroyed();

        private void OnPhaseChanged(GameLoopPhase phase)
        {
            Hold(phase == GameLoopPhase.Paused);
            Show(phase);
        }

        private void Hold(bool hold)
        {
            if (hold)
            {
                gamePause?.Hold(this);
                if (tankInput != null)
                    tankInput.Hold(this);
            }
            else
            {
                gamePause?.Release(this);
                if (tankInput != null)
                    tankInput.Release(this);
            }
        }

        private void Show(GameLoopPhase phase)
        {
            if (pauseScreen == null)
                return;
            pauseScreen.EnableInClassList(HiddenClass, phase != GameLoopPhase.Paused);
            defeatScreen.EnableInClassList(HiddenClass, phase != GameLoopPhase.Defeated);
            if (phase == GameLoopPhase.Paused)
                resume.Focus();
            else if (phase == GameLoopPhase.Defeated)
                focusDefeatOnTriggerRelease = true;
        }

        private void Resume() => state.Resume();

        private void Restart()
        {
            Hold(false);
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex >= 0 ? scene.buildIndex : 0);
        }

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
