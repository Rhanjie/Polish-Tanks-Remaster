using System.Collections.Generic;
using PolishTanks.Missions;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Objective tracker in the top corner: the mission title, the current primary objective with its count or timer,
    /// and the open optional ones. When an objective is done or a new one opens, a short notice slides in under it;
    /// notices raised together (one objective done, the next one open) are shown one after another.
    /// Hidden in scenes without a mission.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MissionHud : MonoBehaviour
    {
        private const string HiddenClass = "mission-hud--hidden";
        private const string NoticeVisibleClass = "mission-notice--visible";
        private const string ObjectiveClass = "mission-objective";
        private const string OptionalClass = "mission-objective--optional";
        private const string CompletedClass = "mission-objective--completed";

        [Tooltip("Seconds a notice stays on screen.")]
        [SerializeField, Min(0.5f)] private float noticeTime = 3.5f;

        [Inject] private IMission mission;

        private readonly List<(ObjectiveStatus status, Label label)> rows = new List<(ObjectiveStatus, Label)>();
        private readonly Queue<(string caption, string text)> notices = new Queue<(string, string)>();
        private readonly HashSet<ObjectiveStatus> announced = new HashSet<ObjectiveStatus>();
        private VisualElement root;
        private Label title;
        private VisualElement objectiveList;
        private VisualElement notice;
        private Label noticeCaption;
        private Label noticeText;
        private MissionProgress progress;
        private float noticeTimer;

        private void OnEnable()
        {
            var tree = GetComponent<UIDocument>().rootVisualElement;
            root = tree.Q("missionHud");
            title = tree.Q<Label>("missionTitle");
            objectiveList = tree.Q("objectives");
            notice = tree.Q("notice");
            noticeCaption = tree.Q<Label>("noticeCaption");
            noticeText = tree.Q<Label>("noticeText");
            root.AddToClassList(HiddenClass);
            Bind();
        }

        private void OnDisable()
        {
            if (progress != null)
            {
                progress.ObjectiveChanged -= OnObjectiveChanged;
                progress.Ended -= OnEnded;
            }
            progress = null;
        }

        private void Update()
        {
            if (progress == null)
            {
                Bind();
                return;
            }

            foreach (var (status, label) in rows)
            {
                if (status.Objective.Kind != MissionObjectiveKind.Survive || status.State != ObjectiveState.Active)
                    continue;
                var text = MissionText.Describe(status);
                if (label.text != text)
                    label.text = text;
            }

            if (noticeTimer > 0f)
            {
                noticeTimer -= Time.unscaledDeltaTime;
                if (noticeTimer <= 0f)
                    notice.RemoveFromClassList(NoticeVisibleClass);
            }
            else if (notices.Count > 0)
            {
                ShowNotice(notices.Dequeue());
            }
        }

        private void Bind()
        {
            if (progress != null || mission == null || mission.Progress == null || root == null)
                return;
            progress = mission.Progress;
            progress.ObjectiveChanged += OnObjectiveChanged;
            progress.Ended += OnEnded;
            title.text = mission.Definition != null ? mission.Definition.Title : string.Empty;
            root.RemoveFromClassList(HiddenClass);
            var current = progress.CurrentPrimary;
            if (current != null)
                announced.Add(current);
            Rebuild();
        }

        private void Rebuild()
        {
            objectiveList.Clear();
            rows.Clear();
            var current = progress.CurrentPrimary;
            if (current != null)
                AddRow(current);
            foreach (var status in progress.Objectives)
                if (status.Objective.Optional && status.State != ObjectiveState.Failed)
                    AddRow(status);
        }

        private void AddRow(ObjectiveStatus status)
        {
            var label = new Label(MissionText.Describe(status));
            label.AddToClassList(ObjectiveClass);
            label.EnableInClassList(OptionalClass, status.Objective.Optional);
            label.EnableInClassList(CompletedClass, status.State == ObjectiveState.Completed);
            objectiveList.Add(label);
            rows.Add((status, label));
        }

        private void OnObjectiveChanged(ObjectiveStatus status)
        {
            if (status.State == ObjectiveState.Completed)
                notices.Enqueue((MissionText.ObjectiveComplete, status.Objective.Description));
            else if (status.State == ObjectiveState.Active && !status.Objective.Optional && announced.Add(status))
                notices.Enqueue((MissionText.NewObjective, status.Objective.Description));
            Rebuild();
        }

        private void OnEnded(MissionOutcome _) => root.AddToClassList(HiddenClass);

        private void ShowNotice((string caption, string text) next)
        {
            noticeCaption.text = next.caption;
            noticeText.text = next.text;
            notice.AddToClassList(NoticeVisibleClass);
            noticeTimer = noticeTime;
        }
    }
}
