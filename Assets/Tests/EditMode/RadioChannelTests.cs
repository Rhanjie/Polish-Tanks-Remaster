using NUnit.Framework;
using PolishTanks.Radio;

namespace PolishTanks.Tests.EditMode
{
    /// <summary><see cref="RadioChannel"/>: typing out a message, skipping the typing, moving on, closing by itself, a new sequence cutting in and the action after a sequence.</summary>
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
        public void Tick_LongerThanTheReadingTime_ClosesTheMessageByItselfAndRunsTheAction()
        {
            var radio = new RadioChannel(1000f, 2f, 0.1f);
            var runs = 0;
            radio.Play(new[] { Message("Ten chars!") }, () => runs++);

            radio.Tick(1f);
            radio.Tick(2.9f);
            var whileReading = radio.Current;
            radio.Tick(0.2f);

            Assert.That(whileReading, Is.Not.Null);
            Assert.That(radio.Current, Is.Null);
            Assert.That(runs, Is.EqualTo(1));
        }

        [Test]
        public void Play_WhileAMessageIsOn_ReplacesItAtOnceAndStillRunsTheReplacedAction()
        {
            var radio = new RadioChannel(1000f);
            var replacedRuns = 0;
            var later = Message("Later");
            radio.Play(new[] { Message("First"), Message("Second") }, () => replacedRuns++);
            radio.Tick(0.002f);

            radio.Play(new[] { later });
            var current = radio.Current;
            var typedAfterCut = radio.VisibleCharacters;
            radio.Confirm();
            radio.Confirm();

            Assert.That(current, Is.SameAs(later));
            Assert.That(typedAfterCut, Is.Zero);
            Assert.That(replacedRuns, Is.EqualTo(1));
            Assert.That(radio.Current, Is.Null);
        }

        [Test]
        public void Play_WithAnAfterAction_RunsItOnlyOnceThePlayerGoesPastTheLastMessage()
        {
            var radio = new RadioChannel(1000f);
            var runs = 0;
            radio.Play(new[] { Message("One"), Message("Two") }, () => runs++);

            radio.Tick(1f);
            radio.Confirm();
            var afterFirst = runs;
            radio.Tick(1f);
            radio.Confirm();

            Assert.That(afterFirst, Is.Zero);
            Assert.That(runs, Is.EqualTo(1));
        }
    }
}
