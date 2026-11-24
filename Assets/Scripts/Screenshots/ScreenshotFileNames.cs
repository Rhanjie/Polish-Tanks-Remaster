using System;
using System.Globalization;
using System.IO;

namespace PolishTanks.Screenshots
{
    /// <summary>
    /// Picks the file for a new screenshot: a PNG named after the moment it was taken, with a counter added when a
    /// file of that name already exists, so two shots in the same second don't overwrite each other.
    /// </summary>
    public static class ScreenshotFileNames
    {
        private const string Prefix = "PolishTanks_";
        private const string TimeFormat = "yyyy-MM-dd_HH-mm-ss";
        private const string Extension = ".png";

        public static string Next(string folder, DateTime time, Func<string, bool> exists)
        {
            var stem = Prefix + time.ToString(TimeFormat, CultureInfo.InvariantCulture);
            var path = Path.Combine(folder, stem + Extension);
            for (var copy = 2; exists(path); copy++)
                path = Path.Combine(folder, stem + "_" + copy.ToString(CultureInfo.InvariantCulture) + Extension);
            return path;
        }
    }
}
