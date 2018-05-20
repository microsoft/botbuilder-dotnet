using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    public class TranslatedDocument
    {
        private string sourceMessage;
        private string targetMessage;
        private string rawAlignment;
        private Dictionary<int, int> indexedAlignment;
        private string[] sourceTokens;
        private string[] translatedTokens;

        public string SourceMessage { get => sourceMessage; set => sourceMessage = value; }
        public string TargetMessage { get => targetMessage; set => targetMessage = value; }
        public string RawAlignment { get => rawAlignment; set => rawAlignment = value; }
        public Dictionary<int, int> IndexedAlignment { get => indexedAlignment; set => indexedAlignment = value; }
        public string[] SourceTokens { get => sourceTokens; set => sourceTokens = value; }
        public string[] TranslatedTokens { get => translatedTokens; set => translatedTokens = value; }
    }
}
