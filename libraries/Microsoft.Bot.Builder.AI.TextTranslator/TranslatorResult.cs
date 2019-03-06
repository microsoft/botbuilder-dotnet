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
    public class TranslatorResult
    {
        public string Text { get; set; }

        public string DetectedSourceLanguage { get; set; }

        public double DetectedSourceScore { get; set; }

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
