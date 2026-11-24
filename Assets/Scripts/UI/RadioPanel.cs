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
    /// have none), their name and the message typed out letter by letter. Every new message flashes the panel and
    /// plays its recording, or a radio chirp without one; the portrait frame lights up while the speaker talks and a
    /// bar runs down the time left before the message closes by itself. The continue key shows the whole message at
    /// once and, pressed again, moves on early. Not typed out and not read while the game is paused; hidden and
    /// emptied once the mission is over.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class RadioPanel : MonoBehaviour
    {
        private const string HiddenClass = "radio--hidden";
        private const string FlashClass = "radio--flash";
        private const string SpeakingClass = "radio__portrait-frame--speaking";
        private const string HintHiddenClass = "radio__hint--hidden";
        private const string HiddenTextTag = "<alpha=#00>";

        [Tooltip("Shown for speakers without a portrait.")]
        [SerializeField] private Sprite unknownPortrait;
        [Tooltip("Played when a message without a recording comes up.")]
        [SerializeField] private AudioClip arrivalSound;
        [Tooltip("Optional. Plays the arrival sound and the recordings.")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Seconds the panel stays lit up after a new message comes up.")]
        [SerializeField, Min(0f)] private float flashTime = 0.15f;

        [Inject] private RadioChannel radio;
        [Inject] private IPlayerInput input;
        [Inject] private IGamePause pause;
        [Inject] private IMission mission;

        private VisualElement root;
        private VisualElement portraitFrame;
        private VisualElement portrait;
        private Label speaker;
        private Label message;
        private VisualElement continueHint;
        private VisualElement timer;
        private RadioMessage shown;
        private int shownCharacters = -1;
        private int pausedFrame = -1;
        private float flashUntil;

        private void OnEnable()
        {
            var tree = GetComponent<UIDocument>().rootVisualElement;
            root = tree.Q("radio");
            portraitFrame = tree.Q("portraitFrame");
            portrait = tree.Q("portrait");
            speaker = tree.Q<Label>("speaker");
            message = tree.Q<Label>("message");
            continueHint = tree.Q("continueHint");
            timer = tree.Q("timer");
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
                if (audioSource != null)
                    audioSource.Stop();
                if (current == null)
                    return;
                speaker.text = current.Speaker != null ? current.Speaker.DisplayName : string.Empty;
                var face = current.Speaker != null && current.Speaker.Portrait != null ? current.Speaker.Portrait : unknownPortrait;
                portrait.style.backgroundImage = face != null
                    ? new StyleBackground(face)
                    : new StyleBackground(StyleKeyword.None);
                Announce(current);
            }

            root.EnableInClassList(FlashClass, Time.unscaledTime < flashUntil);
            if (current == null)
                return;
            var speaking = radio.IsTyping || (audioSource != null && audioSource.isPlaying && current.Voice != null);
            portraitFrame.EnableInClassList(SpeakingClass, speaking && Mathf.FloorToInt(Time.unscaledTime * 5f) % 2 == 0);
            continueHint.EnableInClassList(HintHiddenClass, radio.IsTyping);
            timer.style.width = Length.Percent(radio.TimeLeftShare * 100f);
            if (radio.VisibleCharacters == shownCharacters)
                return;
            shownCharacters = radio.VisibleCharacters;
            var text = current.Text;
            message.text = shownCharacters >= text.Length
                ? text
                : text.Substring(0, shownCharacters) + HiddenTextTag + text.Substring(shownCharacters);
        }

        private void Announce(RadioMessage current)
        {
            flashUntil = Time.unscaledTime + flashTime;
            if (audioSource == null)
                return;
            var clip = current.Voice != null ? current.Voice : arrivalSound;
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
