using System;
using System.Collections.Generic;

namespace PolishTanks.Radio
{
    /// <summary>
    /// The player's radio without any UI. Messages of one sequence play one at a time; the current one is typed out
    /// letter by letter as <see cref="Tick"/> runs, then stays up for a reading time that grows with its length and
    /// moves on by itself, so nothing is left hanging once the player has driven on. A new sequence cuts in at once and
    /// replaces whatever was on; the action of a replaced sequence still runs. <see cref="Confirm"/> shows the whole
    /// message at once and, pressed again, moves on early.
    /// </summary>
    public sealed class RadioChannel
    {
        private readonly Queue<(RadioMessage message, Action after)> queue = new Queue<(RadioMessage, Action)>();
        private readonly List<Action> replaced = new List<Action>();
        private readonly float charactersPerSecond;
        private readonly float readSeconds;
        private readonly float readSecondsPerCharacter;
        private float typed;
        private float sinceFull;
        private Action afterCurrent;

        public RadioChannel(float charactersPerSecond = 55f, float readSeconds = 3f, float readSecondsPerCharacter = 0.05f)
        {
            this.charactersPerSecond = Math.Max(1f, charactersPerSecond);
            this.readSeconds = Math.Max(0f, readSeconds);
            this.readSecondsPerCharacter = Math.Max(0f, readSecondsPerCharacter);
        }

        public event Action<RadioMessage> MessageChanged;

        public RadioMessage Current { get; private set; }
        public int VisibleCharacters { get; private set; }
        public bool IsTyping => Current != null && VisibleCharacters < Current.Text.Length;

        public float ReadTime
        {
            get
            {
                if (Current == null)
                    return 0f;
                var readTime = readSeconds + Current.Text.Length * readSecondsPerCharacter;
                return Current.Voice != null ? Math.Max(readTime, Current.Voice.length) : readTime;
            }
        }

        public float TimeLeftShare
        {
            get
            {
                if (Current == null)
                    return 0f;
                if (IsTyping)
                    return 1f;
                var readTime = ReadTime;
                return readTime > 0f ? Math.Max(0f, 1f - sinceFull / readTime) : 0f;
            }
        }

        public void Play(RadioSequence sequence, Action after = null)
        {
            if (sequence != null)
                Play(sequence.Messages, after);
            else
                after?.Invoke();
        }

        public void Play(IReadOnlyList<RadioMessage> messages, Action after = null)
        {
            var last = -1;
            for (var i = 0; i < messages.Count; i++)
                if (messages[i] != null)
                    last = i;
            if (last < 0)
            {
                after?.Invoke();
                return;
            }

            replaced.Clear();
            if (afterCurrent != null)
                replaced.Add(afterCurrent);
            foreach (var queued in queue)
                if (queued.after != null)
                    replaced.Add(queued.after);
            queue.Clear();
            afterCurrent = null;

            for (var i = 0; i <= last; i++)
                if (messages[i] != null)
                    queue.Enqueue((messages[i], i == last ? after : null));
            Next();

            var dropped = replaced.ToArray();
            replaced.Clear();
            foreach (var action in dropped)
                action();
        }

        public void Tick(float deltaTime)
        {
            if (Current == null || deltaTime <= 0f)
                return;
            if (IsTyping)
            {
                typed += deltaTime * charactersPerSecond;
                VisibleCharacters = Math.Min(Current.Text.Length, (int)typed);
                return;
            }
            sinceFull += deltaTime;
            if (sinceFull >= ReadTime)
                Next();
        }

        public void Confirm()
        {
            if (Current == null)
                return;
            if (IsTyping)
                VisibleCharacters = Current.Text.Length;
            else
                Next();
        }

        public void Clear()
        {
            queue.Clear();
            afterCurrent = null;
            if (Current != null)
                Next();
        }

        private void Next()
        {
            var finished = afterCurrent;
            (Current, afterCurrent) = queue.Count > 0 ? queue.Dequeue() : (null, null);
            typed = 0f;
            sinceFull = 0f;
            VisibleCharacters = 0;
            MessageChanged?.Invoke(Current);
            finished?.Invoke();
        }
    }
}
