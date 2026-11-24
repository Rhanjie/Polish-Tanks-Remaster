namespace PolishTanks.Navigation
{
    /// <summary>
    /// Every string the F2 navigation view shows on screen, in one place for a later localization module.
    /// Values go in through format placeholders, never by gluing sentences together.
    /// </summary>
    public static class NavigationDebugText
    {
        public const string HeaderFormat = "Navigation (F2): {0}";
        public const string Help = "Middle mouse: show the path to the cursor\nShift + middle mouse: drive there";
        public const string LastSearchFormat = "Last search: {0}, {1} cells expanded, {2:0.0} ms, {3:0} m";

        public static string ViewName(NavigationDebugView view) => view switch
        {
            NavigationDebugView.Grid => "grid",
            NavigationDebugView.Clearance => "clearance",
            _ => "off",
        };

        public static string StatusName(PathStatus status) => status switch
        {
            PathStatus.Found => "found",
            PathStatus.Partial => "partial",
            _ => "no path",
        };
    }
}
