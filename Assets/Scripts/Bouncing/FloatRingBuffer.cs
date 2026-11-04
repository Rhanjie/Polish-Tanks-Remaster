using System;
using System.Collections;
using System.Collections.Generic;

namespace PolishTanks.Bouncing
{
    /// <summary>Fixed-capacity history that overwrites the oldest value. Index 0 is the oldest entry.</summary>
    public sealed class FloatRingBuffer : IReadOnlyList<float>
    {
        private readonly float[] _items;
        private int _start;
        private int _count;

        public FloatRingBuffer(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");
            _items = new float[capacity];
        }

        public int Capacity => _items.Length;
        public int Count => _count;

        public float this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _items[(_start + index) % _items.Length];
            }
        }

        public void Add(float value)
        {
            if (_count < _items.Length)
            {
                _items[(_start + _count) % _items.Length] = value;
                _count++;
                return;
            }

            _items[_start] = value;
            _start = (_start + 1) % _items.Length;
        }

        public void Clear()
        {
            _start = 0;
            _count = 0;
        }

        public float Average()
        {
            if (_count == 0)
                return 0f;

            var sum = 0f;
            for (var i = 0; i < _count; i++)
                sum += this[i];
            return sum / _count;
        }

        public IEnumerator<float> GetEnumerator()
        {
            for (var i = 0; i < _count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
