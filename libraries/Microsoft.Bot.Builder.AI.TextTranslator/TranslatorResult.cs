using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.TextTranslator.Deepl;
using Microsoft.Bot.Builder.AI.TextTranslator.MsTranslator;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.TextTranslator
{
    /// <summary>Contains the result of a translation.</summary>
    public class TranslatorResult
    {
        /// <summary>Gets or sets the text.</summary>
        /// <value>
        ///   <para>
        ///  The translated text.
        /// </para>
        /// </value>
        public string Text { get; set; }

        /// <summary>Gets or sets the detected source language.</summary>
        /// <value>The detected source language.</value>
        public string DetectedSourceLanguage { get; set; }

        /// <summary>Gets or sets the detected source score.</summary>
        /// <value>The detected source score.</value>
        public double DetectedSourceScore { get; set; }

        /// <summary>Gets the language.</summary>
        /// <value>
        ///   <para>
        ///  The language of the text.
        /// </para>
        /// </value>
        public string Language { get; private set; }

        internal static TranslatorResult Create(DeeplTranslationResults results, string targetLanguage)
        {
            var t = results.Translations.First();
            return new TranslatorResult()
            {
                Text = t.Text,
                Language = targetLanguage,
                DetectedSourceLanguage = t.DetectedSourceLanguage,
                DetectedSourceScore = 1,
            };
        }

        internal static TranslatorResult Create(MsTranslatorTranslationResults results, string targetLanguage)
        {
            var t = results.Translations.First(p => p.To.Equals(targetLanguage, StringComparison.InvariantCultureIgnoreCase));

            return new TranslatorResult()
            {
                Text = t.Text,
                Language = t.To,
                DetectedSourceLanguage = results.DetectedLanguage.Language,
                DetectedSourceScore = results.DetectedLanguage.Score,
            };
        }
    }
}
