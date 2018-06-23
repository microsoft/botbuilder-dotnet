// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Ai.QnA
{
    /// <summary>
    /// Contains options to control the behaviour of the QnA Maker middleware.
    /// Extends the options used on the <see cref="QnAMaker"/> client.
    /// </summary>
    public class QnAMakerMiddlewareOptions : QnAMakerOptions
    {
        /// <summary>
        /// Indicates whether the middleware pipeline short circuits 
        /// when an answer is successfully returned from the QnA Maker 
        /// knowledge base.
        /// 
        /// The default value is false.
        /// </summary>
        public bool EndActivityRoutingOnAnswer { get; set; } = false;

        /// <summary>
        /// If set, this message is sent before sending the top answer.
        /// For example, "I think this answer might help you..."
        /// </summary>
        public string DefaultAnswerPrefixMessage { get; set; }

        /// <summary>
        /// Creates a new <see cref="QnAMakerMiddlewareOptions"/> object.
        /// </summary>
        public QnAMakerMiddlewareOptions()
        {
        }
    }
}
