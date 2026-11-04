using PolishTanks.Bouncing;
using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>Drives the bounce telemetry panel (BounceHud.uxml) from a <see cref="BounceCounter"/>.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class BounceHud : MonoBehaviour
    {
        private const int CounterDigits = 5;
        private const long DigitTickMs = 140;
        private const long PanelFlashMs = 90;
        private const long StatusBlinkMs = 600;
        private const long UptimeRefreshMs = 100;

        private const string LeadDigitClass = "hud-digit--lead";
        private const string TickDigitClass = "hud-digit--tick";
        private const string PanelFlashClass = "hud-panel--flash";
        private const string StatusDimClass = "hud-status-dot--dim";

        [SerializeField]
        private BounceCounter counter;

        [SerializeField]
        private BounceColorChanger colorChanger;

        [SerializeField, Min(0.1f), Tooltip("Impact speed (m/s) that fills the gauge completely.")]
        private float gaugeMaxSpeed = 20f;

        [SerializeField, Min(0f), Tooltip("How fast the gauge glow fades after an impact (fraction per second).")]
        private float pulseDecayPerSecond = 2.5f;

        private UIDocument _document;
        private bool _bound;

        private VisualElement _panel;
        private VisualElement _statusDot;
        private readonly Label[] _digits = new Label[CounterDigits];
        private Label _speedValue;
        private Label _peakValue;
        private Label _rateValue;
        private Label _intervalValue;
        private Label _uptimeValue;
        private Label _hexValue;
        private RadialGauge _gauge;
        private Sparkline _graph;
        private ColorHistoryStrip _colorStrip;

        private IVisualElementScheduledItem _blink;
        private IVisualElementScheduledItem _uptime;
        private IVisualElementScheduledItem _clearTicks;
        private IVisualElementScheduledItem _clearFlash;

        private int _shownCount = -1;
        private float _pulse;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (counter != null)
                counter.Bounced += OnBounced;
            else
                Debug.LogWarning($"{nameof(BounceHud)} on '{name}' has no {nameof(BounceCounter)} assigned.", this);

            if (colorChanger != null)
                colorChanger.ColorChanged += OnColorChanged;

            TryBind();
        }

        private void OnDisable()
        {
            if (counter != null)
                counter.Bounced -= OnBounced;
            if (colorChanger != null)
                colorChanger.ColorChanged -= OnColorChanged;

            _blink?.Pause();
            _uptime?.Pause();
            _clearTicks?.Pause();
            _clearFlash?.Pause();
            // UIDocument rebuilds its tree when re-enabled, so every reference has to be queried again.
            _bound = false;
        }

        private void Update()
        {
            if (!_bound && !TryBind())
                return;

            if (_pulse <= 0f)
                return;

            _pulse = Mathf.Max(0f, _pulse - pulseDecayPerSecond * Time.deltaTime);
            _gauge.Pulse = _pulse;
        }

        private bool TryBind()
        {
            var root = _document != null ? _document.rootVisualElement : null;
            if (root == null)
                return false;

            _panel = root.Q("panel");
            _statusDot = root.Q("statusDot");
            for (var i = 0; i < CounterDigits; i++)
                _digits[i] = root.Q<Label>("digit" + i);
            _speedValue = root.Q<Label>("speedValue");
            _peakValue = root.Q<Label>("peakValue");
            _rateValue = root.Q<Label>("rateValue");
            _intervalValue = root.Q<Label>("intervalValue");
            _uptimeValue = root.Q<Label>("uptimeValue");
            _hexValue = root.Q<Label>("hexValue");
            _gauge = root.Q<RadialGauge>("speedGauge");
            _graph = root.Q<Sparkline>("impactGraph");
            _colorStrip = root.Q<ColorHistoryStrip>("colorStrip");

            if (_panel == null || _gauge == null || _graph == null || _colorStrip == null || _digits[CounterDigits - 1] == null)
                return false;

            _bound = true;
            _shownCount = -1;

            _blink = _statusDot?.schedule.Execute(() => _statusDot.ToggleInClassList(StatusDimClass)).Every(StatusBlinkMs);
            _uptime = _uptimeValue?.schedule.Execute(() => _uptimeValue.text = HudFormat.Uptime(Time.timeSinceLevelLoad)).Every(UptimeRefreshMs);

            if (colorChanger != null)
                ApplyAccent(colorChanger.TargetColor, pushToHistory: _colorStrip.Count == 0);

            RefreshStats(animate: false);
            return true;
        }

        private void OnBounced(BounceEvent bounce)
        {
            if (!_bound)
                return;

            RefreshStats(animate: true);
            _pulse = 1f;
            _gauge.Pulse = _pulse;

            _panel.AddToClassList(PanelFlashClass);
            _clearFlash?.Pause();
            _clearFlash = _panel.schedule.Execute(() => _panel.RemoveFromClassList(PanelFlashClass)).StartingIn(PanelFlashMs);
        }

        private void OnColorChanged(Color color)
        {
            if (_bound)
                ApplyAccent(color, pushToHistory: true);
        }

        private void ApplyAccent(Color color, bool pushToHistory)
        {
            _gauge.AccentColor = color;
            _graph.AccentColor = color;
            if (pushToHistory)
                _colorStrip.Push(color);
            if (_hexValue != null)
                _hexValue.text = HudFormat.Hex(color);
        }

        private void RefreshStats(bool animate)
        {
            var tracker = counter != null ? counter.Tracker : null;
            var count = tracker?.Count ?? 0;

            UpdateCounter(count, animate);

            if (tracker == null)
                return;

            SetText(_speedValue, HudFormat.Speed(tracker.LastImpactSpeed));
            SetText(_peakValue, HudFormat.Speed(tracker.PeakImpactSpeed));
            SetText(_rateValue, tracker.BouncesPerMinute.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
            SetText(_intervalValue, tracker.LastInterval.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));

            _gauge.Value = Mathf.Clamp01(tracker.LastImpactSpeed / gaugeMaxSpeed);
            _gauge.Peak = Mathf.Clamp01(tracker.PeakImpactSpeed / gaugeMaxSpeed);
            _graph.Capacity = tracker.HistoryCapacity;
            _graph.SetValues(tracker.ImpactHistory, Mathf.Max(gaugeMaxSpeed, tracker.PeakImpactSpeed));
        }

        private void UpdateCounter(int count, bool animate)
        {
            var leadingZeros = HudFormat.LeadingZeros(count, CounterDigits);
            var anyTick = false;

            for (var i = 0; i < CounterDigits; i++)
            {
                var label = _digits[i];
                if (label == null)
                    continue;

                var digit = HudFormat.DigitAt(count, i, CounterDigits);
                var changed = _shownCount < 0 || HudFormat.DigitAt(_shownCount, i, CounterDigits) != digit;
                label.text = HudFormat.Digit(digit);
                label.EnableInClassList(LeadDigitClass, i < leadingZeros);

                if (animate && changed)
                {
                    label.AddToClassList(TickDigitClass);
                    anyTick = true;
                }
            }

            _shownCount = count;

            if (!anyTick)
                return;

            _clearTicks?.Pause();
            _clearTicks = _panel.schedule.Execute(ClearDigitTicks).StartingIn(DigitTickMs);
        }

        private void ClearDigitTicks()
        {
            foreach (var label in _digits)
                label?.RemoveFromClassList(TickDigitClass);
        }

        private static void SetText(Label label, string text)
        {
            if (label != null)
                label.text = text;
        }
    }
}
