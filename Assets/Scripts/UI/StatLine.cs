namespace PolishTanks.UI
{
    /// <summary>One row of <see cref="LoadoutStatSheet"/>: a stat, its value and how it moved since the last build.</summary>
    public readonly struct StatLine
    {
        public StatLine(string label, string value, string delta, StatChange change)
        {
            Label = label;
            Value = value;
            Delta = delta;
            Change = change;
        }

        public string Label { get; }
        public string Value { get; }
        public string Delta { get; }
        public StatChange Change { get; }
    }
}
