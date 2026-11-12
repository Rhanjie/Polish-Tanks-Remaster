using System.Collections.Generic;
using PolishTanks.Controls;
using PolishTanks.Damage;
using PolishTanks.Tank;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Round HUD for the player's tank. The <see cref="HudDial"/> face carries the speedometer and the reload arcs of
    /// the main gun and the machine gun; inside it a top-down drawing of the tank shows part damage: thrown tracks
    /// pulse red and hang a loose end, patched parts turn orange, the engine deck flickers while it burns. Next to the
    /// dial sit the repair popup (only while something can be patched) and the ammunition popup.
    /// <see cref="TankHudPresenter"/> decides what to show; this component reads the tank and writes to the UI tree.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TankHud : MonoBehaviour
    {
        private const int MaxVisionBlocks = 6;
        private const int TrackLinks = 14;
        private const string BurningClass = "fire--burning";
        private const string PulseClass = "hud--pulse";
        private const string TrackLinkClass = "track-link";
        private const string AbsentBlockClass = "vision-block--absent";
        private const string RepairAvailableClass = "repair--available";
        private const string ProgressActiveClass = "progress--active";
        private const string LoadingSwatchClass = "ammo-swatch--loading";

        [Tooltip("Seconds between pulse steps of destroyed parts and the engine fire.")]
        [SerializeField, Min(0.05f)] private float pulseInterval = 0.35f;

        [Tooltip("km/h shown per world unit per second of tank speed.")]
        [SerializeField, Min(0.1f)] private float kmhPerUnit = 6.3f;

        [Inject] private PlayerTank playerTank;
        [Inject] private IPlayerInput input;

        private readonly Dictionary<PartId, VisualElement> partElements = new Dictionary<PartId, VisualElement>();
        private readonly VisualElement[] visionBlocks = new VisualElement[MaxVisionBlocks];
        private VisualElement root;
        private HudDial dial;
        private VisualElement fire;
        private VisualElement repair;
        private VisualElement progress;
        private VisualElement progressFill;
        private VisualElement gunSwatch;
        private VisualElement beltSwatch;
        private Label speedValue;
        private Label repairKey;
        private Label repairCaption;
        private Label repairPart;
        private Label gunState;
        private Label beltState;
        private ModuleDamage damage;
        private FieldRepair fieldRepair;
        private TankController controller;
        private TankMovement movement;
        private TankGun mainGun;
        private TankGun machineGun;
        private float pulseTimer;
        private int shownKmh = -1;
        private string shownGun;
        private string shownBelt;

        private void OnEnable()
        {
            var tree = GetComponent<UIDocument>().rootVisualElement;
            root = tree.Q("tankHud");
            dial = tree.Q<HudDial>("dial");
            fire = tree.Q("fire");
            repair = tree.Q("repair");
            progress = tree.Q("progress");
            progressFill = tree.Q("progressFill");
            speedValue = tree.Q<Label>("speedValue");
            repairKey = tree.Q<Label>("repairKey");
            repairCaption = tree.Q<Label>("repairCaption");
            repairPart = tree.Q<Label>("repairPart");
            gunState = tree.Q<Label>("gunState");
            beltState = tree.Q<Label>("beltState");
            gunSwatch = tree.Q(className: "ammo-swatch--gun");
            beltSwatch = tree.Q(className: "ammo-swatch--belt");
            for (var i = 0; i < MaxVisionBlocks; i++)
                visionBlocks[i] = tree.Q("visionBlock" + i);
            AddTrackLinks(tree.Q("leftTrackLinks"));
            AddTrackLinks(tree.Q("rightTrackLinks"));

            partElements.Clear();
            partElements[new PartId(VehiclePart.Engine)] = tree.Q("engine");
            partElements[new PartId(VehiclePart.LeftTrack)] = tree.Q("leftTrack");
            partElements[new PartId(VehiclePart.RightTrack)] = tree.Q("rightTrack");
            partElements[new PartId(VehiclePart.TurretRing)] = tree.Q("turretRing");
            for (var i = 0; i < MaxVisionBlocks; i++)
                partElements[new PartId(VehiclePart.VisionBlock, i)] = visionBlocks[i];

            shownKmh = -1;
            shownGun = null;
            shownBelt = null;
            Bind();
            Refresh();
        }

        private void Start()
        {
            Bind();
            Refresh();
        }

        private void OnDisable() => Unbind();

        private void Update()
        {
            if (root == null)
                return;
            if (controller == null)
            {
                Bind();
                if (controller == null)
                    return;
            }

            Pulse();
            UpdateSpeed();
            UpdateWeapons();
            if (fieldRepair != null && fieldRepair.IsRepairing)
                progressFill.style.width = Length.Percent(fieldRepair.Progress * 100f);
        }

        private void Bind()
        {
            if (controller != null || playerTank == null)
                return;
            controller = playerTank.Controller;
            movement = controller.GetComponent<TankMovement>();
            damage = controller.GetComponent<ModuleDamage>();
            fieldRepair = controller.GetComponent<FieldRepair>();
            mainGun = TankGun.Find(controller.gameObject, GunMount.Main);
            machineGun = TankGun.Find(controller.gameObject, GunMount.Coaxial);
            controller.Destroyed += OnTankDestroyed;
            if (damage != null)
            {
                damage.PartChanged += OnPartChanged;
                damage.FireChanged += OnFireChanged;
                damage.LayoutChanged += Refresh;
            }
            if (fieldRepair != null)
                fieldRepair.Changed += Refresh;
        }

        private void Unbind()
        {
            if (controller != null)
                controller.Destroyed -= OnTankDestroyed;
            if (damage != null)
            {
                damage.PartChanged -= OnPartChanged;
                damage.FireChanged -= OnFireChanged;
                damage.LayoutChanged -= Refresh;
            }
            if (fieldRepair != null)
                fieldRepair.Changed -= Refresh;
            controller = null;
            movement = null;
            damage = null;
            fieldRepair = null;
            mainGun = null;
            machineGun = null;
        }

        private void OnPartChanged(PartId _, PartCondition __) => Refresh();

        private void OnFireChanged(bool _) => Refresh();

        private void OnTankDestroyed(TankController _) => Refresh();

        private void Refresh()
        {
            if (root == null)
                return;
            var model = damage != null ? damage.Model : null;
            if (model != null)
            {
                foreach (var pair in partElements)
                    ApplyCondition(pair.Value, model.ConditionOf(pair.Key));
                for (var i = 0; i < MaxVisionBlocks; i++)
                    visionBlocks[i].EnableInClassList(AbsentBlockClass, i >= model.VisionBlocks);
                partElements[new PartId(VehiclePart.LeftTrack)].visible = model.Has(new PartId(VehiclePart.LeftTrack));
                partElements[new PartId(VehiclePart.RightTrack)].visible = model.Has(new PartId(VehiclePart.RightTrack));
                partElements[new PartId(VehiclePart.TurretRing)].visible = model.Has(new PartId(VehiclePart.TurretRing));
            }

            fire.EnableInClassList(BurningClass, damage != null && damage.IsBurning);
            RefreshRepair();
        }

        private void RefreshRepair()
        {
            var repairing = fieldRepair != null && fieldRepair.IsRepairing;
            var caption = TankHudPresenter.RepairCaption(repairing, fieldRepair != null && fieldRepair.HasWork);
            repair.EnableInClassList(RepairAvailableClass, caption != null);
            progress.EnableInClassList(ProgressActiveClass, repairing);
            if (caption == null)
                return;

            repairKey.text = input != null ? input.RepairKeyLabel : "R";
            repairCaption.text = caption;
            repairPart.text = repairing && fieldRepair.Current.HasValue ? TankHudPresenter.PartName(fieldRepair.Current.Value) : string.Empty;
            progressFill.style.width = Length.Percent(repairing ? fieldRepair.Progress * 100f : 0f);
        }

        private void UpdateSpeed()
        {
            if (movement == null)
                return;
            var speed = movement.CurrentSpeed;
            var gaugeMax = TankHudPresenter.GaugeMaxKmh(movement.Settings.maxForwardSpeed, kmhPerUnit);
            dial.SetSpeed(TankHudPresenter.GaugeFill(speed, kmhPerUnit, gaugeMax), speed < 0f, gaugeMax);
            var kmh = TankHudPresenter.SpeedKmh(speed, kmhPerUnit);
            if (kmh == shownKmh)
                return;
            shownKmh = kmh;
            speedValue.text = kmh.ToString();
        }

        private void UpdateWeapons()
        {
            var gunFill = mainGun != null ? mainGun.ReloadProgress : 1f;
            var beltSize = machineGun != null ? machineGun.Settings.magazineSize : 0;
            var rounds = machineGun != null ? machineGun.RoundsInMagazine : 0;
            var changing = machineGun != null && machineGun.IsChangingMagazine;
            var beltFill = changing ? machineGun.MagazineReloadProgress : TankHudPresenter.BeltFill(rounds, beltSize);
            dial.SetWeapons(gunFill, beltFill, changing);

            var gunText = gunFill >= 1f ? "Gotowe" : "Ładowanie";
            if (gunText != shownGun)
            {
                shownGun = gunText;
                gunState.text = gunText;
                gunSwatch.EnableInClassList(LoadingSwatchClass, gunFill < 1f);
            }

            var beltText = changing ? "Taśma" : TankHudPresenter.BeltText(rounds, beltSize);
            if (beltText != shownBelt)
            {
                shownBelt = beltText;
                beltState.text = beltText;
                beltSwatch.EnableInClassList(LoadingSwatchClass, changing);
            }
        }

        private void Pulse()
        {
            pulseTimer += Time.deltaTime;
            if (pulseTimer < pulseInterval)
                return;
            pulseTimer = 0f;
            root.ToggleInClassList(PulseClass);
        }

        private static void AddTrackLinks(VisualElement track)
        {
            if (track == null || track.childCount > 0)
                return;
            for (var i = 0; i < TrackLinks; i++)
            {
                var link = new VisualElement { pickingMode = PickingMode.Ignore };
                link.AddToClassList(TrackLinkClass);
                track.Add(link);
            }
        }

        private static void ApplyCondition(VisualElement element, PartCondition condition)
        {
            var active = TankHudPresenter.PartClass(condition);
            element.EnableInClassList(TankHudPresenter.IntactClass, active == TankHudPresenter.IntactClass);
            element.EnableInClassList(TankHudPresenter.DestroyedClass, active == TankHudPresenter.DestroyedClass);
            element.EnableInClassList(TankHudPresenter.PatchedClass, active == TankHudPresenter.PatchedClass);
        }
    }
}
