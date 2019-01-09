using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Recognizers
{
    /// <summary>
    /// Map of language to recognizers, which uses the incoming language to decide which recognizer to invoke
    /// </summary>
    public class LanguageRecognizerSet : IRecognizer
    {

        public LanguageRecognizerSet()
        {
        }

        /// <summary>
        /// Map of language locales to recognizers
        /// </summary>
        Dictionary<string, IRecognizer> Recognizers { get; set; } = new Dictionary<string, IRecognizer>();

        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (this.Recognizers.TryGetValue(turnContext.Activity.Locale, out IRecognizer recognizer))
            {
                return recognizer.RecognizeAsync(turnContext, cancellationToken);
            }
            else if (this.Recognizers.TryGetValue("default", out IRecognizer defaultRecognizer))
            {
                return defaultRecognizer.RecognizeAsync(turnContext, cancellationToken);
            }
            return Task.FromResult(new RecognizerResult());
        }

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            if (this.Recognizers.TryGetValue(turnContext.Activity.Locale, out IRecognizer recognizer))
            {
                return recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
            }
            else if (this.Recognizers.TryGetValue("default", out IRecognizer defaultRecognizer))
            {
                return defaultRecognizer.RecognizeAsync<T>(turnContext, cancellationToken);
            }
            return Task.FromResult(new T());
        }
    }
}
