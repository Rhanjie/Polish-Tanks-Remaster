using System;
using System.Collections.Generic;

namespace PolishTanks.Missions
{
    /// <summary>
    /// Rules of a running mission without any scene. Primary objectives open one at a time in list order and the
    /// mission is won when the last one is done; one with a start marker waits (<see cref="ObjectiveState.Waiting"/>)
    /// until <see cref="Begin"/>. Optional objectives are open from the start and only add respect; an Avoid objective
    /// fails on <see cref="Violated"/> and is done when its closing objective or the won mission is.
    /// A zone counts only while its objective is in progress; targets destroyed early count as soon as theirs begins.
    /// Losing the player's tank (<see cref="Fail"/>) loses the mission. Whatever is still open at the end is missed.
    /// Respect is the sum of completed objectives and enemy losses.
    /// </summary>
    public sealed class MissionProgress
    {
        private readonly List<ObjectiveStatus> objectives = new List<ObjectiveStatus>();
        private readonly Dictionary<EnemyLoss, int> losses = new Dictionary<EnemyLoss, int>();
        private readonly MissionRespect respect;

        public MissionProgress(IEnumerable<MissionObjective> definitions, MissionRespect respect)
        {
            this.respect = respect;
            foreach (var definition in definitions)
                if (definition != null)
                    objectives.Add(new ObjectiveStatus(definition));
            foreach (var status in objectives)
                if (status.Objective.Optional)
                    status.State = ObjectiveState.Active;
            OpenNextPrimary();
        }

        public event Action<ObjectiveStatus> ObjectiveChanged;
        public event Action<MissionOutcome> Ended;

        public IReadOnlyList<ObjectiveStatus> Objectives => objectives;

        public MissionOutcome Outcome { get; private set; }

        public ObjectiveStatus CurrentPrimary =>
            objectives.Find(status => !status.Objective.Optional
                && (status.State == ObjectiveState.Waiting || status.State == ObjectiveState.Active));

        public int ObjectiveRespect
        {
            get
            {
                var total = 0;
                foreach (var status in objectives)
                    total += status.EarnedRespect;
                return total;
            }
        }

        public int LossRespect
        {
            get
            {
                var total = 0;
                foreach (var pair in losses)
                    total += respect.For(pair.Key) * pair.Value;
                return total;
            }
        }

        public int TotalRespect => ObjectiveRespect + LossRespect;

        public int Losses(EnemyLoss loss) => losses.TryGetValue(loss, out var count) ? count : 0;

        public void SetSceneTargets(string id, int count)
        {
            foreach (var status in objectives)
                if (IsDestroyObjective(status, id) && status.Objective.TargetCount == 0)
                    status.Required = Math.Max(1, count);
        }

        public void Begin(string id)
        {
            var current = CurrentPrimary;
            if (Outcome != MissionOutcome.Running || current == null || current.State != ObjectiveState.Waiting
                || current.Objective.Id != id)
                return;
            Activate(current);
        }

        public void Violated(string id)
        {
            if (Outcome != MissionOutcome.Running)
                return;
            for (var i = 0; i < objectives.Count; i++)
            {
                var status = objectives[i];
                if (status.Objective.Id != id || status.Objective.Kind != MissionObjectiveKind.Avoid
                    || status.State != ObjectiveState.Active)
                    continue;
                status.State = ObjectiveState.Failed;
                ObjectiveChanged?.Invoke(status);
            }
        }

        public void Reached(string id)
        {
            if (Outcome != MissionOutcome.Running)
                return;
            for (var i = 0; i < objectives.Count; i++)
            {
                var status = objectives[i];
                if (status.Objective.Id == id && status.Objective.Kind == MissionObjectiveKind.Reach
                    && status.State == ObjectiveState.Active)
                    Complete(status);
            }
        }

        public void TargetDestroyed(string id)
        {
            if (Outcome != MissionOutcome.Running)
                return;
            for (var i = 0; i < objectives.Count; i++)
            {
                var status = objectives[i];
                if (!IsDestroyObjective(status, id) || status.State > ObjectiveState.Active)
                    continue;
                status.Count++;
                if (status.State == ObjectiveState.Active && status.Count >= status.Required)
                    Complete(status);
                else
                    ObjectiveChanged?.Invoke(status);
            }
        }

        public void Tick(float deltaTime)
        {
            if (Outcome != MissionOutcome.Running || deltaTime <= 0f)
                return;
            for (var i = 0; i < objectives.Count; i++)
            {
                var status = objectives[i];
                if (status.State != ObjectiveState.Active || status.Objective.Kind != MissionObjectiveKind.Survive)
                    continue;
                status.TimeLeft = Math.Max(0f, status.TimeLeft - deltaTime);
                if (status.TimeLeft <= 0f)
                    Complete(status);
            }
        }

        public void EnemyLost(EnemyLoss loss)
        {
            if (Outcome == MissionOutcome.Running)
                losses[loss] = Losses(loss) + 1;
        }

        public void Fail() => End(MissionOutcome.Lost);

        private static bool IsDestroyObjective(ObjectiveStatus status, string id) =>
            status.Objective.Id == id && status.Objective.Kind == MissionObjectiveKind.Destroy;

        private void Complete(ObjectiveStatus status)
        {
            if (Outcome != MissionOutcome.Running)
                return;
            status.State = ObjectiveState.Completed;
            ObjectiveChanged?.Invoke(status);
            CloseAvoidObjectivesEndingWith(status.Objective.Id);
            if (status.Objective.Optional)
                return;
            var next = OpenNextPrimary();
            if (next == null)
                End(MissionOutcome.Won);
            else if (next.State == ObjectiveState.Waiting)
                ObjectiveChanged?.Invoke(next);
            else
                Activate(next);
        }

        private void Activate(ObjectiveStatus status)
        {
            status.State = ObjectiveState.Active;
            if (status.Objective.Kind == MissionObjectiveKind.Destroy && status.Count >= status.Required)
                Complete(status);
            else
                ObjectiveChanged?.Invoke(status);
        }

        private void CloseAvoidObjectivesEndingWith(string id)
        {
            for (var i = 0; i < objectives.Count; i++)
            {
                var status = objectives[i];
                if (status.Objective.Kind == MissionObjectiveKind.Avoid && status.State == ObjectiveState.Active
                    && !string.IsNullOrEmpty(status.Objective.UntilObjectiveId) && status.Objective.UntilObjectiveId == id)
                    Complete(status);
            }
        }

        private ObjectiveStatus OpenNextPrimary()
        {
            var next = objectives.Find(status => !status.Objective.Optional && status.State == ObjectiveState.Pending);
            if (next == null)
                return null;
            next.State = next.Objective.StartMarker ? ObjectiveState.Waiting : ObjectiveState.Active;
            return next;
        }

        private void End(MissionOutcome outcome)
        {
            if (Outcome != MissionOutcome.Running)
                return;
            Outcome = outcome;
            foreach (var status in objectives)
            {
                if (status.State == ObjectiveState.Completed || status.State == ObjectiveState.Failed)
                    continue;
                var kept = outcome == MissionOutcome.Won && status.Objective.Kind == MissionObjectiveKind.Avoid
                    && status.State == ObjectiveState.Active;
                status.State = kept ? ObjectiveState.Completed : ObjectiveState.Failed;
            }
            Ended?.Invoke(outcome);
        }
    }
}
