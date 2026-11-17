using System.Collections.Generic;
using UnityEngine;

namespace PolishTanks.Navigation
{
    /// <summary>
    /// What one A* search did, recorded only while a debug view asks for it: every cell it expanded (in order),
    /// the result, and how long it took. <see cref="Version"/> grows with every recorded search, so a view knows when
    /// to repaint.
    /// </summary>
    public sealed class SearchTrace
    {
        private static int lastVersion;

        public SearchTrace(PathRequest request, PathResult result, List<int> expandedCells, double milliseconds)
        {
            Request = request;
            Result = result;
            ExpandedCells = expandedCells;
            Milliseconds = milliseconds;
            Version = ++lastVersion;
        }

        public PathRequest Request { get; }
        public PathResult Result { get; }
        public IReadOnlyList<int> ExpandedCells { get; }
        public double Milliseconds { get; }
        public int Version { get; }
        public Vector2 From => Request.From;
        public Vector2 To => Request.To;
    }
}
