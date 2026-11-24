using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PolishTanks.GameLoop;
using PolishTanks.Tank;
using PolishTanks.Tank.Modules;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Playtest panel for swapping the player's modules while the game runs (F1 by default, in every build while the game is in public beta).
    /// Every slot gets a dropdown plus previous/next buttons that skip parts the hull can't carry; the search box
    /// narrows all slots at once, so a long module list stays usable. Presets load whole loadouts, Random rolls a valid
    /// build, Undo steps back through earlier builds. The stat table shows each stat with its change since the
    /// previous build, green when it helps the crew and red when it hurts. While the panel is open the tank stops
    /// listening to the player, so clicks on the panel don't fire the gun, and the game clock and the camera stop, so
    /// nothing happens while a module is being picked. Changes made from code through
    /// <see cref="TankController.Fit"/> show up here too.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DebugModulePanel : MonoBehaviour
    {
        private const string HiddenClass = "debug-modules--hidden";
        private const string BetterClass = "stat--better";
        private const string WorseClass = "stat--worse";

        [SerializeField] private ModuleCatalog catalog;
        [SerializeField] private Key toggleKey = Key.F1;
        [Tooltip("km/h shown per world unit per second of tank speed, same as the HUD.")]
        [SerializeField, Min(0.1f)] private float kmhPerUnit = 6.3f;
        [SerializeField] private bool startOpen;
        [Tooltip("Stop the game clock and hold the camera while the panel is open.")]
        [SerializeField] private bool pauseWhileOpen = true;

        [Inject] private PlayerTank playerTank;
        [Inject] private IGamePause gamePause;

        private readonly Stack<(TankLoadout asset, LoadoutBuilder copy)> history = new Stack<(TankLoadout asset, LoadoutBuilder copy)>();
        private VisualElement root;
        private VisualElement presets;
        private VisualElement slotList;
        private VisualElement equipmentList;
        private VisualElement statList;
        private Label loadoutName;
        private Label equipmentTitle;
        private Label status;
        private Label overload;
        private Label description;
        private TextField search;
        private Button undo;
        private Button random;
        private Button close;
        private TankController tank;
        private TankInput tankInput;
        private TankStats shownStats;
        private TankStats previousStats;
        private bool isOpen;

        public bool IsOpen => isOpen;

        public void SetOpen(bool open)
        {
            if (root == null || isOpen == open)
                return;
            isOpen = open;
            root.EnableInClassList(HiddenClass, !open);
            HoldTank(open);
            Pause(open && pauseWhileOpen);
            if (open)
                Refresh();
        }

        private void OnEnable()
        {
            var tree = GetComponent<UIDocument>().rootVisualElement;
            root = tree.Q("debugModules");
            presets = tree.Q("presets");
            slotList = tree.Q("slots");
            equipmentList = tree.Q("equipment");
            statList = tree.Q("stats");
            loadoutName = tree.Q<Label>("loadoutName");
            equipmentTitle = tree.Q<Label>("equipmentTitle");
            status = tree.Q<Label>("status");
            overload = tree.Q<Label>("overload");
            description = tree.Q<Label>("description");
            search = tree.Q<TextField>("search");
            undo = tree.Q<Button>("undo");
            random = tree.Q<Button>("random");
            close = tree.Q<Button>("close");

            search.RegisterValueChangedCallback(OnSearchChanged);
            undo.clicked += Undo;
            random.clicked += Randomize;
            close.clicked += Close;

            isOpen = false;
            root.AddToClassList(HiddenClass);
            Bind();
            if (startOpen)
                SetOpen(true);
        }

        private void OnDisable()
        {
            SetOpen(false);
            Unbind();
            if (search == null)
                return;
            search.UnregisterValueChangedCallback(OnSearchChanged);
            undo.clicked -= Undo;
            random.clicked -= Randomize;
            close.clicked -= Close;
        }

        private void OnSearchChanged(ChangeEvent<string> _) => Refresh();

        private void Close() => SetOpen(false);

        private void Start() => Bind();

        private void Update()
        {
            if (tank == null)
                Bind();

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[toggleKey].wasPressedThisFrame)
                SetOpen(!isOpen);
        }

        private void Bind()
        {
            if (tank != null || playerTank == null)
                return;
            tank = playerTank.Controller;
            tankInput = tank.GetComponent<TankInput>();
            tank.StatsApplied += OnStatsApplied;
            shownStats = tank.Stats;
            BuildPresets();
            if (isOpen)
            {
                HoldTank(true);
                Pause(pauseWhileOpen);
                Refresh();
            }
        }

        private void Unbind()
        {
            if (tank != null)
                tank.StatsApplied -= OnStatsApplied;
            tank = null;
            tankInput = null;
        }

        private void Pause(bool pause)
        {
            if (gamePause == null)
                return;
            if (pause)
                gamePause.Hold(this);
            else
                gamePause.Release(this);
        }

        private void HoldTank(bool hold)
        {
            if (tankInput == null)
                return;
            if (hold)
                tankInput.Hold(this);
            else
                tankInput.Release(this);
        }

        private void OnStatsApplied(TankStats stats)
        {
            previousStats = shownStats;
            shownStats = stats;
            if (isOpen)
                Refresh();
        }

        private void Change(System.Action<LoadoutBuilder> change, TankModule fitted = null)
        {
            if (tank == null)
                return;
            var snapshot = Snapshot();
            var builder = new LoadoutBuilder(tank.Loadout);
            change(builder);
            var errors = builder.Validate();
            if (errors.Count > 0)
            {
                ShowStatus(string.Join(" ", errors), true);
                return;
            }

            if (!tank.Rebuild(builder))
            {
                ShowStatus(ModulePanelText.BuildFailed, true);
                return;
            }
            history.Push(snapshot);
            ShowStatus(string.Empty, false);
            ShowDescription(fitted);
        }

        private void ApplyPreset(TankLoadout loadout)
        {
            if (tank == null)
                return;
            var snapshot = Snapshot();
            if (!tank.Apply(loadout))
            {
                ShowStatus(string.Format(ModulePanelText.PresetInvalidFormat, loadout.DisplayName), true);
                return;
            }
            history.Push(snapshot);
            ShowStatus(string.Empty, false);
            ShowDescription(null);
        }

        private void Undo()
        {
            if (tank == null || history.Count == 0)
                return;
            var (asset, copy) = history.Pop();
            if (asset != null)
                tank.Apply(asset);
            else
                tank.Rebuild(copy);
            ShowDescription(null);
        }

        private (TankLoadout asset, LoadoutBuilder copy) Snapshot()
        {
            var loadout = tank.Loadout;
            if (loadout == null)
                return (null, new LoadoutBuilder(null));
            return loadout.IsRuntime ? (null, new LoadoutBuilder(loadout)) : (loadout, null);
        }

        private void Randomize()
        {
            if (catalog == null)
                return;
            Change(builder =>
            {
                foreach (var slot in LoadoutBuilder.FixedSlots)
                {
                    var options = catalog.ForSlot(slot).Where(builder.CanFit).ToList();
                    var skip = !LoadoutBuilder.IsRequired(slot) && Random.value < 1f / (options.Count + 1);
                    if (skip || options.Count == 0)
                        builder.Remove(slot);
                    else
                        builder.Fit(options[Random.Range(0, options.Count)]);
                }
                builder.Remove(ModuleSlot.Equipment);
                foreach (var item in catalog.ForSlot(ModuleSlot.Equipment).OrderBy(_ => Random.value))
                    if (builder.CanFit(item) && Random.value < 0.5f)
                        builder.Fit(item);
            });
        }

        private void BuildPresets()
        {
            presets.Clear();
            if (catalog == null)
                return;
            foreach (var loadout in catalog.Loadouts)
            {
                var preset = loadout;
                var button = new Button(() => ApplyPreset(preset)) { text = preset.DisplayName };
                button.AddToClassList("debug-modules__button");
                presets.Add(button);
            }
        }

        private void Refresh()
        {
            if (root == null || tank == null || tank.Loadout == null)
                return;
            var current = new LoadoutBuilder(tank.Loadout);
            var filter = search.value != null ? search.value.Trim() : string.Empty;

            var title = tank.Loadout.DisplayName;
            if (tank.Loadout.IsRuntime)
                title = string.Format(ModulePanelText.ModifiedLoadoutFormat, title);
            if (tank.IsDestroyed)
                title = string.Format(ModulePanelText.WreckFormat, title);
            loadoutName.text = title;
            undo.SetEnabled(history.Count > 0);

            slotList.Clear();
            foreach (var slot in LoadoutBuilder.FixedSlots)
                AddSlotRow(slot, current, filter);
            RefreshEquipment(current, filter);
            RefreshStats();
        }

        private void AddSlotRow(ModuleSlot slot, LoadoutBuilder current, string filter)
        {
            var fitted = current.Get(slot);
            var slotMatches = Matches(ModulePanelText.SlotName(slot), filter);
            var options = Options(slot)
                .Where(module => slotMatches || module == fitted || Matches(module, filter))
                .ToList();
            if (!slotMatches && !options.Any(module => module != fitted && module != null))
                return;
            if (!LoadoutBuilder.IsRequired(slot))
                options.Insert(0, null);
            if (!options.Contains(fitted))
                options.Insert(0, fitted);

            var row = new VisualElement();
            row.AddToClassList("slot-row");
            var label = new Label(ModulePanelText.SlotName(slot));
            label.AddToClassList("slot-row__label");
            row.Add(label);

            var previous = new Button(() => Step(slot, options, current, -1)) { text = "◀" };
            previous.AddToClassList("slot-row__step");
            row.Add(previous);

            var choices = options.Select(module => Caption(module, current)).ToList();
            var dropdown = new DropdownField(choices, options.IndexOf(fitted));
            dropdown.AddToClassList("slot-row__dropdown");
            dropdown.RegisterValueChangedCallback(_ => Pick(slot, options[dropdown.index], current));
            row.Add(dropdown);

            var next = new Button(() => Step(slot, options, current, 1)) { text = "▶" };
            next.AddToClassList("slot-row__step");
            row.Add(next);

            slotList.Add(row);
        }

        private void RefreshEquipment(LoadoutBuilder current, string filter)
        {
            equipmentList.Clear();
            var hull = current.Hull;
            var capacity = hull != null ? hull.EquipmentSlots : 0;
            equipmentTitle.text = string.Format(ModulePanelText.EquipmentTitleFormat, current.Equipment.Count, capacity);

            var slotMatches = Matches(ModulePanelText.SlotName(ModuleSlot.Equipment), filter);
            foreach (var module in Options(ModuleSlot.Equipment))
            {
                var item = (EquipmentModule)module;
                var fitted = current.IsFitted(item);
                if (!slotMatches && !fitted && !Matches(item, filter))
                    continue;
                var toggle = new Toggle(Caption(item, current)) { value = fitted };
                toggle.AddToClassList("equipment-toggle");
                toggle.SetEnabled(fitted || current.CanFit(item));
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        Change(builder => builder.Fit(item), item);
                    else
                        Change(builder => builder.Remove(item));
                });
                equipmentList.Add(toggle);
            }
        }

        private void RefreshStats()
        {
            statList.Clear();
            var stats = tank.Stats;
            overload.EnableInClassList(HiddenClass, stats == null || !stats.IsOverloaded);

            foreach (var line in LoadoutStatSheet.Build(stats, previousStats, kmhPerUnit))
            {
                var row = new VisualElement();
                row.AddToClassList("stat-row");
                var label = new Label(line.Label);
                label.AddToClassList("stat-row__label");
                var value = new Label(line.Value);
                value.AddToClassList("stat-row__value");
                var delta = new Label(line.Delta);
                delta.AddToClassList("stat-row__delta");
                delta.EnableInClassList(BetterClass, line.Change == StatChange.Better);
                delta.EnableInClassList(WorseClass, line.Change == StatChange.Worse);
                row.Add(label);
                row.Add(value);
                row.Add(delta);
                statList.Add(row);
            }
        }

        private void Pick(ModuleSlot slot, TankModule module, LoadoutBuilder current)
        {
            if (module == current.Get(slot))
                return;
            if (module == null)
            {
                Change(builder => builder.Remove(slot));
                return;
            }
            if (!current.CanFit(module))
            {
                ShowStatus(string.Format(ModulePanelText.DoesNotFitHullFormat, module.DisplayName, current.Hull.DisplayName), true);
                Refresh();
                return;
            }
            Change(builder => builder.Fit(module), module);
        }

        private void Step(ModuleSlot slot, List<TankModule> options, LoadoutBuilder current, int direction)
        {
            var start = options.IndexOf(current.Get(slot));
            for (var offset = 1; offset < options.Count; offset++)
            {
                var index = ((start + direction * offset) % options.Count + options.Count) % options.Count;
                var candidate = options[index];
                if (candidate == null || current.CanFit(candidate))
                {
                    Pick(slot, candidate, current);
                    return;
                }
            }
        }

        private IEnumerable<TankModule> Options(ModuleSlot slot)
        {
            return catalog != null ? catalog.ForSlot(slot) : Enumerable.Empty<TankModule>();
        }

        private static string Caption(TankModule module, LoadoutBuilder current)
        {
            if (module == null)
                return ModulePanelText.NoModule;
            var caption = string.Format(CultureInfo.InvariantCulture, ModulePanelText.ModuleCaptionFormat, module.DisplayName, module.Tier, module.Mass / 1000f);
            return current.CanFit(module) ? caption : string.Format(ModulePanelText.DoesNotFitFormat, caption);
        }

        private static bool Matches(TankModule module, string filter)
        {
            return module != null && (Matches(module.DisplayName, filter) || Matches(module.name, filter));
        }

        private static bool Matches(string text, string filter)
        {
            return string.IsNullOrEmpty(filter) || text.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ShowStatus(string message, bool isError)
        {
            status.text = message;
            status.EnableInClassList("debug-modules__status--error", isError);
            status.EnableInClassList(HiddenClass, string.IsNullOrEmpty(message));
        }

        private void ShowDescription(TankModule module)
        {
            var text = module != null && !string.IsNullOrWhiteSpace(module.Description)
                ? string.Format(ModulePanelText.ModuleDescriptionFormat, module.DisplayName, module.Description)
                : string.Empty;
            description.text = text;
            description.EnableInClassList(HiddenClass, string.IsNullOrEmpty(text));
        }
    }
}
