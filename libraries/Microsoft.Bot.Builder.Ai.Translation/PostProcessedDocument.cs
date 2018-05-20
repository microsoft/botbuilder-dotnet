using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    public class PostProcessedDocument
    {
        private TranslatedDocument translatedDocument;
        private string postProcessedMessage;

        public PostProcessedDocument(TranslatedDocument translatedDocument, string postProcessedMessage)
        {
            this.translatedDocument = translatedDocument;
            this.postProcessedMessage = postProcessedMessage;
        }
        public TranslatedDocument TranslatedDocument { get => translatedDocument; set => translatedDocument = value; }
        public string PostProcessedMessage { get => postProcessedMessage; set => postProcessedMessage = value; }
    }
}
