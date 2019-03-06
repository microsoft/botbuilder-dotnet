using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.TextTranslator.MsTranslator
{
    internal class MsTranslatorTranslationResults
    {
        public MsTranslatorDetectedLanguage DetectedLanguage { get; set; }

        public List<MsTranslatorTranslationResult> Translations { get; set; }
    }
}
