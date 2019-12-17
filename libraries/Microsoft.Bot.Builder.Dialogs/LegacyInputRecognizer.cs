using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Legacy InputRecognizer class for legacy IRecognizer implementations.
    /// </summary>
    public class LegacyInputRecognizer : InputRecognizer
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.LegacyInputRecognizer";

        [JsonConstructor]
        public LegacyInputRecognizer(IRecognizer recognizer = null)
        {
            this.Recognizer = recognizer;
        }

        public IRecognizer Recognizer { get; set; }

        public override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            return this.Recognizer.RecognizeAsync(dialogContext.Context, cancellationToken);
        }

        public override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, string text, string locale, CancellationToken cancellationToken)
        {
            // create turn context with fake message activity with text/locale in it
            return this.Recognizer.RecognizeAsync(
                new TurnContext(
                    dialogContext.Context.Adapter,
                    new Activity()
                    {
                        Type = ActivityTypes.Message,
                        Text = text,
                        Locale = locale
                    }),
                cancellationToken);
        }
    }
}
