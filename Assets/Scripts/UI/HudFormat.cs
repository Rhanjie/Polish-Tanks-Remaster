using System.Globalization;
using UnityEngine;

namespace PolishTanks.UI
{
    /// <summary>Formatting helpers for the HUD, kept free of UI Toolkit so they can be unit tested.</summary>
    public static class HudFormat
    {
        private static readonly string[] DigitStrings = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        public static string Digit(int digit) => DigitStrings[Mathf.Clamp(digit, 0, 9)];

        /// <summary>Largest value that fits in <paramref name="digits"/> decimal digits (99999 for 5).</summary>
        public static int MaxValue(int digits)
        {
            var max = 1;
            for (var i = 0; i < digits; i++)
                max *= 10;
            return max - 1;
        }

        /// <summary>Digit at <paramref name="position"/>, where 0 is the leftmost of a zero-padded number.</summary>
        public static int DigitAt(int value, int position, int digits)
        {
            value = Mathf.Clamp(value, 0, MaxValue(digits));
            for (var i = digits - 1; i > position; i--)
                value /= 10;
            return value % 10;
        }

        /// <summary>How many leading zeros the padded number has. The last digit never counts, so 0 shows as "0".</summary>
        public static int LeadingZeros(int value, int digits)
        {
            value = Mathf.Clamp(value, 0, MaxValue(digits));
            var significant = 1;
            while (value >= 10)
            {
                value /= 10;
                significant++;
            }
            return digits - significant;
        }

        /// <summary>"mm:ss.t"; minutes grow past two digits when needed.</summary>
        public static string Uptime(float seconds)
        {
            if (float.IsNaN(seconds) || seconds < 0f)
                seconds = 0f;
            var tenths = (long)(seconds * 10f);
            var minutes = tenths / 600;
            var secs = tenths / 10 % 60;
            var tenth = tenths % 10;
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}.{2}", minutes, secs, tenth);
        }

        public static string Speed(float metersPerSecond) =>
            metersPerSecond.ToString("0.00", CultureInfo.InvariantCulture);

        public static string Hex(Color color) => "#" + ColorUtility.ToHtmlStringRGB(color);
    }
}
