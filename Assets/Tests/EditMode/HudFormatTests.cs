using System.Globalization;
using NUnit.Framework;
using PolishTanks.UI;
using UnityEngine;

namespace PolishTanks.Tests.EditMode
{
    public class HudFormatTests
    {
        private CultureInfo _previous;

        [SetUp]
        public void SetUp() => _previous = CultureInfo.CurrentCulture;

        [TearDown]
        public void TearDown() => CultureInfo.CurrentCulture = _previous;

        [TestCase(1, 9)]
        [TestCase(3, 999)]
        [TestCase(5, 99999)]
        public void MaxValue_ForDigits_IsAllNines(int digits, int expected)
        {
            Assert.AreEqual(expected, HudFormat.MaxValue(digits));
        }

        [TestCase(12345, 0, 1)]
        [TestCase(12345, 4, 5)]
        [TestCase(42, 0, 0)]
        [TestCase(42, 3, 4)]
        [TestCase(42, 4, 2)]
        public void DigitAt_ZeroPaddedFiveDigits_ReturnsExpectedDigit(int value, int position, int expected)
        {
            Assert.AreEqual(expected, HudFormat.DigitAt(value, position, 5));
        }

        [Test]
        public void DigitAt_ValueAboveMax_ClampsToMax()
        {
            Assert.AreEqual(9, HudFormat.DigitAt(1_000_000, 0, 5));
        }

        [Test]
        public void DigitAt_NegativeValue_ClampsToZero()
        {
            Assert.AreEqual(0, HudFormat.DigitAt(-50, 4, 5));
        }

        [TestCase(0, 4)]
        [TestCase(7, 4)]
        [TestCase(10, 3)]
        [TestCase(99999, 0)]
        [TestCase(500000, 0)]
        [TestCase(-5, 4)]
        public void LeadingZeros_FiveDigits_ReturnsExpected(int value, int expected)
        {
            Assert.AreEqual(expected, HudFormat.LeadingZeros(value, 5));
        }

        [TestCase(0f, "00:00.0")]
        [TestCase(60f, "01:00.0")]
        [TestCase(125.5f, "02:05.5")]
        [TestCase(6000f, "100:00.0")]
        public void Uptime_ValidSeconds_FormatsMinutesSecondsTenths(float seconds, string expected)
        {
            Assert.AreEqual(expected, HudFormat.Uptime(seconds));
        }

        [Test]
        public void Uptime_Negative_ShowsZero()
        {
            Assert.AreEqual("00:00.0", HudFormat.Uptime(-4f));
        }

        [Test]
        public void Uptime_NaN_ShowsZero()
        {
            Assert.AreEqual("00:00.0", HudFormat.Uptime(float.NaN));
        }

        [Test]
        public void Uptime_UnderPolishCulture_UsesDot()
        {
            CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

            Assert.AreEqual("00:01.5", HudFormat.Uptime(1.5f));
        }

        [Test]
        public void Speed_UnderPolishCulture_UsesDotNotComma()
        {
            CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

            Assert.AreEqual("3.50", HudFormat.Speed(3.5f));
        }

        [Test]
        public void Speed_Integer_HasTwoDecimals()
        {
            Assert.AreEqual("2.00", HudFormat.Speed(2f));
        }

        [Test]
        public void Hex_OpaqueColor_ReturnsUppercaseRgbWithHash()
        {
            Assert.AreEqual("#FF8000", HudFormat.Hex(new Color32(255, 128, 0, 255)));
        }
    }
}
