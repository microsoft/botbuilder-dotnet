﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension action.
    /// </summary>
    public class MessagingExtensionAction : TaskModuleRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionAction"/> class.
        /// </summary>
        /// <param name="data">User input data. Free payload with key-value
        /// pairs.</param>
        /// <param name="context">Current user context, i.e., the current
        /// theme.</param>
        /// <param name="commandId">Id of the command assigned by Bot.</param>
        /// <param name="commandContext">The context from which the command
        /// originates. Possible values include: 'message', 'compose',
        /// 'commandbox'.</param>
        /// <param name="botMessagePreviewAction">Bot message preview action
        /// taken by user. Possible values include: 'edit', 'send'.</param>
        /// <param name="botActivityPreview">A collection of bot activities.</param>
        /// <param name="messagePayload">Message content sent as part of the
        /// command request.</param>
        public MessagingExtensionAction(object data = default, TaskModuleRequestContext context = default, string commandId = default, string commandContext = default, string botMessagePreviewAction = default, IList<Activity> botActivityPreview = default, MessageActionsPayload messagePayload = default)
            : base(data, context)
        {
            CommandId = commandId;
            CommandContext = commandContext;
            BotMessagePreviewAction = botMessagePreviewAction;
            BotActivityPreview = botActivityPreview ?? new List<Activity>();
            MessagePayload = messagePayload;
        }

        /// <summary>
        /// Gets or sets ID of the command assigned by Bot.
        /// </summary>
        /// <value>The ID of the command assigned by the Bot.</value>
        [JsonProperty(PropertyName = "commandId")]
        public string CommandId { get; set; }

        /// <summary>
        /// Gets or sets the context from which the command originates.
        /// Possible values include: 'message', 'compose', 'commandbox'.
        /// </summary>
        /// <value>The context from which the command originates.</value>
        [JsonProperty(PropertyName = "commandContext")]
        public string CommandContext { get; set; }

        /// <summary>
        /// Gets or sets bot message preview action taken by user. Possible
        /// values include: 'edit', 'send'.
        /// </summary>
        /// <value>The bot message preview action taken by the user.</value>
        [JsonProperty(PropertyName = "botMessagePreviewAction")]
        public string BotMessagePreviewAction { get; set; }

        /// <summary>
        /// Gets the bot activity preview.
        /// </summary>
        /// <value>The bot activity preview.</value>
        [JsonProperty(PropertyName = "botActivityPreview")]
        public IList<Activity> BotActivityPreview { get; private set; } = new List<Activity>();

        /// <summary>
        /// Gets or sets message content sent as part of the command request.
        /// </summary>
        /// <value>The message content sent as part of the command request.</value>
        [JsonProperty(PropertyName = "messagePayload")]
        public MessageActionsPayload MessagePayload { get; set; }

        /// <summary>
        /// Gets or sets state parameter passed back to the bot after authentication flow.
        /// </summary>
        /// <value>The state parameter passed back to the bot after authentication flow.</value>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }
    }
}
