using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Ai.Translation
{
    /// <summary>
    /// A class to store the state of the translated document before and after the post processing.
    /// </summary>
    public class PostProcessedDocument
    {
        private TranslatedDocument translatedDocument;
        private string postProcessedMessage;

        /// <summary>
        /// Constructor that initializes a post processed document object using the two states.
        /// </summary>
        /// <param name="translatedDocument">Translated object to be post processed</param>
        /// <param name="postProcessedMessage">The result message/translation after the post processing</param>
        public PostProcessedDocument(TranslatedDocument translatedDocument, string postProcessedMessage)
        {
            this.translatedDocument = translatedDocument;
            this.postProcessedMessage = postProcessedMessage;
        }
        public TranslatedDocument TranslatedDocument { get => translatedDocument; set => translatedDocument = value; }
        public string PostProcessedMessage { get => postProcessedMessage; set => postProcessedMessage = value; }
    }
}
