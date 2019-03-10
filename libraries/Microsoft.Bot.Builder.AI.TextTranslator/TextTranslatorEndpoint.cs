using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Configuration;

namespace Microsoft.Bot.Builder.AI.TextTranslator
{
    public enum TranslatorEngine
    {
        /// <summary>
        /// Microsoft Translator API
        /// </summary>
        MicrosoftTranslator,

        /// <summary>
        /// Use of Deepl Translator
        /// </summary>
        Deepl,
    }

    public class TextTranslatorEndpoint
    {
        private const string MsTranslatorUri = "https://api.cognitive.microsofttranslator.com";
        private const string DeeplRequestUri = "https://api.deepl.com/v1/translate";

        public TextTranslatorEndpoint(TranslatorEngine engine)
        {
            Engine = engine;

            switch (engine)
            {
                case TranslatorEngine.Deepl:
                    Host = DeeplRequestUri;
                    break;
                case TranslatorEngine.MicrosoftTranslator:
                    Host = MsTranslatorUri;
                    break;
            }
        }

        public TextTranslatorEndpoint(TextTranslatorService service)
        {
            switch (service.Engine.ToLowerInvariant())
            {
                case "microsofttranslator":
                    Engine = TranslatorEngine.MicrosoftTranslator;
                    break;
                case "deepl":
                    Engine = TranslatorEngine.Deepl;
                    break;
                default:
                    throw new ArgumentException("TranslatorEngine unknown");
            }

            SubscriptionKey = service.SubscriptionKey;
            Host = service.Host;
        }

        public TranslatorEngine Engine { get; set; }

        public string Host { get; set; }

        public string SubscriptionKey { get; set; }
    }
}
