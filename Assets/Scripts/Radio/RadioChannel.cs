using System;
using System.Collections.Generic;

namespace PolishTanks.Radio
{
    /// <summary>
    /// The player's radio without any UI. Messages queue up and play one at a time; the current one is typed out
    /// letter by letter as <see cref="Tick"/> runs. <see cref="Confirm"/> first shows the whole message at once and,
    /// once it is all shown, moves on to the next message (or clears the radio when the queue is empty). A sequence can
    /// carry an action that runs once the player has gone past its last message.
    /// </summary>
    public sealed class RadioChannel
    {
        private readonly Queue<(RadioMessage message, Action after)> queue = new Queue<(RadioMessage, Action)>();
        private readonly float charactersPerSecond;
        private float typed;
        private Action afterCurrent;

        public RadioChannel(float charactersPerSecond = 40f)
        {
            this.charactersPerSecond = Math.Max(1f, charactersPerSecond);
        }

        public event Action<RadioMessage> MessageChanged;

        public RadioMessage Current { get; private set; }
        public int VisibleCharacters { get; private set; }
        public bool IsTyping => Current != null && VisibleCharacters < Current.Text.Length;

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
            for (var i = 0; i <= last; i++)
                if (messages[i] != null)
                    queue.Enqueue((messages[i], i == last ? after : null));
            if (Current == null)
                Next();
        }

        public void Tick(float deltaTime)
        {
            if (!IsTyping || deltaTime <= 0f)
                return;
            typed += deltaTime * charactersPerSecond;
            VisibleCharacters = Math.Min(Current.Text.Length, (int)typed);
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
            VisibleCharacters = 0;
            MessageChanged?.Invoke(Current);
            finished?.Invoke();
        }
    }
}
