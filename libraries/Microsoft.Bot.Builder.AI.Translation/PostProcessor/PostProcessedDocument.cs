// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.AI.Translation
{
    /// <summary>
    /// A class to store the state of the translated document before and after the post processing.
    /// </summary>
    public class PostProcessedDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostProcessedDocument"/> class using the two states.
        /// </summary>
        /// <param name="translatedDocument">Translated object to be post processed.</param>
        /// <param name="postProcessedMessage">The result message/translation after the post processing.</param>
        public PostProcessedDocument(TranslatedDocument translatedDocument, string postProcessedMessage)
        {
            this.TranslatedDocument = translatedDocument;
            this.PostProcessedMessage = postProcessedMessage;
        }

        public TranslatedDocument TranslatedDocument { get; set; }

        public string PostProcessedMessage { get; set; }
    }
}
