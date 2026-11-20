using System;
using System.Collections.Generic;

namespace PolishTanks.Radio
{
    /// <summary>
    /// The player's radio without any UI. Messages queue up and play one at a time; the current one is typed out
    /// letter by letter as <see cref="Tick"/> runs. <see cref="Confirm"/> first shows the whole message at once and,
    /// once it is all shown, moves on to the next message (or clears the radio when the queue is empty).
    /// </summary>
    public sealed class RadioChannel
    {
        private readonly Queue<RadioMessage> queue = new Queue<RadioMessage>();
        private readonly float charactersPerSecond;
        private float typed;

        public RadioChannel(float charactersPerSecond = 40f)
        {
            this.charactersPerSecond = Math.Max(1f, charactersPerSecond);
        }

        public event Action<RadioMessage> MessageChanged;

        public RadioMessage Current { get; private set; }
        public int VisibleCharacters { get; private set; }
        public bool IsTyping => Current != null && VisibleCharacters < Current.Text.Length;

        public void Play(RadioSequence sequence)
        {
            if (sequence != null)
                Play(sequence.Messages);
        }

        public void Play(IEnumerable<RadioMessage> messages)
        {
            foreach (var message in messages)
                if (message != null)
                    queue.Enqueue(message);
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
            if (Current != null)
                Next();
        }

        private void Next()
        {
            Current = queue.Count > 0 ? queue.Dequeue() : null;
            typed = 0f;
            VisibleCharacters = 0;
            MessageChanged?.Invoke(Current);
        }
    }
}
