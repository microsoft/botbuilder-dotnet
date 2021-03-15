// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Enum representing Messaging Extension Attachment Layout types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter), /*camelCase*/ true)]
    public enum MEAttachmentLayoutResponseType
    {
        /// <summary>
        /// List layout type.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        list,

        /// <summary>
        /// Grid layout type.
        /// </summary>
        grid
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
