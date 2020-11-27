// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Enum representing Messaging Extension Result Response types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
    public enum MessagingExtensionResultResponseType
    {
        /// <summary>
        /// Result response type.
        /// </summary>
        Result,

        /// <summary>
        /// Auth response type.
        /// </summary>
        Auth,

        /// <summary>
        /// Config response type.
        /// </summary>
        Config,

        /// <summary>
        /// Message response type.
        /// </summary>
        Message,

        /// <summary>
        /// BotMessagePreview response type.
        /// </summary>
        BotMessagePreview
    }
}
