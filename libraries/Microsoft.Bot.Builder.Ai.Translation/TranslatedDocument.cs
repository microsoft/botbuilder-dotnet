using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    public class TranslatedDocument
    {
        private string sourceMessage;
        private string targetMessage;
        private string alignment;

        public string SourceMessage { get => sourceMessage; set => sourceMessage = value; }
        public string TargetMessage { get => targetMessage; set => targetMessage = value; }
        public string Alignment { get => alignment; set => alignment = value; }
    }
}
