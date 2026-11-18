namespace PolishTanks.Navigation
{
    /// <summary>Binary min-heap of cell indices by priority. Duplicates are allowed; the search skips closed cells.</summary>
    internal sealed class MinHeap
    {
        private int[] items = new int[256];
        private float[] priorities = new float[256];

        public int Count { get; private set; }

        public void Clear() => Count = 0;

        public void Push(int item, float priority)
        {
            if (Count == items.Length)
            {
                System.Array.Resize(ref items, Count * 2);
                System.Array.Resize(ref priorities, Count * 2);
            }

            var i = Count++;
            while (i > 0)
            {
                var parent = (i - 1) / 2;
                if (priorities[parent] <= priority)
                    break;
                items[i] = items[parent];
                priorities[i] = priorities[parent];
                i = parent;
            }
            items[i] = item;
            priorities[i] = priority;
        }

        public int Pop()
        {
            var top = items[0];
            var lastItem = items[--Count];
            var lastPriority = priorities[Count];
            var i = 0;
            while (true)
            {
                var child = i * 2 + 1;
                if (child >= Count)
                    break;
                if (child + 1 < Count && priorities[child + 1] < priorities[child])
                    child++;
                if (priorities[child] >= lastPriority)
                    break;
                items[i] = items[child];
                priorities[i] = priorities[child];
                i = child;
            }
            items[i] = lastItem;
            priorities[i] = lastPriority;
            return top;
        }
    }
}
