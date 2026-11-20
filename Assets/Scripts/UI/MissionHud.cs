using System.Collections.Generic;
using PolishTanks.Missions;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace PolishTanks.UI
{
    /// <summary>
    /// Objective panels in the top corner. The main panel shows the mission title and the current primary objective
    /// with its count or timer; while that objective waits for its start marker it is dimmed and a hint says to drive
    /// into the circle. A separate panel lists every optional objective that has opened as open, done or missed. When an
    /// objective is done or a new one opens, a short notice slides in below; notices raised together are shown one after
    /// another.
    /// Hidden in scenes without a mission.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MissionHud : MonoBehaviour
    {
        private const string HiddenClass = "mission-hud--hidden";
        private const string NoticeVisibleClass = "mission-notice--visible";
        private const string ObjectiveClass = "mission-objective";
        private const string WaitingClass = "mission-objective--waiting";
        private const string CompletedClass = "mission-objective--completed";
        private const string FailedClass = "mission-objective--failed";

        [Tooltip("Seconds a notice stays on screen.")]
        [SerializeField, Min(0.5f)] private float noticeTime = 3.5f;

        [Inject] private IMission mission;

        private readonly List<(ObjectiveStatus status, Label label)> rows = new List<(ObjectiveStatus, Label)>();
        private readonly Queue<(string caption, string text)> notices = new Queue<(string, string)>();
        private readonly HashSet<ObjectiveStatus> announced = new HashSet<ObjectiveStatus>();
        private VisualElement root;
        private Label title;
        private VisualElement currentList;
        private Label startHint;
        private VisualElement optionalPanel;
        private VisualElement optionalList;
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
            currentList = tree.Q("currentObjective");
            startHint = tree.Q<Label>("startHint");
            optionalPanel = tree.Q("optionalPanel");
            optionalList = tree.Q("optionalObjectives");
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
                var text = MissionText.Describe(status, false);
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
            foreach (var status in progress.Objectives)
                if (status.State != ObjectiveState.Pending)
                    announced.Add(status);
            Rebuild();
        }

        private void Rebuild()
        {
            rows.Clear();
            currentList.Clear();
            optionalList.Clear();

            var current = progress.CurrentPrimary;
            if (current != null)
                AddRow(currentList, current);
            startHint.EnableInClassList(HiddenClass, current == null || current.State != ObjectiveState.Waiting);

            var hasOptional = false;
            foreach (var status in progress.Objectives)
            {
                if (!status.Objective.Optional || status.State == ObjectiveState.Pending)
                    continue;
                hasOptional = true;
                AddRow(optionalList, status);
            }
            optionalPanel.EnableInClassList(HiddenClass, !hasOptional);
        }

        private void AddRow(VisualElement list, ObjectiveStatus status)
        {
            var label = new Label(MissionText.Describe(status, false));
            label.AddToClassList(ObjectiveClass);
            label.EnableInClassList(WaitingClass, status.State == ObjectiveState.Waiting);
            label.EnableInClassList(CompletedClass, status.State == ObjectiveState.Completed);
            label.EnableInClassList(FailedClass, status.State == ObjectiveState.Failed);
            list.Add(label);
            rows.Add((status, label));
        }

        private void OnObjectiveChanged(ObjectiveStatus status)
        {
            if (status.State == ObjectiveState.Completed)
                notices.Enqueue((MissionText.ObjectiveComplete, status.Objective.Description));
            else if (status.State == ObjectiveState.Failed)
                notices.Enqueue((MissionText.ObjectiveFailed, status.Objective.Description));
            else if (status.State != ObjectiveState.Pending && announced.Add(status))
                notices.Enqueue((status.Objective.Optional ? MissionText.NewOptionalObjective : MissionText.NewObjective,
                    status.Objective.Description));
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
