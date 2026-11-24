using System;
using System.IO;
using NUnit.Framework;
using PolishTanks.Screenshots;

namespace PolishTanks.Tests.EditMode
{
    /// <summary>Tests for <see cref="ScreenshotFileNames"/>.</summary>
    public class ScreenshotFileNamesTests
    {
        private static readonly DateTime Moment = new DateTime(2026, 3, 7, 9, 5, 2);

        [Test]
        public void Next_NothingExists_ReturnsTimestampedPngInFolder()
        {
            var path = ScreenshotFileNames.Next("shots", Moment, _ => false);

            Assert.AreEqual(Path.Combine("shots", "PolishTanks_2026-03-07_09-05-02.png"), path);
        }

        [Test]
        public void Next_NameAndFirstCopyTaken_ReturnsThirdVariant()
        {
            var taken = new[]
            {
                Path.Combine("shots", "PolishTanks_2026-03-07_09-05-02.png"),
                Path.Combine("shots", "PolishTanks_2026-03-07_09-05-02_2.png"),
            };

            var path = ScreenshotFileNames.Next("shots", Moment, p => Array.IndexOf(taken, p) >= 0);

            Assert.AreEqual(Path.Combine("shots", "PolishTanks_2026-03-07_09-05-02_3.png"), path);
        }
    }
}
