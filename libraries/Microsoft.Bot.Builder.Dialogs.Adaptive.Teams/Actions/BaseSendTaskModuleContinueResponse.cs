// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Base class for a url and card Task Module Continue responses.
    /// </summary>
    public abstract class BaseSendTaskModuleContinueResponse : BaseTeamsCacheInfoResponseDialog
    {
        /// <summary>
        /// Gets or sets the optional template or text to use to generate the title message to send.
        /// </summary>
        /// <value>
        /// Message to send.
        /// </value>
        [JsonProperty("title")]
        public ITemplate<string> Title { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the height of the Task Module response.
        /// </summary>
        /// <value>
        /// An integer expression. 
        /// </value>
        [JsonProperty("height")]
        public IntExpression Height { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the width of the Task Module response.
        /// </summary>
        /// <value>
        /// An integer expression. 
        /// </value>
        [JsonProperty("width")]
        public IntExpression Width { get; set; }

        /// <summary>
        /// Gets or sets an optional expression for the Completion Bot Id of the Task Module Task Info response.
        /// This is a bot App ID to send the result of the user's interaction with the task module to. If
        /// specified, the bot will receive a task/submit invoke event with a JSON object in the event payload.
        /// </summary>
        /// <value>
        /// An string expression. 
        /// </value>
        [JsonProperty("completionBotId")]
        public StringExpression CompletionBotId { get; set; }
    }
}
