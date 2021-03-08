using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers.Tests
{
    [CollectionDefinition("Dialogs.Adaptive.Recognizers")]
    public class QuotedTextRecognizerTests
    {
        private Recognizer _recognizer = new RegexRecognizer() { Entities = new EntityRecognizerSet() { new QuotedTextEntityRecognizer() } };

        public async Task<RecognizerResult> Recognize(string text, string locale)
        {
            var activity = (Activity)Microsoft.Bot.Schema.Activity.CreateMessageActivity();
            activity.Locale = locale;
            activity.Text = text;
            var dc = new DialogContext(new DialogSet(), new TurnContext(new TestAdapter(), (Activity)activity), new DialogState());
            var entities = new List<Entity>();
            return await _recognizer.RecognizeAsync(dc, activity, default(CancellationToken));
        }

        [Fact]
        public async Task TestQuotedEntity_NullLocale()
        {
            var result = await Recognize("this is a `Isn't this cool?` „another quoted string”", null);
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.Equal(1, quotedText.Count);
            Assert.Equal("Isn't this cool?", quotedText[0].ToString());
        }

        [Fact]
        public async Task TestQuotedEntity_English()
        {
            var result = await Recognize("this is a `Isn't this cool?` „another quoted string”", "en");
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.Equal(1, quotedText.Count);
            Assert.Equal("Isn't this cool?", quotedText[0].ToString());
        }

        [Fact]
        public async Task TestQuotedEntity_Africaans()
        {
            var result = await Recognize("this is a `Isn't this cool?` „another quoted string”", "af");
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.Equal(2, quotedText.Count);
            Assert.Equal("another quoted string", quotedText[0].ToString());
            Assert.Equal("Isn't this cool?", quotedText[1].ToString());
        }

        [Fact]
        public async Task TestQuotedEntity_Overlapping()
        {
            var result = await Recognize("this is a `this is \"a test\"` ", "en");
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.Equal(2, quotedText.Count);
            Assert.Equal("a test", quotedText[0].ToString());
            Assert.Equal("this is \"a test\"", quotedText[1].ToString());
        }

        [Fact]
        public async Task TestQuotedEntity_OverlappingOffset()
        {
            var result = await Recognize("this is a `this is \"a` test\" ", "en");
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.Equal(2, quotedText.Count);
            Assert.Equal("a` test", quotedText[0].ToString());
            Assert.Equal("this is \"a", quotedText[1].ToString());
        }
    }
}
