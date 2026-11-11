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
    /// Damage panel for the player's tank: a top-down icon whose tracks, engine, turret ring and vision blocks turn
    /// red when destroyed and orange when patched, a flashing flame while the engine burns, and the repair key with
    /// what pressing it does now plus the progress of the part being patched. <see cref="DamageHudPresenter"/> decides
    /// what to show; this component only reads the tank and writes to the UI Toolkit tree.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DamageHud : MonoBehaviour
    {
        private const int MaxVisionBlocks = 6;
        private const string HiddenClass = "hud--hidden";
        private const string BurningClass = "fire--burning";
        private const string DimClass = "fire--dim";
        private const string AbsentBlockClass = "vision-block--absent";
        private const string RepairAvailableClass = "repair--available";
        private const string ProgressActiveClass = "progress--active";

        [Tooltip("Seconds between flame flashes while the engine burns.")]
        [SerializeField, Min(0.05f)] private float fireFlashInterval = 0.3f;

        [Inject] private PlayerTank playerTank;
        [Inject] private IPlayerInput input;

        private readonly Dictionary<PartId, VisualElement> partElements = new Dictionary<PartId, VisualElement>();
        private readonly VisualElement[] visionBlocks = new VisualElement[MaxVisionBlocks];
        private VisualElement root;
        private VisualElement fire;
        private VisualElement repair;
        private VisualElement progress;
        private VisualElement progressFill;
        private Label repairKey;
        private Label repairCaption;
        private Label repairPart;
        private ModuleDamage damage;
        private FieldRepair fieldRepair;
        private TankController controller;
        private float flashTimer;
        private bool flameDim;

        private void OnEnable()
        {
            var tree = GetComponent<UIDocument>().rootVisualElement;
            root = tree.Q("damageHud");
            fire = tree.Q("fire");
            repair = tree.Q("repair");
            progress = tree.Q("progress");
            progressFill = tree.Q("progressFill");
            repairKey = tree.Q<Label>("repairKey");
            repairCaption = tree.Q<Label>("repairCaption");
            repairPart = tree.Q<Label>("repairPart");
            for (var i = 0; i < MaxVisionBlocks; i++)
                visionBlocks[i] = tree.Q("visionBlock" + i);

            partElements.Clear();
            partElements[new PartId(VehiclePart.Engine)] = tree.Q("engine");
            partElements[new PartId(VehiclePart.LeftTrack)] = tree.Q("leftTrack");
            partElements[new PartId(VehiclePart.RightTrack)] = tree.Q("rightTrack");
            partElements[new PartId(VehiclePart.TurretRing)] = tree.Q("turretRing");
            for (var i = 0; i < MaxVisionBlocks; i++)
                partElements[new PartId(VehiclePart.VisionBlock, i)] = visionBlocks[i];

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
            if (damage == null)
            {
                Bind();
                if (damage == null)
                    return;
            }

            UpdateFlame();
            if (fieldRepair != null && fieldRepair.IsRepairing)
                progressFill.style.width = Length.Percent(fieldRepair.Progress * 100f);
        }

        private void Bind()
        {
            if (damage != null || playerTank == null)
                return;
            controller = playerTank.Controller;
            damage = controller.GetComponent<ModuleDamage>();
            fieldRepair = playerTank.Controller.GetComponent<FieldRepair>();
            if (damage == null)
                return;
            damage.PartChanged += OnPartChanged;
            damage.FireChanged += OnFireChanged;
            damage.LayoutChanged += Refresh;
            if (fieldRepair != null)
                fieldRepair.Changed += Refresh;
            controller.Destroyed += OnTankDestroyed;
        }

        private void Unbind()
        {
            if (damage != null)
            {
                damage.PartChanged -= OnPartChanged;
                damage.FireChanged -= OnFireChanged;
                damage.LayoutChanged -= Refresh;
            }
            if (fieldRepair != null)
                fieldRepair.Changed -= Refresh;
            if (controller != null)
                controller.Destroyed -= OnTankDestroyed;
            damage = null;
            controller = null;
            fieldRepair = null;
        }

        private void OnPartChanged(PartId _, PartCondition __) => Refresh();

        private void OnFireChanged(bool _) => Refresh();

        private void OnTankDestroyed(TankController _) => Refresh();

        private void Refresh()
        {
            if (root == null)
                return;
            var model = damage != null ? damage.Model : null;
            var burning = damage != null && damage.IsBurning;
            root.EnableInClassList(HiddenClass, !DamageHudPresenter.IsVisible(model, burning));
            if (model == null)
                return;

            foreach (var pair in partElements)
                ApplyCondition(pair.Value, model.ConditionOf(pair.Key));
            for (var i = 0; i < MaxVisionBlocks; i++)
                visionBlocks[i].EnableInClassList(AbsentBlockClass, i >= model.VisionBlocks);
            partElements[new PartId(VehiclePart.LeftTrack)].visible = model.Has(new PartId(VehiclePart.LeftTrack));
            partElements[new PartId(VehiclePart.RightTrack)].visible = model.Has(new PartId(VehiclePart.RightTrack));
            partElements[new PartId(VehiclePart.TurretRing)].visible = model.Has(new PartId(VehiclePart.TurretRing));

            fire.EnableInClassList(BurningClass, burning);
            RefreshRepair();
        }

        private void RefreshRepair()
        {
            var repairing = fieldRepair != null && fieldRepair.IsRepairing;
            var caption = DamageHudPresenter.RepairCaption(repairing, fieldRepair != null && fieldRepair.HasWork);
            repair.EnableInClassList(RepairAvailableClass, caption != null);
            progress.EnableInClassList(ProgressActiveClass, repairing);
            if (caption == null)
                return;

            repairKey.text = input != null ? input.RepairKeyLabel : "R";
            repairCaption.text = caption;
            repairPart.text = repairing && fieldRepair.Current.HasValue ? DamageHudPresenter.PartName(fieldRepair.Current.Value) : string.Empty;
            progressFill.style.width = Length.Percent(repairing ? fieldRepair.Progress * 100f : 0f);
        }

        private void UpdateFlame()
        {
            if (!damage.IsBurning)
            {
                if (flameDim)
                    SetFlameDim(false);
                return;
            }
            flashTimer += Time.deltaTime;
            if (flashTimer < fireFlashInterval)
                return;
            flashTimer = 0f;
            SetFlameDim(!flameDim);
        }

        private void SetFlameDim(bool dim)
        {
            flameDim = dim;
            fire.EnableInClassList(DimClass, dim);
        }

        private static void ApplyCondition(VisualElement element, PartCondition condition)
        {
            var active = DamageHudPresenter.PartClass(condition);
            element.EnableInClassList(DamageHudPresenter.IntactClass, active == DamageHudPresenter.IntactClass);
            element.EnableInClassList(DamageHudPresenter.DestroyedClass, active == DamageHudPresenter.DestroyedClass);
            element.EnableInClassList(DamageHudPresenter.PatchedClass, active == DamageHudPresenter.PatchedClass);
        }
    }
}
