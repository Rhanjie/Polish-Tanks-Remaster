using PolishTanks.Controls;
using PolishTanks.GameLoop;
using PolishTanks.Missions;
using PolishTanks.Radio;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Radio messages at the top of the screen: the speaker's portrait in a circle (a stranger's silhouette when they
    /// have none), their name and the message typed out letter by letter. The continue key shows the whole message at
    /// once, and pressed again moves on to the next one. Not typed out and not read while the game is paused; hidden
    /// and emptied once the mission is over.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class RadioPanel : MonoBehaviour
    {
        private const string HiddenClass = "radio--hidden";
        private const string HintHiddenClass = "radio__hint--hidden";
        private const string HiddenTextTag = "<alpha=#00>";

        [Tooltip("Shown for speakers without a portrait.")]
        [SerializeField] private Sprite unknownPortrait;

        [Inject] private RadioChannel radio;
        [Inject] private IPlayerInput input;
        [Inject] private IGamePause pause;
        [Inject] private IMission mission;

        private VisualElement root;
        private VisualElement portrait;
        private Label speaker;
        private Label message;
        private VisualElement continueHint;
        private RadioMessage shown;
        private int shownCharacters = -1;
        private int pausedFrame = -1;

        private void OnEnable()
        {
            var tree = GetComponent<UIDocument>().rootVisualElement;
            root = tree.Q("radio");
            portrait = tree.Q("portrait");
            speaker = tree.Q<Label>("speaker");
            message = tree.Q<Label>("message");
            continueHint = tree.Q("continueHint");
            message.enableRichText = true;
            shown = null;
            shownCharacters = -1;
            root.AddToClassList(HiddenClass);
        }

        private void Update()
        {
            if (radio == null)
                return;

            var progress = mission != null ? mission.Progress : null;
            if (progress != null && progress.Outcome != MissionOutcome.Running)
                radio.Clear();

            if (pause != null && pause.IsPaused)
                pausedFrame = Time.frameCount;
            else
            {
                if (input != null && input.ContinuePressedThisFrame && Time.frameCount > pausedFrame + 1)
                    radio.Confirm();
                radio.Tick(Time.deltaTime);
            }

            Refresh();
        }

        private void Refresh()
        {
            var current = radio.Current;
            if (current != shown)
            {
                shown = current;
                shownCharacters = -1;
                root.EnableInClassList(HiddenClass, current == null);
                if (current == null)
                    return;
                speaker.text = current.Speaker != null ? current.Speaker.DisplayName : string.Empty;
                var face = current.Speaker != null && current.Speaker.Portrait != null ? current.Speaker.Portrait : unknownPortrait;
                portrait.style.backgroundImage = face != null
                    ? new StyleBackground(face)
                    : new StyleBackground(StyleKeyword.None);
            }

            if (current == null || radio.VisibleCharacters == shownCharacters)
                return;
            shownCharacters = radio.VisibleCharacters;
            var text = current.Text;
            message.text = shownCharacters >= text.Length
                ? text
                : text.Substring(0, shownCharacters) + HiddenTextTag + text.Substring(shownCharacters);
            continueHint.EnableInClassList(HintHiddenClass, radio.IsTyping);
        }
    }
}
