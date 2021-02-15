// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Enum representing Messaging Extension Result Response types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
    public enum MEResultResponseType
    {
        /// <summary>
        /// Result response type.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        result,

        /// <summary>
        /// Auth response type.
        /// </summary>
        auth,

        /// <summary>
        /// Config response type.
        /// </summary>
        config,

        /// <summary>
        /// Message response type.
        /// </summary>
        message,

        /// <summary>
        /// BotMessagePreview response type.
        /// </summary>
        botMessagePreview
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
