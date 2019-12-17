using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Legacy InputRecognizer class for legacy IRecognizer implementations.
    /// </summary>
    public class LegacyInputRecognizer : InputRecognizer
    {
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
            throw new NotImplementedException("This recognizer can only be used with a ITurnContext.");
        }
    }
}
