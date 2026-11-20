using NUnit.Framework;
using PolishTanks.Radio;

namespace PolishTanks.Tests.EditMode
{
    /// <summary><see cref="RadioChannel"/>: typing out a message, skipping the typing, moving on and queueing.</summary>
    public class RadioChannelTests
    {
        private static RadioMessage Message(string text) => new RadioMessage(null, text);

        [Test]
        public void Confirm_WhileTyping_ShowsTheWholeMessageAndAgain_MovesToTheNext()
        {
            var radio = new RadioChannel(10f);
            var first = Message("Twenty characters...");
            var second = Message("Next");
            radio.Play(new[] { first, second });

            radio.Tick(0.5f);
            var typedHalfway = radio.VisibleCharacters;
            radio.Confirm();
            var afterSkip = radio.VisibleCharacters;
            radio.Confirm();

            Assert.That(typedHalfway, Is.EqualTo(5));
            Assert.That(afterSkip, Is.EqualTo(first.Text.Length));
            Assert.That(radio.Current, Is.SameAs(second));
            Assert.That(radio.VisibleCharacters, Is.Zero);
        }

        [Test]
        public void Play_WhileAMessageIsOn_QueuesBehindItAndTheLastConfirmEmptiesTheRadio()
        {
            var radio = new RadioChannel(1000f);
            var first = Message("First");
            var later = Message("Later");
            radio.Play(new[] { first });

            radio.Play(new[] { later });
            var current = radio.Current;
            radio.Tick(1f);
            radio.Confirm();
            var next = radio.Current;
            radio.Tick(1f);
            radio.Confirm();

            Assert.That(current, Is.SameAs(first));
            Assert.That(next, Is.SameAs(later));
            Assert.That(radio.Current, Is.Null);
        }
    }
}
