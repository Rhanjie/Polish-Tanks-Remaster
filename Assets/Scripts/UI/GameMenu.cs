using PolishTanks.Controls;
using PolishTanks.GameLoop;
using PolishTanks.Missions;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Pause menu and end screen of a gameplay scene. The pause key opens and closes the pause menu, which stops the
    /// game and holds the player's tank. Losing the player's tank or winning the scene's mission brings up the end
    /// screen after a short delay, which waits while another menu holds the game; in a mission it lists the objectives
    /// and the respect earned. The end screen's Restart button takes keyboard focus only once the fire button is let
    /// go, and the end screen holds a tank that is still alive, so a button held for the gun can't restart by accident.
    /// Both screens offer a restart, which reloads the scene, and the main menu (quitting the game when the build has
    /// no main menu scene). <see cref="GameLoopState"/> decides what is shown; this component reads the input, the tank
    /// and the mission and writes to the UI tree.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameMenu : MonoBehaviour
    {
        private const string HiddenClass = "game-menu--hidden";
        private const string VictoryClass = "game-menu--victory";
        private const string RowClass = "summary-row";
        private const string RowTextClass = "summary-row__text";
        private const string RowRespectClass = "summary-row__respect";
        private const string CompletedClass = "summary-row--completed";
        private const string MissedClass = "summary-row--missed";
        private const string TotalClass = "summary-row--total";

        [Tooltip("Seconds between the player's tank blowing up and the end screen.")]
        [SerializeField, Min(0f)] private float defeatDelay = 3f;
        [Tooltip("Seconds between the last objective and the end screen.")]
        [SerializeField, Min(0f)] private float victoryDelay = 2f;
        [Tooltip("Scene the Main menu buttons load.")]
        [SerializeField] private string mainMenuScene = "MainMenu";

        [Inject] private PlayerTank playerTank;
        [Inject] private IPlayerInput input;
        [Inject] private IGamePause gamePause;
        [Inject] private IMission mission;

        private GameLoopState state;
        private VisualElement pauseScreen;
        private VisualElement endScreen;
        private VisualElement summary;
        private Label missionTitle;
        private Button resume;
        private Button pauseRestart;
        private Button pauseMainMenu;
        private Button endRestart;
        private Button endMainMenu;
        private TankController tank;
        private TankInput tankInput;
        private MissionProgress progress;
        private bool focusEndOnTriggerRelease;

        public GameLoopPhase Phase => state != null ? state.Phase : GameLoopPhase.Playing;

        public void TogglePause() => state?.TogglePause();

        private void Awake()
        {
            state = new GameLoopState(defeatDelay, victoryDelay);
            state.PhaseChanged += OnPhaseChanged;
        }

        private void OnEnable()
        {
            var tree = GetComponent<UIDocument>().rootVisualElement;
            pauseScreen = tree.Q("pauseScreen");
            endScreen = tree.Q("endScreen");
            summary = endScreen.Q("summary");
            missionTitle = endScreen.Q<Label>("missionTitle");
            resume = pauseScreen.Q<Button>("resume");
            pauseRestart = pauseScreen.Q<Button>("restart");
            pauseMainMenu = pauseScreen.Q<Button>("mainMenu");
            endRestart = endScreen.Q<Button>("restart");
            endMainMenu = endScreen.Q<Button>("mainMenu");

            resume.clicked += Resume;
            pauseRestart.clicked += Restart;
            pauseMainMenu.clicked += GoToMainMenu;
            endRestart.clicked += Restart;
            endMainMenu.clicked += GoToMainMenu;

            Bind();
            Show(state.Phase);
        }

        private void OnDisable()
        {
            if (resume != null)
            {
                resume.clicked -= Resume;
                pauseRestart.clicked -= Restart;
                pauseMainMenu.clicked -= GoToMainMenu;
                endRestart.clicked -= Restart;
                endMainMenu.clicked -= GoToMainMenu;
            }
            Hold(false, false);
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
            if (tank == null || progress == null)
                Bind();
            if (input != null && input.PausePressedThisFrame)
                state.TogglePause();
            var heldByOthers = state.Phase == GameLoopPhase.Playing && gamePause != null && gamePause.IsPaused;
            state.Tick(heldByOthers ? 0f : Time.unscaledDeltaTime);
            if (focusEndOnTriggerRelease && (input == null || !input.FireHeld))
            {
                focusEndOnTriggerRelease = false;
                endRestart.Focus();
            }
        }

        private void Bind()
        {
            if (progress == null && mission != null && mission.Progress != null)
            {
                progress = mission.Progress;
                progress.Ended += OnMissionEnded;
                if (progress.Outcome == MissionOutcome.Won)
                    state.MissionWon();
            }

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
            if (progress != null)
                progress.Ended -= OnMissionEnded;
            tank = null;
            tankInput = null;
            progress = null;
        }

        private void OnTankDestroyed(TankController _) => state.PlayerDestroyed();

        private void OnMissionEnded(MissionOutcome outcome)
        {
            if (outcome == MissionOutcome.Won)
                state.MissionWon();
        }

        private void OnPhaseChanged(GameLoopPhase phase)
        {
            Hold(phase == GameLoopPhase.Paused, phase != GameLoopPhase.Playing);
            Show(phase);
        }

        private void Hold(bool pause, bool tankControls)
        {
            if (pause)
                gamePause?.Hold(this);
            else
                gamePause?.Release(this);

            if (tankInput == null)
                return;
            if (tankControls)
                tankInput.Hold(this);
            else
                tankInput.Release(this);
        }

        private void Show(GameLoopPhase phase)
        {
            if (pauseScreen == null)
                return;
            var ended = phase == GameLoopPhase.Defeated || phase == GameLoopPhase.Victory;
            pauseScreen.EnableInClassList(HiddenClass, phase != GameLoopPhase.Paused);
            endScreen.EnableInClassList(HiddenClass, !ended);
            endScreen.EnableInClassList(VictoryClass, phase == GameLoopPhase.Victory);
            if (phase == GameLoopPhase.Paused)
                resume.Focus();
            if (!ended)
                return;
            FillSummary();
            focusEndOnTriggerRelease = true;
        }

        private void FillSummary()
        {
            summary.Clear();
            var definition = mission != null ? mission.Definition : null;
            var hasMission = progress != null && definition != null;
            missionTitle.text = hasMission ? definition.Title : string.Empty;
            missionTitle.EnableInClassList(HiddenClass, !hasMission);
            summary.EnableInClassList(HiddenClass, !hasMission);
            if (!hasMission)
                return;

            foreach (var status in progress.Objectives)
            {
                var row = AddRow(MissionText.Describe(status), status.EarnedRespect > 0 ? MissionText.Respect(status.EarnedRespect) : string.Empty);
                row.AddToClassList(status.State == ObjectiveState.Completed ? CompletedClass : MissedClass);
            }
            foreach (var loss in new[] { EnemyLoss.Tank, EnemyLoss.Module, EnemyLoss.Infantry })
            {
                var count = progress.Losses(loss);
                if (count > 0)
                    AddRow(MissionText.LossLine(loss, count), MissionText.Respect(count * definition.Respect.For(loss)));
            }
            AddRow(MissionText.TotalRespect, progress.TotalRespect.ToString()).AddToClassList(TotalClass);
        }

        private VisualElement AddRow(string text, string respect)
        {
            var row = new VisualElement();
            row.AddToClassList(RowClass);
            var label = new Label(text);
            label.AddToClassList(RowTextClass);
            var value = new Label(respect);
            value.AddToClassList(RowRespectClass);
            row.Add(label);
            row.Add(value);
            summary.Add(row);
            return row;
        }

        private void Resume() => state.Resume();

        private void Restart()
        {
            Hold(false, false);
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex >= 0 ? scene.buildIndex : 0);
        }

        private void GoToMainMenu()
        {
            Hold(false, false);
            if (!string.IsNullOrEmpty(mainMenuScene) && Application.CanStreamedLevelBeLoaded(mainMenuScene))
            {
                SceneManager.LoadScene(mainMenuScene);
                return;
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
