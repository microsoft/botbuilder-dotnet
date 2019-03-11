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

    /// <summary>The endpoint defintion of a translator.</summary>
    public class TextTranslatorEndpoint
    {
        private const string MsTranslatorUri = "https://api.cognitive.microsofttranslator.com";
        private const string DeeplRequestUri = "https://api.deepl.com/v1/translate";

        /// <summary>Initializes a new instance of the <see cref="TextTranslatorEndpoint"/> class.</summary>
        /// <param name="engine">The supported engines.</param>
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

        /// <summary>Initializes a new instance of the <see cref="TextTranslatorEndpoint"/> class.</summary>
        /// <param name="service">An instance of a TextTranslatorService.</param>
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

        /// <summary>Gets or sets the engine.</summary>
        /// <value>
        ///   <para>
        ///  The translator engine.
        /// </para>
        /// </value>
        public TranslatorEngine Engine { get; set; }

        /// <summary>Gets or sets the host.</summary>
        /// <value>
        ///   <para>
        ///  The Host of a Translator. e.g. https://api.cognitive.microsofttranslator.com or https://api.deepl.com/v1/translate</para>
        /// </value>
        public string Host { get; set; }

        /// <summary>Gets or sets the subscription key.</summary>
        /// <value>The subscription key.</value>
        public string SubscriptionKey { get; set; }
    }
}
