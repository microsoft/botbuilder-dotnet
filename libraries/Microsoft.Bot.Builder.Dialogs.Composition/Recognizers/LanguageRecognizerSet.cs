using System;
using System.Collections.Generic;
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
        /// {
        ///    "default" : {} 
        ///    "en" : { },
        ///    "fr": {}
        /// }
        /// </summary>
        public Dictionary<string, IRecognizer> Recognizers { get; set; } = new Dictionary<string, IRecognizer>(StringComparer.CurrentCultureIgnoreCase);

        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var recognizer = this.findRecognizerForLanguage(turnContext);
            if (recognizer != null)
            {
                return recognizer.RecognizeAsync(turnContext, cancellationToken);
            }
            return Task.FromResult(new RecognizerResult());
        }


        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            var recognizer = this.findRecognizerForLanguage(turnContext);
            if (recognizer != null)
            { 
                return recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
            }
            return Task.FromResult(new T());
        }

        private IRecognizer findRecognizerForLanguage(ITurnContext turnContext)
        {
            var loc = turnContext.Activity.Locale ?? String.Empty;
            while (loc.Length > 0)
            {
                if (this.Recognizers.TryGetValue(loc, out IRecognizer recognizer))
                {
                    return recognizer;
                }
                int iPos = loc.IndexOf("-");
                if (iPos < 0)
                    break;
                loc = loc.Substring(0, iPos);
            }

            if (this.Recognizers.TryGetValue("default", out IRecognizer defaultRecognizer))
            {
                return defaultRecognizer;
            }

            return null;
        }
    }
}
